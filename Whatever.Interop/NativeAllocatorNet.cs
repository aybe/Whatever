using System;
using System.Runtime.InteropServices;

namespace Whatever.Interop
{
    public sealed unsafe class NativeAllocatorNet
        : NativeAllocator<NativeAllocatorNet>
    {
        public override T* Alloc<T>(int elementCount)
        {
            if (elementCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(elementCount), elementCount, null);
            }

            var elementSize = NativeHelper.SizeOf<T>();

            var byteCount = elementCount * elementSize;

            var pointer = Marshal.AllocHGlobal(byteCount);

            return (T*)pointer;
        }

        public override void* Alloc(int byteCount)
        {
            if (byteCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(byteCount), byteCount, null);
            }

            var pointer = Marshal.AllocHGlobal(byteCount);

            return (void*)pointer;
        }

        public override void Clear(void* pointer, int byteCount)
        {
            if (byteCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(byteCount));
            }

            new Span<byte>(pointer, byteCount).Clear();
        }

        public override void Free(void* pointer)
        {
            Marshal.FreeHGlobal(new IntPtr(pointer));
        }
    }
}