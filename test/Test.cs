using System;

namespace test
{
    class Test
    {
        static void Main(string[] args)
        {
            TestCollections.Run();
            Done();

            TestDefaultTypeValues();
            TestVerbatimStringLiteral();
            TestSwitchStuff();
            TestInnerClass();

            Console.ReadKey(true);
        }
        static void Done()
        {
            Console.WriteLine("=-=-=-=-=-=-=-=-=");
        }
#pragma warning disable CS0649
        class DefaultTypeValues
        {
            public float _float;
            public int _int;
            public string _string;
        }
#pragma warning restore CS0649
        static void TestDefaultTypeValues()
        {
            var foo = new DefaultTypeValues();
            Console.WriteLine($"float {foo._float}, int {foo._int}, string {foo._string}");
            Done();
        }
        static void TestVerbatimStringLiteral()
        {
            string verbatim = @"does this whitespace
            become part of the string?";

            Console.WriteLine($"verbatim: <{verbatim}>");
            Done();
        }
        static void TestSwitchStuff()
        {
            string foo = "bar";
            switch(foo)
            {
                case "bar":
                    Console.WriteLine("wow this works");
                    break;
            }
            Done();
        }
        class Outer
        {
            readonly int field;
            readonly Inner inner;
            public Outer(int outerField, int innerField) { this.field = outerField; this.inner = new Inner(this, innerField); }

            class Inner
            {
                readonly int field;
                readonly Outer parent;
                public Inner(Outer parent, int field) {  this.field = field; this.parent = parent; }

                public void TestCanAccessPrivateParentField()
                {
                    Console.WriteLine($"Inner instance {field} sees Outer's field: {parent.field}");
                }
            }
            public void Test() {  inner.TestCanAccessPrivateParentField(); }
        }
        static void TestInnerClass()
        {
            var outer = new Outer(21, 12);
            outer.Test();
            Done();
        }
    }
}
