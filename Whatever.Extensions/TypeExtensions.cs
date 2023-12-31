﻿using System;
using System.Text.RegularExpressions;

namespace Whatever.Extensions
{
    /// <summary>
    ///     Extension methods for <see cref="Type" />.
    /// </summary>
    public static class TypeExtensions
    {
        private const RegexOptions GetNiceNameRegexOptions = RegexOptions.Compiled | RegexOptions.Singleline;

        private static readonly Regex GetNiceNameRegex1 = new(@"`\d\[", GetNiceNameRegexOptions);

        private static readonly Regex GetNiceNameRegex2 = new(@"\]", GetNiceNameRegexOptions);

        private static readonly Regex GetNiceNameRegex3 = new(@"\w+\.", GetNiceNameRegexOptions);

        /// <summary>
        ///     Gets a nice name string for a type, i.e. without back-quotes for type arguments and optionally non-qualified.
        /// </summary>
        public static string GetNiceName(this Type type, bool qualified)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            var name = type.ToString();

            name = GetNiceNameRegex1.Replace(name, "<");

            name = GetNiceNameRegex2.Replace(name, ">");

            if (qualified)
            {
                return name;
            }

            name = GetNiceNameRegex3.Replace(name, string.Empty);

            return name;
        }
    }
}