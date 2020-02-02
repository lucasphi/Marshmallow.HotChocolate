using System;

namespace Marshmallow.HotChocolate.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class JoinAttribute : Attribute
    {
        public string PropertyName { get; private set; }

        public JoinAttribute(string tableName)
        {
            PropertyName = tableName;
        }
    }
}
