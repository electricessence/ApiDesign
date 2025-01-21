namespace ApiDesign.MethodSignatures;

public static class HttpExtensions
{
	/// <summary>
	/// Returns the content if the response has a success code. Otherwise throws an <see cref="HttpRequestException"/>/
	/// </summary>
	/// <param name="response">The task that contains the <see cref="HttpRequestMessage"/>.</param>
	/// <exception cref="HttpRequestException">Thrown when the response does not have a success code.</exception>
	public static async ValueTask<HttpContent> EnsureSuccessContent(
		this Task<HttpResponseMessage> response)
	{
		var r = await response.ConfigureAwait(false);
		r.EnsureSuccessStatusCode();
		return r.Content;
	}

	/// <summary>
	/// Returns the content as an <see cref="ImmutableArray{T}"/> of <see cref="byte"/>.
	/// </summary>
	public static async ValueTask<ImmutableArray<byte>> ReadAsImmutableBytesAsync(
		this HttpContent content,
		bool clearSharedBufferAfterFetch = false)
	{
		// Instead of ReadAsByteArrayAsync we will use ReadAsStreamAsync and build up an ImmutableArray<byte> from the stream.
		var builder = ImmutableArray.CreateBuilder<byte>();
		using var lease = MemoryPool<byte>.Shared.Rent(4096);
		try
		{
			await using var stream = await content.ReadAsStreamAsync().ConfigureAwait(false);
			int bytesRead;
			while ((bytesRead = await stream.ReadAsync(lease.Memory).ConfigureAwait(false)) > 0)
			{
				builder.AddRange(lease.Memory.Span.Slice(0, bytesRead));
			}
		}
		finally
		{
			// Clear the memory before returning it to the pool.
			if (clearSharedBufferAfterFetch)
			{
				lease.Memory.Span.Clear();
			}
		}

		return builder.MoveToImmutable();
	}

	/// <summary>
	/// Awaits the content and returns the content as an <see cref="ImmutableArray{T}"/> of <see cref="byte"/>.
	/// </summary>
	public static async ValueTask<ImmutableArray<byte>> ReadAsImmutableBytesAsync(
		this ValueTask<HttpContent> contentAsync,
		bool clearSharedBufferAfterFetch = false)
	{
		var content = await contentAsync.ConfigureAwait(false);
		return await content
			.ReadAsImmutableBytesAsync(clearSharedBufferAfterFetch)
			.ConfigureAwait(false);
	}
}
