using System;
using System.Collections.Generic;
using System.Text;

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
