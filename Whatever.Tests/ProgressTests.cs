using JetBrains.Annotations;
using Whatever.Progress;

namespace Whatever.Tests;

[TestClass]
public sealed class ProgressTests
{
    [UsedImplicitly] public required TestContext TestContext { get; set; }

    [TestMethod]
    public void TestSparseProgress()
    {
        // you have a lengthy operation with many steps, say 1 million of them
        // obviously, you cannot report them each as that would kill the UI/UX
        // where this comes handy is that you can use any type for the progress
        // i.e. you're not tied to use IProgress<double> or IProgress<float>
        // your callbacks receive progress snapped to number of digits specified

        var myCustomProgress = new MyCustomProgress { Logger = TestContext.WriteLine };

        var sparseProgress = new SparseProgress<MyCustomProgress>(ProgressGetter, ProgressSetter)
        {
            Digits = 0, Synchronous = true // 1% increments, console use
        };

        sparseProgress.ProgressChanged += ProgressChanged;

        const int steps = 1_000_000; // an operation with 1M steps

        for (var i = 0; i < steps; i++)
        {
            // snaps progress according progress digits, calls user code (progress setter)
            sparseProgress.Update(ref myCustomProgress, i, steps);

            // report as usual with IProgress<T>.Report(T)
            sparseProgress.Report(myCustomProgress);
        }

        var expected = 100 * (int)Math.Pow(10, sparseProgress.Digits);

        Assert.AreEqual(expected, myCustomProgress.Counter);
    }

    private static void ProgressChanged(object? sender, SparseProgressEventArgs<MyCustomProgress> e)
    {
        // present the changed progress any way you have to

        var progress = e.Value;

        progress.Logger(
            $"Counter: {progress.Counter}, Percentage: {progress.Percentage:P}, Message: {progress.Message}");

        // for test assertion
        progress.Counter += 1;
    }

    private static double ProgressGetter(ref MyCustomProgress progress)
    {
        return progress.Percentage; // get progress from type
    }

    private static void ProgressSetter(ref MyCustomProgress progress, double value)
    {
        // set progress in type

        progress.Percentage = value; // value has been snapped to N digits

        // do any extra stuff

        progress.Message = value switch
        {
            < 0.1d => "Starting...",
            < 0.5d => "Working...",
            < 1.0d => "Almost done...",
            _ => "Done!"
        };
    }

    [TestMethod]
    public void TestTextProgressBar()
    {
        var bar = new TextProgressBar
        {
            Options = // customize
            {
                Width = 20
            }
        };

        const int steps = 10;

        for (var i = 0; i < steps; i++)
        {
            bar.Update((i + 1.0) / steps); // normalized value

            // TODO optionally add stuff to string builder

            var text = bar.Builder.ToString();

            bar.Clear(); // clear builder for next step

            TestContext.WriteLine(text);

            var expected = i switch
            {
                0 => "██░░░░░░░░░░░░░░░░░░ 10.00 %",
                1 => "████░░░░░░░░░░░░░░░░ 20.00 %",
                2 => "██████░░░░░░░░░░░░░░ 30.00 %",
                3 => "████████░░░░░░░░░░░░ 40.00 %",
                4 => "██████████░░░░░░░░░░ 50.00 %",
                5 => "████████████░░░░░░░░ 60.00 %",
                6 => "██████████████░░░░░░ 70.00 %",
                7 => "████████████████░░░░ 80.00 %",
                8 => "██████████████████░░ 90.00 %",
                9 => "████████████████████ 100.00 %",
                _ => throw new NotImplementedException()
            };

            Assert.AreEqual(expected, text);
        }
    }

    private sealed class MyCustomProgress
    {
        public int Counter;
        public Action<string?> Logger = null!;
        public string? Message;
        public double Percentage;
    }
}