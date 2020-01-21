using FluentAssertions;
using Marshmallow.HotChocolate;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text;
using Xunit;

[assembly: InternalsVisibleTo("Marshmallow.Tests")]
namespace Marshmallow.Tests.Extensions
{
    public class TypeExtensionsTests
    {
        [Fact]
        public void ListGenericCollectionTest()
        {
            var test = new List<string>();

            var result = test.GetType().IsGenericCollection();

            result.Should().BeTrue();
        }

        [Fact]
        public void CollectionGenericCollectionTest()
        {
            var test = new Collection<string>();

            var result = test.GetType().IsGenericCollection();

            result.Should().BeTrue();
        }
    }
}
