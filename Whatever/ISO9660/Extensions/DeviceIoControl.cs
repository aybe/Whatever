using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using JetBrains.Annotations;
using Microsoft.Win32.SafeHandles;
using Whatever.ISO9660.Physical;

namespace Whatever.ISO9660.Extensions;

public static class DeviceIoControl
{
    [MustUseReturnValue]
    [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
    public static uint Send<TSource, TTarget>(
        SafeFileHandle handle, uint code, NativeMarshaller<TSource> source, NativeMarshaller<TTarget> target
    )
        where TSource : struct
        where TTarget : struct
    {
        if (OperatingSystem.IsWindows())
        {
            return SendWindows(handle, code, source, target);
        }

        throw new PlatformNotSupportedException();
    }

    [SupportedOSPlatform("windows")]
    private static unsafe uint SendWindows<TSource, TTarget>(
        SafeFileHandle handle,
        uint code,
        NativeMarshaller<TSource> source,
        NativeMarshaller<TTarget> target
    )
        where TSource : struct
        where TTarget : struct
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        var ioctl = NativeMethods.DeviceIoControl(
            handle, code, source.Pointer, (uint)source.Length, target.Pointer, (uint)target.Length, out var length, null
        );

        if (ioctl is false && Marshal.GetLastPInvokeError() is var error and not NativeConstants.ERROR_SUCCESS)
        {
            throw new Win32Exception(error);
        }

        return length;
    }

    [MustUseReturnValue]
    [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
    public static async Task<uint> SendAsync<TSource, TTarget>(
        SafeFileHandle handle,
        uint code,
        NativeMarshaller<TSource> source,
        NativeMarshaller<TTarget> target,
        TimeSpan timeout
    )
        where TSource : struct
        where TTarget : struct
    {
        if (OperatingSystem.IsWindows())
        {
            return await SendWindowsAsync(handle, code, source, target, timeout).ConfigureAwait(false);
        }

        throw new PlatformNotSupportedException();
    }

    [SupportedOSPlatform("windows")]
    private static async Task<uint> SendWindowsAsync<TSource, TTarget>(
        SafeFileHandle handle,
        uint code,
        NativeMarshaller<TSource> source,
        NativeMarshaller<TTarget> target,
        TimeSpan timeout
    )
        where TSource : struct
        where TTarget : struct
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        var tcs = new TaskCompletionSource<uint>();

        using var @event = new ManualResetEvent(false);

        using var result = SendWindowsAsync(handle, code, source, target, timeout, tcs, @event);

        if (result.Handle is null)
        {
            return result.Length;
        }

        var error = Marshal.GetLastWin32Error();

        if (error is not (NativeConstants.ERROR_SUCCESS or NativeConstants.ERROR_IO_PENDING))
        {
            throw new Win32Exception(error);
        }

        var bytes = await tcs.Task.ConfigureAwait(false);

        return bytes;
    }

    [SupportedOSPlatform("windows")]
    private static unsafe Result SendWindowsAsync<TSource, TTarget>(
        SafeFileHandle handle,
        uint code,
        NativeMarshaller<TSource> src,
        NativeMarshaller<TTarget> tgt,
        TimeSpan timeout,
        TaskCompletionSource<uint> tcs,
        WaitHandle evt
    )
        where TSource : struct
        where TTarget : struct
    {
        var overlapped =
            new Overlapped(0, 0, evt.SafeWaitHandle.DangerousGetHandle(), null)
                .Pack(null, null);

        var ioctl = NativeMethods.DeviceIoControl(
            handle, code, src.Pointer, (uint)src.Length, tgt.Pointer, (uint)tgt.Length, out var len, overlapped
        );

        var rwh = default(RegisteredWaitHandle?);

        if (ioctl)
        {
            Overlapped.Free(overlapped);
        }
        else
        {
            rwh = ThreadPool.RegisterWaitForSingleObject(
                evt, Callback, new WindowsState(handle, overlapped, tcs), timeout, true
            );
        }

        return new Result(rwh, len);
    }

    private static unsafe void Callback(object? state, bool timedOut)
    {
        if (state is not WindowsState ws)
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }

        var tcs = ws.Source;

        try
        {
            if (timedOut)
            {
                tcs.SetCanceled();
            }
            else
            {
                var result = NativeMethods.GetOverlappedResult(ws.Handle, ws.Overlapped, out var length, true);

                if (result is false)
                {
                    throw new Win32Exception();
                }

                tcs.SetResult(length);
            }
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception e)
#pragma warning restore CA1031
        {
            tcs.SetException(e);
        }
    }

    private readonly struct Result(RegisteredWaitHandle? handle, uint length) : IDisposable
    {
        public readonly RegisteredWaitHandle? Handle = handle;

        public readonly uint Length = length;

        public void Dispose()
        {
            Handle?.Unregister(null);
        }
    }

    [SupportedOSPlatform("windows")]
    private readonly unsafe struct WindowsState(
        SafeFileHandle handle,
        NativeOverlapped* overlapped,
        TaskCompletionSource<uint> source)
    {
        public readonly SafeFileHandle Handle = handle;

        public readonly NativeOverlapped* Overlapped = overlapped;

        public readonly TaskCompletionSource<uint> Source = source;
    }
}