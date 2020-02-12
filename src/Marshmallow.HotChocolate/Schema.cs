using Marshmallow.HotChocolate.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marshmallow.HotChocolate
{
    public static class Schema
    {
        public static TDestiny Create<TDestiny>(object source)
            where TDestiny : class, new()
        {
            return new Transformer().Transform<TDestiny>(source);
        }
    }
}
