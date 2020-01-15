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
        private readonly Mock<MemoryCache> _memoryCacheMock;

        public QueryProjectionTests()
        {
            _httpContextAcessorMock = new Mock<IHttpContextAccessor>();
            _memoryCacheMock = new Mock<MemoryCache>();
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
            _memoryCacheMock.Verify(f => f.TryGetValue(It.IsAny<object>(), out cached), Times.Never);
        }

        [Fact]
        public void CreateExpressionNotCached()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Items.Add("graph", QueryRequestBuilder.New().SetQuery("{}").Create());

            _httpContextAcessorMock.Setup(f => f.HttpContext).Returns(httpContext);

            _memoryCacheMock.Setup(f => f.TryGetValue(It.IsAny<object>(), out cached)).Returns(false);

            var queryProjection = new QueryProjectionTest(_httpContextAcessorMock.Object, _memoryCacheMock.Object);

            queryProjection.CreateExpression<object>();

            queryProjection.Visited.Should().BeTrue();
        }

        [Fact]
        public void CreateExpressionCached()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Items.Add("graph", QueryRequestBuilder.New().SetQuery("{}").Create());

            _httpContextAcessorMock.Setup(f => f.HttpContext).Returns(httpContext);

            _memoryCacheMock.Setup(f => f.TryGetValue(It.IsAny<object>(), out cached)).Returns(true);

            var queryProjection = new QueryProjectionTest(_httpContextAcessorMock.Object, _memoryCacheMock.Object);

            queryProjection.CreateExpression<object>();

            queryProjection.Visited.Should().BeFalse();
        }

        private object cached;
    }

    class QueryProjectionTest : QueryProjection
    {
        public bool Visited { get; set; }

        public QueryProjectionTest(IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache = null) 
            : base(httpContextAccessor, memoryCache)
        { }

        protected override System.Linq.Expressions.Expression<Func<TEntity, dynamic>> CreateExpression<TEntity>(IQuery query)
        {
            Visited = true;
            return null;
        }
    }

    public class MemoryCache : IMemoryCache
    {
        public ICacheEntry CreateEntry(object key)
        {
            return new Mock<ICacheEntry>().Object;
        }

        public void Dispose()
        {
        }

        public void Remove(object key)
        {
        }

        public virtual bool TryGetValue(object key, out object value)
        {
            throw new NotImplementedException();
        }
    }
}
