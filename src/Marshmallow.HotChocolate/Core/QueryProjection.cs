using HotChocolate.Execution;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Marshmallow.Tests")]
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

        public Expression<Func<TEntity, dynamic>> CreateExpression<TEntity>(bool usePagination = true)
        {
            return CreateExpression<TEntity, TEntity>(_readOnlyQueryRequest.Query, usePagination);
        }

        public Expression<Func<TEntity, dynamic>> CreateExpression<TEntity, TSchema>(bool usePagination = true)
        {
            return CreateExpression<TEntity, TSchema>(_readOnlyQueryRequest.Query, usePagination);
        }

        protected virtual Expression<Func<TEntity, dynamic>> CreateExpression<TEntity, TSchema>(IQuery query, bool usePagination)
        {
            var parser = new GraphToExpressionParser<TEntity>(query as QueryDocument);
            return parser.CreateExpression<TSchema>(usePagination);
        }
    }
}
