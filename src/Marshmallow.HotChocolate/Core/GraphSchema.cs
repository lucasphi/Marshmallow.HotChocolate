using HotChocolate.Language;
using System;
using System.Reflection;

namespace Marshmallow.HotChocolate.Core
{
    class GraphSchema
    {
        public PropertyInfo Property { get; set; }

        public PropertyInfo SchemaProperty { get; set; }

        public FieldNode FieldNode { get; set; }

        public GraphSchema(PropertyInfo propertyInfo, PropertyInfo schemaProperty)
        {
            Property = propertyInfo;
            SchemaProperty = schemaProperty;
        }
    }
}
