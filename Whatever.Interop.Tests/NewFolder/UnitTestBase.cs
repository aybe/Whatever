using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Whatever.Interop.Tests.NewFolder;

public abstract class UnitTestBase
{
    [PublicAPI] public TestContext TestContext { get; set; } = null!;

    protected void Write(object? value = null)
    {
        TestContext.Write(value?.ToString());
    }

    protected void WriteLine(object? value = null)
    {
        TestContext.WriteLine(value?.ToString());
    }

    protected void WriteLineVar(object value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        TestContext.WriteLine($"{paramName}: {value}");
    }
}