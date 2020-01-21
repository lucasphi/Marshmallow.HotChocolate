using System;
using System.Collections.Generic;
using System.Linq;

namespace Marshmallow.HotChocolate
{
    static class TypeExtensions
    {
        public static bool IsComplex(this Type type)
        {
            return !type.IsPrimitive
                && !type.IsGenericType
                && !type.IsEnum
                && type != typeof(string)
                && type != typeof(DateTime)
                && type != typeof(Guid);
        }

        public static bool IsGenericCollection(this Type type)
        {
            if (type.IsGenericType)
            {
                var enumerableType = typeof(IEnumerable<>);
                return type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == enumerableType);
            }

            return false;
        }
    }
}
