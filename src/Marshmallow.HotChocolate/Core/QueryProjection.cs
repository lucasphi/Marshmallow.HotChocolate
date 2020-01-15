using HotChocolate.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Marshmallow.Tests")]
namespace Marshmallow.HotChocolate.Core
{
    class QueryProjection : IQueryProjection
    {
        private readonly IReadOnlyQueryRequest _readOnlyQueryRequest;
        private readonly IMemoryCache _memoryCache;

        public QueryProjection(IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache = null)
        {
            if (!httpContextAccessor.HttpContext.Items.ContainsKey("graph"))
            {
                throw new MissingQueryException();
            }

            _readOnlyQueryRequest = httpContextAccessor.HttpContext.Items["graph"] as IReadOnlyQueryRequest;
            _memoryCache = memoryCache;
        }

        public Expression<Func<TEntity, dynamic>> CreateExpression<TEntity>()
        {
            if (_memoryCache != null)
            {
                if (_memoryCache.TryGetValue(_readOnlyQueryRequest.QueryHash, out Expression<Func<TEntity, dynamic>> expression))
                {
                    return expression;
                }

                Expression<Func<TEntity, dynamic>> generatedExpression = CreateExpression<TEntity>(_readOnlyQueryRequest.Query);

                _memoryCache.Set(_readOnlyQueryRequest.QueryHash, generatedExpression);

                return generatedExpression;
            }
            else
            {
                return CreateExpression<TEntity>(_readOnlyQueryRequest.Query);
            }
        }

        protected virtual Expression<Func<TEntity, dynamic>> CreateExpression<TEntity>(IQuery query)
        {
            var parser = new GraphToExpressionParser<TEntity>(query as QueryDocument);
            return parser.CreateExpression();
        }
    }
}
