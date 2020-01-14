using HotChocolate.Execution;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq.Expressions;

namespace Marshmallow.HotChocolate.Core
{
    class QueryProjection : IQueryProjection
    {
        private readonly IReadOnlyQueryRequest _readOnlyQueryRequest;

        public QueryProjection(IHttpContextAccessor httpContextAccessor)
        {
            if (!httpContextAccessor.HttpContext.Items.ContainsKey("graph"))
            {
                throw new MissingQueryException();
            }

            _readOnlyQueryRequest = httpContextAccessor.HttpContext.Items["graph"] as IReadOnlyQueryRequest;
        }

        public Expression<Func<TEntity, dynamic>> CreateExpression<TEntity>()
        {
            var parser = new GraphToExpressionParser<TEntity>(_readOnlyQueryRequest.Query as QueryDocument);
            return parser.CreateExpression();
        }
    }
}
