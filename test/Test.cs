using System;

namespace test
{
    class Test
    {
        static void Main(string[] args)
        {
            TestDefaultTypeValues();
            TestCollections.Run();

            Console.ReadKey(true);
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
        }
    }
}
