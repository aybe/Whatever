using System;

namespace Whatever.Interop
{
    public abstract unsafe class NativeAllocator
    {
        public static NativeAllocator Default { get; } = NativeAllocatorNet.Instance; // TODO #if UNITY

        public virtual T* Alloc<T>(int elementCount)
            where T : unmanaged
        {
            if (elementCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(elementCount), elementCount, null);
            }

            var sizeOf = NativeHelper.SizeOf<T>();

            var byteCount = elementCount * sizeOf;

            var pointer = Alloc(byteCount);

            return (T*)pointer;
        }

        public abstract void* Alloc(int byteCount);

        public abstract void Clear(void* pointer, int byteCount);

        public void Clear<T>(T* pointer, int elementCount)
            where T : unmanaged
        {
            if (elementCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(elementCount), elementCount, null);
            }

            var sizeOf = NativeHelper.SizeOf<T>();

            var byteCount = elementCount * sizeOf;

            Clear((void*)pointer, byteCount);
        }

        public abstract void Free(void* pointer);
    }

    public abstract class NativeAllocator<T>
        : NativeAllocator
        where T : NativeAllocator<T>, new()
    {
        public static T Instance { get; } = new T();
    }
}