namespace ApiDesign.MethodSignatures;

public abstract class CachedContentBase<TKey>
{
	// Concurrent dictionaries are optimistic
	// Using a lazy allows us to ensure that the request is only made once.
	// Using an ImmutableArray<byte> ensures the caller can't modify the reused/shared content.
	private readonly ConcurrentDictionary<string, Lazy<Task<ImmutableArray<byte>>>> _cache;

	// Allow looking up the cache with a ReadOnlySpan<char> (URL fragment) to avoid allocations.
	private readonly ConcurrentDictionary<string, Lazy<Task<ImmutableArray<byte>>>>.AlternateLookup<ReadOnlySpan<char>> _cacheLookup;

	protected CachedContentBase(bool ignoreCase) {
		_cache = ignoreCase ? new(StringComparer.OrdinalIgnoreCase) : new();
		// Alternatate lookup will be case sensitive.
		_cacheLookup = _cache.GetAlternateLookup<ReadOnlySpan<char>>();
	}

	// Note: Multiple overloads are provided her to avoid branching.

	/// <summary>
	/// Attempts to get the existing value from the cache.
	/// </summary>
	public bool TryGetExisting(
		string key,
		[MaybeNullWhen(false)] out Task<ImmutableArray<byte>>? value)
	{
		if (_cache.TryGetValue(key, out var lazy))
		{
			value = lazy.Value;
			return true;
		}

		value = null;
		return false;
	}

	/// <remarks>Allows for using URLs from other strings or buffers instead of having to create a new one.</remarks>
	/// <inheritdoc cref="TryGetExisting(string, out Task{ImmutableArray{byte}}?)"/>
	public bool TryGetExisting(
		ReadOnlySpan<char> key,
		[MaybeNullWhen(false)] out Task<ImmutableArray<byte>>? value)
	{
		if (_cacheLookup.TryGetValue(key, out var lazy))
		{
			value = lazy.Value;
			return true;
		}

		value = null;
		return false;
	}

	public Task<ImmutableArray<byte>> GetAsync(TKey key)
		=> GetAsync(GetCacheKey(key), key);

	private Task<ImmutableArray<byte>> GetAsync(string cacheKey, TKey key)
	{
		var lazy = _cache.GetOrAdd(cacheKey, k => new(() =>
		{
			// The following will only be invoked once when the .Value property is accessed.
			var task = GetContentAsync(key);
			// Don't cache failed results.
			// If the task is faulted, remove it from the cache.
			// There is a rare case where it may have been removed already but this is highly unlikely so need to check or synchronize.
			task.ContinueWith(t => _cache.TryRemove(k, out _), TaskContinuationOptions.NotOnRanToCompletion);
			return task;
		}));

		try
		{
			return lazy.Value;
		}
		catch
		{
			// If the value is faulted, evict it from the cache.
			// And like above, there's a rare case, but not worth optimizing for.
			_cache.TryRemove(cacheKey, out _);
			throw;
		}
	}

	public bool Evict(string cacheKey)
		=> _cache.TryRemove(cacheKey, out _);

	public bool Evict(ReadOnlySpan<char> cacheKey)
		=> _cacheLookup.TryRemove(cacheKey, out _);

	protected abstract string GetCacheKey(TKey key);

	protected abstract Task<ImmutableArray<byte>> GetContentAsync(TKey key);
}
