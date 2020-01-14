using HotChocolate.Language;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marshmallow.HotChocolate
{
    [Serializable]
    public class UnsupportedOperationException : Exception
    {
        public UnsupportedOperationException(OperationType operationType)
            : base($"There is no support for the operation {operationType}")
        { }
    }
}
