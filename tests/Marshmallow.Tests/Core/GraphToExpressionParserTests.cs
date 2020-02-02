using FluentAssertions;
using HotChocolate.Execution;
using HotChocolate.Language;
using Marshmallow.HotChocolate;
using Marshmallow.HotChocolate.Core;
using Marshmallow.HotChocolate.Core.Attributes;
using System;
using System.Collections.Generic;
using Xunit;

namespace Marshmallow.Tests.Core
{
    public class GraphToExpressionParserTests
    {
        [Fact]
        public void CreateBasicExpression()
        {
            DocumentNode document = Utf8GraphQLParser.Parse("{ testQuery { strProp intProp dateProp } }");

            var queryBuilder = QueryRequestBuilder.New().SetQuery(document);

            var queryRequest = queryBuilder.Create();

            var parser = new GraphToExpressionParser<TestClass>(queryRequest.Query as QueryDocument);

            var expression = parser.CreateExpression();

            expression.ToString().Should().Be("a => new {StrProp = a.StrProp, IntProp = a.IntProp, DateProp = a.DateProp}");
        }

        [Fact]
        public void CreateExpressionForQuery()
        {
            DocumentNode document = Utf8GraphQLParser.Parse("{ testQuery { strProp intProp dateProp child { strProp } sameClass { strProp } children { strProp } } }");

            var queryBuilder = QueryRequestBuilder.New().SetQuery(document);

            var queryRequest = queryBuilder.Create();

            var parser = new GraphToExpressionParser<TestClass>(queryRequest.Query as QueryDocument);

            var expression = parser.CreateExpression();

            expression.ToString().Should().Be("a => new {StrProp = a.StrProp, IntProp = a.IntProp, DateProp = a.DateProp, Child = new {StrProp = a.Child.StrProp}, SameClass = new {StrProp = a.SameClass.StrProp}, Children = a.Children.Select(b => new {StrProp = b.StrProp})}");
        }

        [Fact]
        public void CreateExpressionForMutation()
        {
            DocumentNode document = Utf8GraphQLParser.Parse("mutation { updateClient(strProp: \"test\") { strProp intProp dateProp child { strProp } sameClass { strProp } children { strProp } } }");

            var queryBuilder = QueryRequestBuilder.New().SetQuery(document);

            var queryRequest = queryBuilder.Create();

            var parser = new GraphToExpressionParser<TestClass>(queryRequest.Query as QueryDocument);

            var expression = parser.CreateExpression();

            expression.ToString().Should().Be("a => new {StrProp = a.StrProp, IntProp = a.IntProp, DateProp = a.DateProp, Child = new {StrProp = a.Child.StrProp}, SameClass = new {StrProp = a.SameClass.StrProp}, Children = a.Children.Select(b => new {StrProp = b.StrProp})}");
        }

        [Fact]
        public void UnsupportedOperation()
        {
            DocumentNode document = Utf8GraphQLParser.Parse("subscription { onReview(episode: NEWHOPE) { stars comment } }");

            var queryBuilder = QueryRequestBuilder.New().SetQuery(document);

            var queryRequest = queryBuilder.Create();

            var parser = new GraphToExpressionParser<TestClass>(queryRequest.Query as QueryDocument);

            Exception exception = Assert.Throws<UnsupportedOperationException>(() => parser.CreateExpression());
            exception.Should().BeOfType(typeof(UnsupportedOperationException));
            exception.Message.Should().Be("There is no support for the operation Subscription");
        }

        [Fact]
        public void ClassConvertion()
        {
            DocumentNode document = Utf8GraphQLParser.Parse("{ testQuery { strProp innerProp intInnerProp } }");

            var queryBuilder = QueryRequestBuilder.New().SetQuery(document);

            var queryRequest = queryBuilder.Create();

            var parser = new GraphToExpressionParser<AttrData>(queryRequest.Query as QueryDocument);

            var expression = parser.CreateExpression<AttrScheme>();

            expression.ToString().Should().Be("a => new {StrProp = a.StrProp, Child = new {InnerProp = a.Child.InnerProp, IntInnerProp = a.Child.IntInnerProp}}");
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

        public class AttrScheme
        {
            public string StrProp { get; set; }

            [Join(nameof(AttrData.Child))]
            public string InnerProp { get; set; }

            [Join(nameof(AttrData.Child))]
            public int IntInnerProp { get; set; }
        }

        public class AttrData
        {
            public string StrProp { get; set; }

            public AttrChildData Child { get; set; }
        }

        public class AttrChildData
        {
            public string InnerProp { get; set; }

            public int IntInnerProp { get; set; }
        }
    }
}
