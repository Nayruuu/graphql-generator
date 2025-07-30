using System;
using System.Linq;

namespace FluentGraphQL
{
    internal static class ObjectExtensions
    {
        public static bool IsPrimitive(this Type type)
        {
            var types = new[] {
                typeof(string),
                typeof(char?),
                typeof(byte?),
                typeof(sbyte?),
                typeof(ushort?),
                typeof(short?),
                typeof(uint?),
                typeof(int?),
                typeof(ulong?),
                typeof(long?),
                typeof(float?),
                typeof(double?),
                typeof(decimal?),
                typeof(DateTime?),
                typeof(Guid?),

                typeof(char),
                typeof(byte),
                typeof(sbyte),
                typeof(ushort),
                typeof(short),
                typeof(uint),
                typeof(int),
                typeof(ulong),
                typeof(long),
                typeof(float),
                typeof(double),
                typeof(decimal),
                typeof(DateTime),
                typeof(Guid)
            };

            return type.IsPrimitive || types.Any(t => t.IsAssignableFrom(type));
        }
    }
}
