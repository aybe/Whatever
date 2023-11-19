# Whatever

Things I eventually got tired to rewrite over and over in different projects.

Just like a flea market, random things next to totally unrelated stuff.

## Whatever.Extensions

Mostly extension methods, few base classes and helpers.

https://www.nuget.org/packages/Whatever.Extensions/

### Types

| Type | Description |
| - | - |
| ConvertExtensions | `To[[S]Byte\|[U]Int[16\|32\|64]\|Single\|Double\|Decimal]()` instead of `Convert.To...`. |
| DictionaryExtensions | `IDictionary.GetOrAdd(..., Func<TKey> valueFactory)`. |
| Disposable | `IDisposable` base class. |
| DisposableAsync | `IDisposableAsync` base class. |
| EnumExtensions | `Enum.HasFlags<T>(...)`, type-safe enum flag-checking.|
| SharedBuffer | `Span<T>` alternative for async scenarios. |
| Singleton | Generic singleton base class. |
| SpanMemoryManager | `MemoryManager<T>` for `Span<T>` as `Memory<T>` in async scenarios. |
| SparseProgress | `IProgress<T>` with granularity so it doesn't overwhelm consumers. |
| StreamExtensions |`[Get\|Set]Endianness[Scope]`, gets/sets endianness for endian-aware stuff. |
| |`[Read\|Write]<T>[Async](endianness?)`, for reading/writing unmanaged types. |
| |`ReadExactly[Async](...)` for .NET Standard 2.1. |
| |`[Read\|Write]StringAscii[Async]`, because it never gets old. |
| TextProgressBar | Text-mode progress bar, e.g. `████████████░░░░░░░░░░░░░░░░░░ 42.86%`.|
| TypeExtensions | `GetNiceName`, friendlier version of `Type.ToString()`.|

### Templates

| Name | Description |
| - | - |
| Default.csproj | NuGet package, SourceLink, versioning, code analysis/style.  |

