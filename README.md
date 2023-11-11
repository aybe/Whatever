# Whatever

Things I eventually got tired to rewrite over and over in different projects.

Just like a flea market, random things next to totally unrelated stuff.

## Whatever.Extensions

Mostly extension methods, few base classes and helpers.

https://www.nuget.org/packages/Whatever.Extensions/


| File | Description |
| - | - |
| ConvertExtensions.cs | `To[[S]Byte\|[U]Int[16\|32\|64]\|Single\|Double\|Decimal]()`, instead of `Convert.To...()`. |
| DictionaryExtensions.cs | `IDictionary.GetOrAdd(..., Func<TKey> valueFactory)`, to mimic `ConcurrentDictionary`. |
| Disposable.cs | `IDisposable` base class. |
| DisposableAsync.cs | `IDisposableAsync` base class. |
| EnumExtensions.cs | `Enum.HasFlags<T>(...)`, type-safe enum flag-checking.|
| QuantizedProgress.cs | `IProgress<T>` that doesn't report unnecessarily. |
| SharedBuffer.cs | `Span<T>` alternative for async scenarios + `As[Array\|[[ReadOnly][Memory\|Span]]]()`. |
| Singleton.cs | Generic singleton base class. |
| SpanMemoryManager.cs | `MemoryManager<T>` implementation for `Span<T>` as `Memory<T>` in async scenarios. |
| StreamExtensions.cs |`[Get\|Set]Endianness[Scope]`, gets/sets endianness for endian-aware stuff. |
| |`Read<T>[Async](endianness?)`, for reading primitive types. |
| |`ReadExactly[Async](...)`, missing in .NET Standard 2.1. |
| |`ReadStringAscii[Async]`, because it never gets old. |
| TypeExtensions.cs | `GetNiceName`, friendlier version of `Type.ToString()`.|