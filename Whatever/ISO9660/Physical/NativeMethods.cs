using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Whatever.ISO9660.Physical;

internal static partial class NativeMethods
{
    #region ioapiset.h

    /// <summary>
    ///     https://learn.microsoft.com/en-us/windows/win32/api/ioapiset/nf-ioapiset-deviceiocontrol
    /// </summary>
    [return: MarshalAs(UnmanagedType.Bool)]
    [LibraryImport("kernel32.dll", SetLastError = true)]
    public static unsafe partial bool DeviceIoControl(
        SafeFileHandle hDevice,
        uint dwIoControlCode,
        [Optional] nint lpInBuffer,
        uint nInBufferSize,
        [Optional] nint lpOutBuffer,
        uint nOutBufferSize,
        out uint lpBytesReturned,
        [Optional] NativeOverlapped* lpOverlapped);

    /// <summary>
    ///     https://learn.microsoft.com/en-us/windows/win32/api/ioapiset/nf-ioapiset-getoverlappedresult
    /// </summary>
    [return: MarshalAs(UnmanagedType.Bool)]
    [LibraryImport("kernel32.dll", SetLastError = true)]
    public static unsafe partial bool GetOverlappedResult(
        SafeFileHandle hFile,
        NativeOverlapped* lpOverlapped,
        out uint lpNumberOfBytesTransferred,
        [MarshalAs(UnmanagedType.Bool)] bool bWait
    );

    #endregion
}