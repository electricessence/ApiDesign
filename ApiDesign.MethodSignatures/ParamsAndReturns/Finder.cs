namespace ApiDesign.MethodSignatures;

/// <summary>
/// A generic finder that can search through a collection of items.
/// </summary>
public class Finder<T>(Func<T, bool> predicate) : IFind<T>
{
	/// <inheritdoc />
	public IEnumerable<T> FindAll(IEnumerable<T> source)
		=> source.Where(predicate); // Exactly the same as the LINQ implementation.

	// Note about the following method:
	// - Span<T> and ReadOnlySpan<T> cannot be used with await or yield so the return type should be whatever is used to produce the result.
	// - This is also an example of irony as the use of ReadOnlySpan<T> is to avoid allocations, but the implemenation here will allocate.

	/// <inheritdoc />
	public List<T> FindAll(ReadOnlySpan<T> source)
	{
		var list = new List<T>();
		foreach (var e in source)
		{
			if (predicate(e))
			{
				list.Add(e);
			}
		}

		return list;
	}

	IList<T> IFind<T>.FindAll(ReadOnlySpan<T> source) => FindAll(source);

	/// <inheritdoc />
	public bool TryFindFirst(
		IEnumerable<T> source,
		[MaybeNullWhen(false)] out T result)
	{
		foreach (var item in source)
		{
			if (predicate(item))
			{
				result = item;
				return true;
			}
		}

		result = default;
		return false;
	}

	/// <inheritdoc />
	public bool TryFindFirst(ReadOnlySpan<T> source,
		[MaybeNullWhen(false)] out T result)
	{
		foreach (var item in source)
		{
			if (predicate(item))
			{
				result = item;
				return true;
			}
		}

		result = default;
		return false;
	}

}
