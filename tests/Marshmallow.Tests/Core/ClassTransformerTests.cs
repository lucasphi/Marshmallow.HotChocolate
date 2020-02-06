﻿using FluentAssertions;
using Marshmallow.HotChocolate.Core.Transformer;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Xunit;

[assembly: InternalsVisibleTo("Marshmallow.Tests")]
namespace Marshmallow.Tests.Core
{
    public class ClassTransformerTests
    {
        [Fact]
        public void MapObject()
        {
            var transformer = new ClassTransformer();

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
            var result = transformer.Transform<TestClass>(testObj);

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
            var transformer = new ClassTransformer();

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
            var result = transformer.Transform<List<TestClass>>(list);

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
            var transformer = new ClassTransformer();

            var testObj = new
            {
                StrProp = "Hello",
                Child = new
                {
                    InnerProp = "Val",
                    IntInnerProp = 1,
                }
            };
            var result = transformer.Transform<AttrSchema>(testObj);

            result.Should().BeEquivalentTo(new AttrSchema()
            {
                StrProp = "Hello",
                InnerProp = "Val",
                IntInnerProp = 1,
            });
        }
    }
}
