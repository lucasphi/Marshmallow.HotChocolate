using Marshmallow.HotChocolate;
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
