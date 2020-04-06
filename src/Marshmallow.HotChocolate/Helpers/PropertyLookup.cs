using Marshmallow.HotChocolate.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Marshmallow.Tests")]
namespace Marshmallow.HotChocolate.Helpers
{
    class PropertyLookup
    {
        private readonly PropertyInfo[] _properties;

        public Type Type { get; }

        public PropertyLookup(Type type)
        {
            Type = type;
            _properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        }

        public PropertyInfo FindProperty(string name)
        {
            name = name.ToLower();
            foreach (var prop in _properties)
            {
                var nameAttribute = prop.GetCustomAttributes(typeof(AliasAttribute), false).FirstOrDefault() as AliasAttribute;
                if (nameAttribute != null && nameAttribute.Name.ToLower() == name)
                {
                    return prop;
                }

                if (prop.Name.ToLower() == name)
                {
                    return prop;
                }
            }
            return null;
        }

        public IEnumerable<PropertyInfo> GetAllProperties()
        {
            return _properties;
        }
    }
}
