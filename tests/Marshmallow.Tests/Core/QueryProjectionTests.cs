using FluentAssertions;
using HotChocolate.Execution;
using Marshmallow.HotChocolate;
using Marshmallow.HotChocolate.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Marshmallow.Tests.Core
{
    public class QueryProjectionTests
    {
        private readonly Mock<IHttpContextAccessor> _httpContextAcessorMock;

        public QueryProjectionTests()
        {
            _httpContextAcessorMock = new Mock<IHttpContextAccessor>();
        }

        [Fact]
        public void CreateExpressionWithoutInitializing()
        {
            var httpContext = new DefaultHttpContext();

            _httpContextAcessorMock.Setup(f => f.HttpContext).Returns(httpContext);

            Exception exception = Assert.Throws<MissingQueryException>(() => new QueryProjectionTest(_httpContextAcessorMock.Object));
            exception.Should().BeOfType(typeof(MissingQueryException));
        }

        [Fact]
        public void CreateExpressionWithoutCache()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Items.Add("graph", QueryRequestBuilder.New().SetQuery("{}").Create());

            _httpContextAcessorMock.Setup(f => f.HttpContext).Returns(httpContext);

            var queryProjection = new QueryProjectionTest(_httpContextAcessorMock.Object);

            queryProjection.CreateExpression<object>();

            queryProjection.Visited.Should().BeTrue();

            object cached;
        }

        [Fact]
        public void CreateExpressionNotCached()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Items.Add("graph", QueryRequestBuilder.New().SetQuery("{}").Create());

            _httpContextAcessorMock.Setup(f => f.HttpContext).Returns(httpContext);

            var queryProjection = new QueryProjectionTest(_httpContextAcessorMock.Object);

            queryProjection.CreateExpression<object>();

            queryProjection.Visited.Should().BeTrue();
        }
    }

    class QueryProjectionTest : QueryProjection
    {
        public bool Visited { get; set; }

        public QueryProjectionTest(IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        { }

        protected override System.Linq.Expressions.Expression<Func<TEntity, dynamic>> CreateExpression<TEntity>(IQuery query)
        {
            Visited = true;
            return null;
        }
    }
}
