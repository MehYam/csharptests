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

            Console.ReadKey(true);
        }
        static void Done()
        {
            Console.WriteLine("=-=-=-=-=-=-=-=-=");
        }
        class DefaultTypeValues
        {
            public float _float;
            public int _int;
            public string _string;
        }
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
    }
}
