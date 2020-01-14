using HotChocolate;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Marshmallow.Tests")]
namespace Marshmallow.HotChocolate.Core
{
    class PropertyLookup
    {
        private readonly PropertyInfo[] _properties;

        public PropertyLookup(Type type)
        {
            _properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        public PropertyInfo FindProperty(string name)
        {
            foreach (var prop in _properties)
            {
                if (prop.Name.ToLower() == name.ToLower())
                {
                    return prop;
                }

                var nameAttribute = prop.GetCustomAttributes(typeof(GraphQLNameAttribute), false).FirstOrDefault() as GraphQLNameAttribute;
                if (nameAttribute != null && nameAttribute.Name == name)
                {
                    return prop;
                }
            }
            return null;
        }
    }
}
