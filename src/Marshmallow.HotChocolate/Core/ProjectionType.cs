using System;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Marshmallow.HotChocolate.Core
{
    class GraphExpression
    {
        public DynamicProperty Property { get; set; }

        public Expression Expression { get; set; }
    }
}
