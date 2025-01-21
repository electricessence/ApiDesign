namespace ApiDesign.MethodSignatures;

/// <summary>
/// Represents a URL or a URL string.
/// </summary>
/// <remarks>
/// Helps validate URLs and avoid redundant parsing.
/// </remarks>
public readonly record struct UriOrUrlString
{
	/// <summary>
	/// Initializes a new instance of the <see cref="UriOrUrlString"/> struct.
	/// </summary>
	/// <param name="uri">The <see cref="Uri"/> to use.</param>
	public UriOrUrlString(Uri uri)
		=> (Uri, String) = (uri, uri.ToString());

	/// <inheritdoc cref="UriOrUrlString(Uri)" />
	/// <param name="url">The <see cref="string"/> to use as a URL.</param>
	/// <exception cref="UriFormatException">Thrown when <paramref name="url"/> is not a valid URL.</exception>"
	public UriOrUrlString([StringSyntax("Uri")] string url)
		=> (Uri, String) = (new Uri(url), url);

	/// <summary>
	/// Gets the <see cref="Uri"/> value.
	/// </summary>
	public Uri Uri { get; }

	/// <summary>
	/// Gets the <see cref="string"/> value.
	/// </summary>
	public string String { get; }

	/// <inheritdoc />
	public override int GetHashCode() => String.GetHashCode();

	/// <inheritdoc />
	public override string ToString() => String;
	
	public static implicit operator Uri(UriOrUrlString urlOrUrlString) => urlOrUrlString.Uri;

	public static implicit operator UriOrUrlString([StringSyntax("Uri")] Uri uri) => new(uri);
	public static implicit operator UriOrUrlString(string url) => new(url);
}
