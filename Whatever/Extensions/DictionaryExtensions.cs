using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Whatever.Extensions
{
    /// <summary>
    ///     Extension methods for <see cref="IDictionary{TKey,TValue}" />.
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        ///     Gets or adds a value to this instance.
        /// </summary>
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, [DisallowNull] TKey key, Func<TValue> valueFactory)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (valueFactory == null)
            {
                throw new ArgumentNullException(nameof(valueFactory));
            }

            if (dictionary.TryGetValue(key, out var value))
            {
                return value;
            }

            value = valueFactory();

            dictionary.Add(key, value);

            return value;
        }
    }
}