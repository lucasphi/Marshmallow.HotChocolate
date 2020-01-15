using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Marshmallow.HotChocolate
{
    [Serializable]
    public class MissingQueryException : Exception
    {
        public MissingQueryException()
            : base($"The method {nameof(HttpContextExtensions.SetRequestQuery)} must be called before injecting {nameof(IQueryProjection)}.")
        { }
    }
}
