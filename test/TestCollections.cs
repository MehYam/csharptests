using System.Collections.Generic;

namespace test
{
    static class TestCollections
    {
        static public void Run()
        {
            TestReadOnlyCollections();
        }
        struct Foo<T>
        {
            public T x;
            public T y;
            public Foo(T x, T y) {  this.x = x; this.y = y; }
        }
        static void TestReadOnlyCollections()
        {
            var list = new List<Foo<int>>();
            list.Add(new Foo<int>(1, 2));
            list.Add(new Foo<int>(3, 4));

            var constList = list.AsReadOnly();

            // will not compile
            //constList[1] = new Foo<int>(4, 5);

            IList<Foo<int>> sortaConstList = list.AsReadOnly();

            // will throw
            sortaConstList[1] = new Foo<int>(7, 8);
        }
    }
}
