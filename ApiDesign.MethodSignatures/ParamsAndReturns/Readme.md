# Method Signatures: Parameters and Return Values

As general guidence for designing APIs, the types used in the parameters should be as broad as possible, while the return values should be as specific as possible.

## Parameters

Guidance here is generally simple as it is best to accept the broadest type possible for parameters with specialized overloads when justified for performance.

```csharp
public void DoSomething(IEnumerable<int> values) { }
```

For the above example, by specifying the paramerter type as `IEnumerable<int>`,
the method can accept any type that implements `IEnumerable<int>`.
This allows for a wide range of types to be passed in, including arrays, lists, and other collections.

If there are performance reasons to specialize the parameter, then overloads can be used.

## Return Types

This is where things get a bit tricky and it's best to show more than tell.
See the files contained in this folder for more detailed examples.

### Simple or Value Types

General guidance here is to just use the type directly.
See `FindExtensions.cs` for exact examples.

The implementation of the `FindFirst<T>` extension is simple
and returns a single item from the collection
and will throw if not found.
Similar to LINQ's `.First()` extension.

### Collections

But when dealing with collections,
it depends on what the intention is
and what the method has actually done.

In the `Finder.FindAll(IEnumerable<T> source)` example,
the method returns `IEnumerable<T>` because it is returning a collection of items that may be lazily evaluated
and therefore avoid unnecessary allocations as the caller may try to apply more filtering to the results.

But in a twist of fate, the `Finder.FindAll(ReadOnlySpan<T> source)` method
returns a `List<T>` as that is the only way to produce a filtered result.

> There are other ways to optimize for memory,
but for our example `List<T>` is the most straightforward.

Instead of returning an `IList<T>` as the interface suggests,
we return the concrete type `List<T>`
and add the explicit implementation of `IList<T>`.

This results in not having to recast the result
if the caller has a direct reference to the `Finder<T>` class
and it functionally satisfies the interface.

#### PagedResult Examples

In the `PagedResult<T>` examples,
it is clear that a collection type (not `IEnumerable<T>`) will be necessary
as the caller will need the count of items.

In order to setup the class inheritance, we start with a constraint of `IEnumerable<T>`,
but we don't start seeing the benefits until we use more specific types.

`PagedResult<T, TPage>` is highly flexible and is essentially the base class for all paged results.

It then becomes easy to define either a mutable or immutable page types.

In cases like this, it should be obvious that the return type should be as specific as possible.

#### Creating Enumerables

Whenever possible, use the `yield` keyword to create enumerables instead of building a list.

```csharp
public IEnumerable<T> SpecialFilter(IEnumerable<T> source)
{
	foreach(var e in source)
	{
		if(MeetsCriteria(e))
		{
			yield return e;
		}
	}
}
```

Using the `yield` keyword is one of the easiest ways to keep your code simple and efficient.

1. It allows for lazy evaluation.
2. It avoids unnecessary allocations.
3. Allows the caller to decide how they want to materialize the results.
4. Highly complex filtering can be done without creating a new collection.