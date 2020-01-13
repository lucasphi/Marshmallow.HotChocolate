using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("Marshmallow.Tests")]
namespace Marshmallow.HotChocolate.Helpers
{
    class ExpressionParameters
    {
        private int ascIICode = 97;

        public string Next()
        {
            int code = ascIICode;
            ascIICode += 1;
            return ((char)code).ToString();
        }
    }
}
