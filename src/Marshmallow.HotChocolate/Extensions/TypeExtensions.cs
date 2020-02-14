using System;
using System.Collections.Generic;
using System.Linq;

namespace Marshmallow.HotChocolate
{
    static class TypeExtensions
    {
        public static bool IsGenericCollection(this Type type)
        {
            if (type.IsGenericType)
            {
                var enumerableType = typeof(IEnumerable<>);
                return type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == enumerableType);
            }

            return false;
        }

        public static bool IsTypePrimitive(this Type type)
        {
            if (type.IsPrimitive || type.IsValueType)
                return true;
            
            var underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                type = underlyingType;
            }

            return type == typeof(string) ||
                   type == typeof(DateTime) ||
                   type == typeof(decimal);
        }
    }
}
