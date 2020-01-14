using HotChocolate.Execution;
using Marshmallow.HotChocolate.Core;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq.Expressions;

namespace Marshmallow.HotChocolate
{
    public static class IHttpContextAccessorExtensions
    {
        public static IReadOnlyQueryRequest SetRequestQuery(this HttpContext context, IQueryRequestBuilder queryRequestBuilder)
        {
            IReadOnlyQueryRequest request = queryRequestBuilder.Create();
            context.Items.Add("graph", request);

            return request;
        }

        public static IReadOnlyQueryRequest QueryRequest(this IHttpContextAccessor contextAccessor)
        {
            var context = contextAccessor.HttpContext;

            if (!context.Items.ContainsKey("graph"))
            {
                throw new MissingQueryException();
            }

            return contextAccessor.HttpContext.Items["graph"] as IReadOnlyQueryRequest;
        }

        public static Expression<Func<TEntity, dynamic>> CreateExpression<TEntity>(this IReadOnlyQueryRequest queryRequest)
        {
            var parser = new GraphToExpressionParser<TEntity>(queryRequest.Query as QueryDocument);
            return parser.CreateExpression();
        }
    }
}
