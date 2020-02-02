using System.Reflection;

namespace Marshmallow.HotChocolate.Core
{
    class GraphScheme
    {
        public PropertyInfo Property { get; set; }

        public PropertyInfo SchemeProperty { get; set; }

        public GraphScheme(PropertyInfo propertyInfo, PropertyInfo schemeProperty)
        {
            Property = propertyInfo;
            SchemeProperty = schemeProperty;
        }
    }
}
