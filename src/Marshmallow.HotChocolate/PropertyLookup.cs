using HotChocolate;
using System.Linq;
using System.Reflection;

namespace Marshmallow.HotChocolate
{
    class PropertyLookup
    {
        public PropertyInfo FindProperty(PropertyInfo[] typeProperties, string name)
        {
            foreach (var prop in typeProperties)
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
