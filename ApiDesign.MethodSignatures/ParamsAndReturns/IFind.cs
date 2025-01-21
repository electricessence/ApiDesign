namespace ApiDesign.MethodSignatures;
public interface IFind<T>
{
	/// <summary>
	/// Finds all elements that match the predicate.
	/// </summary>
	/// <param name="source">The source to search through.</param>
	/// <returns>An enumerable that yields results.</returns>
	IEnumerable<T> FindAll(IEnumerable<T> source);

	/// <inheritdoc cref="FindAll(IEnumerable{T})"/>
	IList<T> FindAll(ReadOnlySpan<T> source);

	/// <summary>
	/// Attempts to find the first element that matches the predicate.
	/// </summary>
	/// <param name="source">The source to search through.</param>
	/// <param name="result">The value found.</param>
	/// <param name="predicate">The predicate to use for comparison.</param>
	/// <returns><see langword="true"/> if found; otherwise <see langword="false"/>.</returns>
	bool TryFindFirst(IEnumerable<T> source, [MaybeNullWhen(false)] out T result);

	/// <inheritdoc cref="TryFindFirst(IEnumerable{T}, out T, Func{T, bool})"/>
	bool TryFindFirst(ReadOnlySpan<T> source, [MaybeNullWhen(false)] out T result);
}

public static class FindExtensions
{
	/// <summary>Finds the first element that matches the predicate.</summary>
	/// <remarks>Returns the default value if the predicated never finds a match.</remarks>
	public static T? FindFirstOrDefault<T>(
		this IFind<T> finder,
		IEnumerable<T> source)
		=> finder.TryFindFirst(source, out var result)
			? result
			: default;

	/// <remarks>Throws an <see cref="InvalidOperationException"/> if no element is found.</remarks>
	/// <exception cref="InvalidOperationException">If the <paramref name="predicate"/> never finds a match.</exception>
	/// <inheritdoc cref="FindFirstOrDefault{T}(IFind{T}, IEnumerable{T})" />
	public static T FindFirst<T>(
		this IFind<T> finder,
		IEnumerable<T> source)
		=> finder.TryFindFirst(source, out var result)
			? result
			: throw new InvalidOperationException("No matching element found.");
}
