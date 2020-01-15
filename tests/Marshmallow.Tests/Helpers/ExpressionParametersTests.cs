using System;
using Xunit;
using Marshmallow.HotChocolate.Helpers;
using FluentAssertions;

namespace Marshmallow.Tests.Helpers
{
    public class ExpressionParametersTests
    {
        [Fact]
        public void GetNext()
        {
            var parameter = new ExpressionParameters();
            parameter.Next().Should().Be("a");
            parameter.Next().Should().Be("b");
        }
    }
}
