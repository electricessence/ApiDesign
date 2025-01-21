namespace ApiDesign.MethodSignatures;

public interface IPagedResult<T, TPage>
	where TPage : IEnumerable<T>
{
	TPage Page { get; }
	int Index { get; }
	int Total { get; }
}

/// <summary>
/// Flexible page result that can be used with any collection type.
/// </summary>

public record PagedResult<T, TPage>(TPage Page, int Index, int Total)
	where TPage : IEnumerable<T>;

/// <summary>
/// Simple paged result that uses a <see cref="ICollection{T}"/>.
/// </summary>
public record PagedResult<T>
	: PagedResult<T, ICollection<T>>
{
	public PagedResult(ICollection<T> page, int index, int total)
		: base(page, index, total) { }
}

/// <summary>
/// Base class for all read-only paged results.
/// </summary>
public abstract record ReadOnlyPagedResultBase<T, TPage>
	: PagedResult<T, TPage>
	where TPage : IReadOnlyCollection<T>
{
	protected ReadOnlyPagedResultBase(TPage page, int index, int total)
		: base(page, index, total) { }
}

/// <summary>
/// Simple read-only paged result that uses a <see cref="IReadOnlyCollection{T}"/>.
/// </summary>
public record ReadOnlyPagedResult<T>
	: ReadOnlyPagedResultBase<T, IReadOnlyCollection<T>>
{
	public ReadOnlyPagedResult(IReadOnlyCollection<T> page, int index, int total)
		: base(page, index, total) { }
}

/// <summary>
/// Simple read-only paged result that uses a <see cref="IReadOnlyList{T}"/>.
/// </summary>
public record ReadOnlyListPagedResult<T>
	: ReadOnlyPagedResultBase<T, IReadOnlyList<T>>
{
	public ReadOnlyListPagedResult(IReadOnlyList<T> page, int index, int total)
		: base(page, index, total) { }
}

/// <summary>
/// Simple read-only paged result that uses a <see cref="IReadOnlyCollection{T}"/>.
/// </summary>
/// <remarks>
/// Should be used in scenarios where pages are cached or shared to avoid any risk of mutation.
/// </remarks>
public record ImmutablePagedResult<T>
	: ReadOnlyPagedResultBase<T, ImmutableArray<T>>
{
	public ImmutablePagedResult(ImmutableArray<T> page, int index, int total)
		: base(page, index, total) { }
}