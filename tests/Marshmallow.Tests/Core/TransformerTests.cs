using FluentAssertions;
using Marshmallow.HotChocolate;
using Marshmallow.HotChocolate.Core;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Xunit;

[assembly: InternalsVisibleTo("Marshmallow.Tests")]
namespace Marshmallow.Tests.Core
{
    public class TransformerTests
    {
        [Fact]
        public void MapObject()
        {
            var testObj = new
            {
                StrProp = "Hello",
                IntProp = 1,
                DateProp = new DateTime(2000, 01, 01),
                Child = new
                {
                    OtherStrProp = "Inner"
                }
            };
            var result = new Transformer().Transform<TestClass>(testObj);

            result.Should().BeEquivalentTo(new TestClass()
            {
                StrProp = "Hello",
                IntProp = 1,
                DateProp = new DateTime(2000, 01, 01),
                Child = new OtherClass
                {
                    OtherStrProp = "Inner"
                }
            });
        }

        [Fact]
        public void MapList()
        {
            List<object> list = new List<object>
            {
                new {
                    StrProp = "Hello",
                    IntProp = 1,
                    DateProp = new DateTime(2000, 01, 01),
                    Children = new List<object>() {
                        new { ListStrProp = "Inner"}
                    } 
                },
                new {
                    StrProp = "Hello2",
                }
            };
            var result = new Transformer().Transform<List<TestClass>>(list);

            result.Should().BeEquivalentTo(new List<TestClass>
            {
                new TestClass
                {
                    StrProp = "Hello",
                    IntProp = 1,
                    DateProp = new DateTime(2000, 01, 01),
                    Children = new List<ListClass>()
                    {
                        new ListClass() { ListStrProp = "Inner"}
                    }
                },
                new TestClass
                {
                    StrProp = "Hello2",
                }
            });
        }

        [Fact]
        public void MapObjectWithAttribute()
        {
            var testObj = new
            {
                StrProp = "Hello",
                Child = new
                {
                    InnerProp = "Val",
                    IntInnerProp = 1,
                }
            };
            var result = new Transformer().Transform<AttrSchema>(testObj);

            result.Should().BeEquivalentTo(new AttrSchema()
            {
                StrProp = "Hello",
                InnerProp = "Val",
                IntInnerProp = 1,
            });
        }

        [Fact]
        public void MapUsingStaticClass()
        {
            var testObj = new
            {
                StrProp = "Hello",                
            };
            var result = Schema.Create<TestClass>(testObj);

            result.Should().BeEquivalentTo(new TestClass()
            {
                StrProp = "Hello",                
            });
        }

        [Fact]
        public void MapGetOnlyProperty()
        {
            var testObj = new
            {
                Prop = "Hello",
                GetterOnly = "Value",
            };
            var result = new Transformer().Transform<GetterOnlyClass>(testObj);

            result.Should().BeEquivalentTo(new GetterOnlyClass()
            {
                Prop = "Hello",
            });
        }
    }
}
