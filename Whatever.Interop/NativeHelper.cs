using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Whatever.Interop
{
    [SuppressMessage("ReSharper", "ReturnTypeCanBeEnumerable.Local")]
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    [SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")]
    public static class NativeHelper
    {
        private static FieldInfo[] GetFields<T>(
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            var type = typeof(T);

            var fields = type.GetFields(flags);

            return fields;
        }

        private static int GetFieldSize(FieldInfo field)
        {
            var type = field.FieldType;

            var size = type == typeof(bool)
                ? sizeof(bool) // Marshal maps it to Win32 BOOL, we don't want that
                : Marshal.SizeOf(type);

            return size;
        }

        #region IsBlittable

        public static bool IsBlittable<T>() where T : struct
        {
            return IsBlittableCache<T>.Value;
        }

        private static class IsBlittableCache<T> where T : struct
        {
            public static readonly bool Value = GetValue<T>();

            private static bool GetValue<TStruct>() where TStruct : struct
            {
                var result = RuntimeHelpers.IsReferenceOrContainsReferences<TStruct>() == false;

                return result;
            }
        }

        #endregion

        #region AlignOf

        public static int AlignOf<T>() where T : struct
        {
            return AlignOfCache<T>.Value;
        }

        private static class AlignOfCache<T> where T : struct
        {
            public static readonly int Value = GetValue<T>();

            private static int GetValue<TStruct>() where TStruct : struct
            {
                var type = typeof(TStruct);

                if (IsBlittable<TStruct>() == false)
                {
                    throw new InvalidOperationException($"The type {type} is not blittable.");
                }

                var result = 1;

                var fields = GetFields<TStruct>();

                foreach (var field in fields)
                {
                    var fieldSize = GetFieldSize(field);

                    var max = Math.Max(result, fieldSize);

                    result = max;
                }

                return result;
            }
        }

        #endregion

        #region SizeOf

        public static int SizeOf<T>() where T : struct
        {
            return SizeOfCache<T>.Value;
        }

        private static class SizeOfCache<T> where T : struct
        {
            public static readonly int Value = GetValue<T>();

            private static int GetValue<TStruct>() where TStruct : struct
            {
                var result = 0;

                var fields = GetFields<TStruct>();

                foreach (var field in fields)
                {
                    var fieldSize = GetFieldSize(field);

                    result += fieldSize;
                }

                return result;
            }
        }

        #endregion
    }
}