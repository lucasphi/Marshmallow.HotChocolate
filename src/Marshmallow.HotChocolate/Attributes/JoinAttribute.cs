using System;

namespace Marshmallow.HotChocolate
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
