using System;
using System.Collections.Generic;
using System.Text;

namespace Marshmallow.HotChocolate
{
    [Serializable]
    public class PropertyNotFoundException : Exception
    {
        public PropertyNotFoundException(string type, string propertyName)
            : base ($"The type {type} does not contain a property '{propertyName}'.")
        { }
    }
}
