using System;

namespace FluentGraphQL
{
    internal static class StringExtensions
    {
        public static string ToCamelCase(this string value)
        {
            Span<char> result = stackalloc char[value.Length];

            value.AsSpan().CopyTo(result);
            result[0] = char.ToLowerInvariant(result[0]);
            
            return new string(result);
        }
    }
}

