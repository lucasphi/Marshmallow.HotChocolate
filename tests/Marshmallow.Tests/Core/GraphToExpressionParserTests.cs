using FluentAssertions;
using HotChocolate.Execution;
using HotChocolate.Language;
using Marshmallow.HotChocolate;
using Marshmallow.HotChocolate.Core;
using System;
using Xunit;

namespace Marshmallow.Tests.Core
{
    public class GraphToExpressionParserTests
    {
        [Fact]
        public void FilterPaginationNodes()
        {
            DocumentNode document = Utf8GraphQLParser.Parse(@"
            {
                testQuery
                {
                    edges
                    {
                        node
                        {
                            strProp intProp dateProp child { otherStrProp }
                        }
                    }
                    nodes 
                    {
                        strProp intProp dateProp child { otherStrProp }
                    }
                    pageInfo
                    {
                        endCursor
                        hasNextPage
                        hasPreviousPage
                        startCursor
                    }
                    totalCount
                }
            }");

            var queryBuilder = QueryRequestBuilder.New().SetQuery(document);

            var queryRequest = queryBuilder.Create();

            var parser = new GraphToExpressionParser<TestClass>(queryRequest.Query as QueryDocument);

            var expression = parser.CreateExpression<TestClass>(usePagination: true);

            expression.ToString().Should().Be("a => new {StrProp = a.StrProp, IntProp = a.IntProp, DateProp = a.DateProp, Child = new {OtherStrProp = a.Child.OtherStrProp}}");
        }

        [Fact]
        public void CreateBasicExpression()
        {
            DocumentNode document = Utf8GraphQLParser.Parse("{ testQuery { strProp intProp dateProp decProp } }");

            var queryBuilder = QueryRequestBuilder.New().SetQuery(document);

            var queryRequest = queryBuilder.Create();

            var parser = new GraphToExpressionParser<TestClass>(queryRequest.Query as QueryDocument);

            var expression = parser.CreateExpression<TestClass>();

            expression.ToString().Should().Be("a => new {StrProp = a.StrProp, IntProp = a.IntProp, DateProp = a.DateProp, DecProp = a.DecProp}");
        }

        [Fact]
        public void CreateExpressionForQuery()
        {
            DocumentNode document = Utf8GraphQLParser.Parse("{ testQuery { strProp intProp dateProp child { otherStrProp } sameClass { strProp } children { listStrProp } } }");

            var queryBuilder = QueryRequestBuilder.New().SetQuery(document);

            var queryRequest = queryBuilder.Create();

            var parser = new GraphToExpressionParser<TestClass>(queryRequest.Query as QueryDocument);

            var expression = parser.CreateExpression<TestClass>();

            expression.ToString().Should().Be("a => new {StrProp = a.StrProp, IntProp = a.IntProp, DateProp = a.DateProp, Child = new {OtherStrProp = a.Child.OtherStrProp}, SameClass = new {StrProp = a.SameClass.StrProp}, Children = a.Children.Select(b => new {ListStrProp = b.ListStrProp})}");
        }

        [Fact]
        public void CreateExpressionForMutation()
        {
            DocumentNode document = Utf8GraphQLParser.Parse("mutation { updateClient(strProp: \"test\") { strProp intProp dateProp child { otherStrProp } sameClass { strProp } children { listStrProp } } }");

            var queryBuilder = QueryRequestBuilder.New().SetQuery(document);

            var queryRequest = queryBuilder.Create();

            var parser = new GraphToExpressionParser<TestClass>(queryRequest.Query as QueryDocument);

            var expression = parser.CreateExpression<TestClass>();

            expression.ToString().Should().Be("a => new {StrProp = a.StrProp, IntProp = a.IntProp, DateProp = a.DateProp, Child = new {OtherStrProp = a.Child.OtherStrProp}, SameClass = new {StrProp = a.SameClass.StrProp}, Children = a.Children.Select(b => new {ListStrProp = b.ListStrProp})}");
        }

        [Fact]
        public void UnsupportedOperation()
        {
            DocumentNode document = Utf8GraphQLParser.Parse("subscription { onReview(episode: NEWHOPE) { stars comment } }");

            var queryBuilder = QueryRequestBuilder.New().SetQuery(document);

            var queryRequest = queryBuilder.Create();

            var parser = new GraphToExpressionParser<TestClass>(queryRequest.Query as QueryDocument);

            Exception exception = Assert.Throws<UnsupportedOperationException>(() => parser.CreateExpression<TestClass>());
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

            var expression = parser.CreateExpression<AttrSchema>();

            expression.ToString().Should().Be("a => new {StrProp = a.StrProp, Child = new {InnerProp = a.Child.InnerProp, IntInnerProp = a.Child.IntInnerProp}}");
        }

        [Fact]
        public void CreateExpressionFromComplexObjectWithListInside()
        {
            DocumentNode document = Utf8GraphQLParser.Parse("{ testQuery { child { otherStrProp secondChild { id EmptyId } } } }");

            var queryBuilder = QueryRequestBuilder.New().SetQuery(document);

            var queryRequest = queryBuilder.Create();

            var parser = new GraphToExpressionParser<TestClass>(queryRequest.Query as QueryDocument);

            var expression = parser.CreateExpression<TestClass>();

            expression.ToString().Should().Be("a => new {Child = new {OtherStrProp = a.Child.OtherStrProp, SecondChild = a.Child.SecondChild.Select(b => new {Id = b.Id, EmptyId = b.EmptyId})}}");
        }
    }
}
