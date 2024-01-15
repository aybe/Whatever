using System;
using System.Collections.Generic;

namespace Whatever.Interop
{
    public static unsafe class NativeBuffer
    {
        private static readonly Dictionary<IntPtr, NativeAllocator> Dictionary =
            new Dictionary<IntPtr, NativeAllocator>();

        public static void Register(void* pointer, NativeAllocator allocator)
        {
            Dictionary.Add(new IntPtr(pointer), allocator);
        }

        public static void Dispose(void* pointer)
        {
            var key = new IntPtr(pointer);

            var allocator = Dictionary[key];

            allocator.Free(pointer);

            Dictionary.Remove(key);
        }
    }
}