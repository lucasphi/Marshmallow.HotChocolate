using System;

namespace Marshmallow.HotChocolate.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class AliasAttribute : Attribute
    {
        public string Name { get; }

        public AliasAttribute(string name)
        {
            Name = name;
        }
    }
}
