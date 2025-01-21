namespace ApiDesign.MethodSignatures;

/// <summary>
/// A cached HTTP fetcher that uses an <see cref="HttpClient"/> to fetch content.
/// </summary>
/// <param name="httpClient">The <paramref name="httpClient"/> to use when requesting data.</param>
/// <param name="clearSharedBufferAfterFetch">
/// When <see langword="true"/> will cause the content reusable stream buffer
/// to be cleaned when the stream if finished.
/// </param>
public class CachedHttpFetcher(HttpClient httpClient, bool clearSharedBufferAfterFetch)
	: CachedContentBase<UriOrUrlString>(false)
{
	/// <summary>
	/// Initializes a new instance of the <see cref="CachedHttpFetcher"/> class.
	/// </summary>
	public CachedHttpFetcher(HttpClient httpClient) : this(httpClient, false) { }

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected override string GetCacheKey(UriOrUrlString key) => key.String;

	protected override Task<ImmutableArray<byte>> GetContentAsync(UriOrUrlString key)
		=> httpClient
		.GetAsync(key)
		.EnsureSuccessContent()
		.ReadAsImmutableBytesAsync(clearSharedBufferAfterFetch)
		.AsTask();
}
