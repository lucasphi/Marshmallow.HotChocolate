using HotChocolate.Execution;
using Microsoft.AspNetCore.Http;

namespace Marshmallow.HotChocolate
{
    public static class HttpContextExtensions
    {
        public static IReadOnlyQueryRequest SetRequestQuery(this HttpContext context, IQueryRequestBuilder queryRequestBuilder)
        {
            IReadOnlyQueryRequest request = queryRequestBuilder.Create();
            context.Items.Add("graph", request);

            return request;
        }
    }
}
