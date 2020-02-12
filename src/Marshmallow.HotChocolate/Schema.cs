using Marshmallow.HotChocolate.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marshmallow.HotChocolate
{
    public static class Schema
    {
        public static TDestination Create<TDestination>(object source)
            where TDestination : class, new()
        {
            return new Transformer().Transform<TDestination>(source);
        }
    }
}
