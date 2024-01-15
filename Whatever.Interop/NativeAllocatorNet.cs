using System;
using System.Runtime.InteropServices;

namespace Whatever.Interop
{
    public sealed unsafe class NativeAllocatorNet
        : NativeAllocator<NativeAllocatorNet>
    {
        public override T* Alloc<T>(int elementCount)
        {
            var elementSize = NativeHelper.SizeOf<T>();

            var byteCount = elementCount * elementSize;

            var pointer = Marshal.AllocHGlobal((int)byteCount);

            return (T*)pointer;
        }

        public override void Clear(void* pointer, uint byteCount)
        {
            new Span<byte>(pointer, (int)byteCount).Clear();
        }

        public override void Free(void* pointer)
        {
            Marshal.FreeHGlobal(new IntPtr(pointer));
        }
    }
}