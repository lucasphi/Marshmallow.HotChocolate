using Marshmallow.HotChocolate;
using Marshmallow.HotChocolate.Attributes;
using System;
using System.Collections.Generic;

namespace Marshmallow.Tests
{
    public class TestClass
    {
        public string StrProp { get; set; }

        public int IntProp { get; set; }

        public DateTime DateProp { get; set; }

        public decimal DecProp { get; set; }

        public OtherClass Child { get; set; }

        public TestClass SameClass { get; set; }

        public ICollection<ListClass> Children { get; set; }
    }

    public class OtherClass
    {
        public string OtherStrProp { get; set; }

        public List<SecondChildClass> SecondChild { get; set; }
    }

    public class SecondChildClass
    {
        public Guid Id { get; set; }
    }

    public class ListClass
    {
        public string ListStrProp { get; set; }
    }

    public class AttrSchema
    {
        public string StrProp { get; set; }

        [Alias("Prop")]
        public string Prop1 { get; set; }

        [Alias("Prop")]
        public string Prop2 { get; set; }

        [Join(nameof(AttrData.Child))]
        public long? InnerProp { get; set; }

        [Join(nameof(AttrData.Child))]
        public Guid Guid { get; set; }

        [Join(nameof(AttrData.Child))]
        public ICollection<AttrChildPropData> IntInnerProp { get; set; }
    }

    public class AttrData
    {
        public string StrProp { get; set; }

        public string Prop { get; set; }

        public AttrChildData Child { get; set; }

        public static List<AttrData> CreateTestList()
        {
            return new List<AttrData>()
            {
                new AttrData()
                {
                    StrProp = "aaa",
                    Child = new AttrChildData()
                    {
                        InnerProp = 7,
                        Guid = Guid.NewGuid(),
                        IntInnerProp = new List<AttrChildPropData>()
                        {
                            new AttrChildPropData()
                            {
                                Val = "test"
                            }
                        }
                    }
                }
            };
        }
    }

    public class AttrChildData
    {
        public long? InnerProp { get; set; }

        public Guid Guid { get; set; }

        public ICollection<AttrChildPropData> IntInnerProp { get; set; }
    }

    public class AttrChildPropData
    {
        public string Val { get; set; }
    }

    public class GetterOnlyClass
    {
        public string Prop { get; set; }

        public string GetterOnly { get; } = "Test";
    }
}
