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
    /// <exception cref="Win32Exception" />
    private static void CheckLastError(bool flag)
    {
        if (flag)
        {
            return;
        }

        var error = Marshal.GetLastPInvokeError();

        if (error is not (NativeConstants.ERROR_SUCCESS or NativeConstants.ERROR_IO_PENDING))
        {
            throw new Win32Exception(error);
        }
    }

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

        CheckLastError(ioctl);

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

        WindowsAsyncState state;

        using var e = new ManualResetEvent(false);

        unsafe
        {
            var overlapped =
                new Overlapped(0, 0, e.SafeWaitHandle.DangerousGetHandle(), null)
                    .Pack(null, null);

            var ioctl = NativeMethods.DeviceIoControl(
                handle, code,
                source.Pointer, (uint)source.Length,
                target.Pointer, (uint)target.Length,
                out var length, overlapped
            );

            try
            {
                CheckLastError(ioctl);

                if (ioctl)
                {
                    return length;
                }
            }
            finally
            {
                if (ioctl)
                {
                    Overlapped.Free(overlapped);
                }
            }

            state = new WindowsAsyncState(handle, overlapped, e, timeout);
        }

        return await state.Source.Task.ConfigureAwait(false);
    }

    [SupportedOSPlatform("windows")]
    private sealed unsafe class WindowsAsyncState
    {
        private readonly SafeFileHandle FileHandle;

        private readonly NativeOverlapped* Overlapped;

        private readonly RegisteredWaitHandle WaitHandle;

        public WindowsAsyncState(
            SafeFileHandle fileHandle, NativeOverlapped* overlapped, WaitHandle waitHandle, TimeSpan timeout)
        {
            ArgumentNullException.ThrowIfNull(fileHandle);
            ArgumentNullException.ThrowIfNull(overlapped);
            ArgumentNullException.ThrowIfNull(waitHandle);

            FileHandle = fileHandle;
            Overlapped = overlapped;
            WaitHandle = ThreadPool.RegisterWaitForSingleObject(waitHandle, Callback, this, timeout, true);
        }

        public TaskCompletionSource<uint> Source { get; } = new();

        private void Callback(object? state, bool timedOut)
        {
            try
            {
                if (timedOut)
                {
                    Source.SetCanceled();
                }
                else
                {
                    var result = NativeMethods.GetOverlappedResult(FileHandle, Overlapped, out var bytes, true);

                    CheckLastError(result);

                    Source.SetResult(bytes);
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
#pragma warning restore CA1031
            {
                Source.SetException(e);
            }
            finally
            {
                WaitHandle.Unregister(null);
            }
        }
    }
}