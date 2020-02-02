using FluentAssertions;
using HotChocolate;
using Marshmallow.HotChocolate.Helpers;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Marshmallow.Tests.Helpers
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

        [Fact]
        public void FindInheritedProperty()
        {
            var propertyLookup = new PropertyLookup(typeof(TestClass));

            var result = propertyLookup.FindProperty("BaseProp");

            result.Should().NotBeNull();
        }

        [Fact]
        public void SkipPrivateProperty()
        {
            var propertyLookup = new PropertyLookup(typeof(TestClass));

            var result = propertyLookup.FindProperty("PrivateProp");

            result.Should().BeNull();
        }

        [Fact]
        public void SkipProtectedProperty()
        {
            var propertyLookup = new PropertyLookup(typeof(TestClass));

            var result = propertyLookup.FindProperty("ProtectedProp");

            result.Should().BeNull();
        }

        [Fact]
        public void ListAllProperties()
        {
            var propertyLookup = new PropertyLookup(typeof(TestClass));

            var result = propertyLookup.GetAllProperties();

            result.Should().HaveCount(4);
            result.Select(f => f.Name).Should().BeEquivalentTo(new List<string>()
            {
                "Prop1", "Prop2", "Test", "BaseProp"
            });
        }

        public class TestClass : BaseClass
        {
            public string Prop1 { get; set; }

            public int Prop2 { get; set; }

            [GraphQLName("Prop3")]
            public string Test { get; set; }

            private string PrivateProp { get; set; }

            protected int ProtectedProp { get; set; }
        }

        public class BaseClass
        {
            public string BaseProp { get; set; }
        }
    }
}
