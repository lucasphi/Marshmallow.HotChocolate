using System;
using System.Linq.Expressions;

namespace Marshmallow.HotChocolate
{
    public interface IQueryProjection
    {
        Expression<Func<TEntity, dynamic>> CreateExpression<TEntity>();

        TScheme CreateScheme<TScheme>(object result) where TScheme : class, new();
    }
}
