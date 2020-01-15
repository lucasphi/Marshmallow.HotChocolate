using FluentAssertions;
using HotChocolate;
using Marshmallow.HotChocolate.Core;
using Xunit;

namespace Marshmallow.Tests.Core
{
    public class PropertyLoopTests
    {
        [Fact]
        public void TestFindProperty()
        {
            var propertyLookup = new PropertyLookup(typeof(TestClass));

            var result = propertyLookup.FindProperty("Prop2");

            result.Should().NotBeNull();
        }

        [Fact]
        public void TestNonExistingProperty()
        {
            var propertyLookup = new PropertyLookup(typeof(TestClass));

            var result = propertyLookup.FindProperty("Empty");

            result.Should().BeNull();
        }

        [Fact]
        public void TestFindPropertyByAttribute()
        {
            var propertyLookup = new PropertyLookup(typeof(TestClass));

            var result = propertyLookup.FindProperty("Prop3");

            result.Should().NotBeNull();
        }

        public class TestClass
        {
            public string Prop1 { get; set; }

            public int Prop2 { get; set; }

            [GraphQLName("Prop3")]
            public string Test { get; set; }
        }
    }
}
