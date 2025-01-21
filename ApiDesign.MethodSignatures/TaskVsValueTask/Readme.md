# Async API Design: Task vs ValueTask

Examples and guidance for designing APIs with asynchronous methods, focusing on the nuanced choice between `Task` and `ValueTask`. Includes scenarios where each type is appropriate, with clear recommendations and real-world examples.

## TL;DR

| **Scenario**                                    | **Task** | **ValueTask** | **Reason**                                                                                           |
|-------------------------------------------------|----------|---------------|------------------------------------------------------------------------------------------------------|
| If unsure                                       | ✔        |               | Simpler, safer default for maintainable and compatible APIs.                                         |
| Operation requires multiple awaits              | ✔        |               | Tasks can be awaited multiple times, unlike `ValueTask`.                                             |
| Potentially cached asynchronous calls           | ✔        |               | Commonly `Task<T>` is used as an asynchronous container that can have multiple subscribers.          |
| High-throughput, synchronous completion likely  |          | ✔             | Reduces heap allocation overhead when methods frequently complete synchronously (e.g., cache hits).  |
| Private or local helper methods                 |          | ✔             | Allows optimization for internal operations guaranteed to be awaited only once.                      |
| Specialized operations (single-await)           |          | ✔             | Optimized for scenarios where the method is awaited exactly once and doesn't involve reuse.          |

## More Reading
- [Understanding the Whys, Whats, and Whens of ValueTask](https://devblogs.microsoft.com/dotnet/understanding-the-whys-whats-and-whens-of-valuetask/) by Stephen Toub
- [Microsoft Docs: Task and ValueTask](https://learn.microsoft.com/en-us/dotnet/standard/memory-and-spans/valuetask)
- [Microsoft Docs: Asynchronous Programming](https://learn.microsoft.com/en-us/dotnet/csharp/async)


## Examples

These are just examples. In each case, there may not be a need for this as modern .NET may have these use cases solved.

---

### Cached Results

It is quite common for optimizing an asynchronous cache to store the original `Task<T>` that was used to make the underlying call
so it can be shared immediately upon creation.
That task may be evicted if it fails,
but while in-flight and after completion can still handle multiple subscribers.

```csharp
protected override Task<ImmutableArray<byte>> GetContentAsync(UriOrUrlString key)
    => httpClient
    .GetAsync(key) // Task<HttpResponseMessage>
    .EnsureSuccessContent() // ValueTask<HttpContent>
    .ReadAsImmutableBytesAsync(clearSharedBufferAfterFetch) // ValueTask<ImmutableArray<byte>>
    .AsTask();
```

In the above example (found in `CachedHttpFetcher.cs`) some of the methods return `ValueTask<ImmutableArray<byte>>` but the final result is converted to `Task<ImmutableArray<byte>>`
to allow for multiple subscribers. This provides the best of both worlds where intermediate steps are processed without additional heap allocation but the final result is cachable and can handle repeat subscriptions.

---

### High-throughput, synchronous completion likely

Use `ValueTask` for operations where synchronous completion is common.

The best example of this is with `System.Threading.Channels`
where methods like `channel.Writer.WriteAsync(T, CancellationToken)` can process synchronously if the channel is not full.
And conversely, `channel.Reader.ReadAsync(CancellationToken)` can process synchronously if there is data available.

---

### Private or local helper methods

Use `ValueTask` for private or local helpers that are always awaited exactly once.


In the following examples, we avoid unnecessary heap allocation by using `ValueTask<int>` for the helper methods.


#### Private helper method
```csharp
private static async ValueTask<int> GetIntAsync<T>(T package)
{
    // Perform some asynchronous operation.
}

public async Task<int> GetIntAsync()
{
    var bytes = new List<byte>();
    // Do some processing and preparation...
    return await GetIntAsync(bytes);
}
```

#### Local helper method
```csharp
public async Task<int> GetIntAsync()
{
    var bytes = new List<byte>();
    // Do some processing and preparation...
    return await GetIntAsync(bytes);

    async ValueTask<int> GetIntAsync<T>(T package)
    {
        // Perform some asynchronous operation.
    }
}
```
