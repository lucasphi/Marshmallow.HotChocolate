using FluentAssertions;
using HotChocolate.Execution;
using HotChocolate.Language;
using Marshmallow.HotChocolate;
using Marshmallow.HotChocolate.Core;
using System;
using System.Collections.Generic;
using Xunit;

namespace Marshmallow.Tests.Core
{
    public class GraphToExpressionParserTests
    {
        [Fact]
        public void CreateExpression()
        {
            DocumentNode document = Utf8GraphQLParser.Parse("{ testQuery { strProp intProp dateProp child { strProp } sameClass { strProp } children { strProp } } }");

            var queryBuilder = QueryRequestBuilder.New().SetQuery(document);

            var queryRequest = queryBuilder.Create();

            var parser = new GraphToExpressionParser<TestClass>(queryRequest.Query as QueryDocument);

            var expression = parser.CreateExpression();

            expression.ToString().Should().Be("a => new {StrProp = a.StrProp, IntProp = a.IntProp, DateProp = a.DateProp, Child = new {StrProp = a.Child.StrProp}, SameClass = new {StrProp = a.SameClass.StrProp}, Children = a.Children.Select(b => new {StrProp = b.StrProp})}");
        }

        [Fact]
        public void UnsupportedOperation()
        {
            DocumentNode document = Utf8GraphQLParser.Parse("mutation { testMutation { } }");

            var queryBuilder = QueryRequestBuilder.New().SetQuery(document);

            var queryRequest = queryBuilder.Create();

            var parser = new GraphToExpressionParser<TestClass>(queryRequest.Query as QueryDocument);

            Exception exception = Assert.Throws<UnsupportedOperationException>(() => parser.CreateExpression());
            exception.Should().BeOfType(typeof(UnsupportedOperationException));
            exception.Message.Should().Be("There is no support for the operation Mutation");
        }

        public class TestClass
        {
            public string StrProp { get; set; }

            public int IntProp { get; set; }

            public DateTime DateProp { get; set; }

            public OtherClass Child { get; set; }

            public TestClass SameClass { get; set; }

            public ICollection<ListClass> Children { get; set; }
        }

        public class OtherClass
        {
            public string StrProp { get; set; }
        }

        public class ListClass
        {
            public string StrProp { get; set; }
        }
    }
}
