using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace jinttest
{
    class jinttest
    {
        static void Main(string[] args)
        {
            basics();
            exceptions();
            classAccess();
            classAccessExceptions();
            listOperations();
        }
        static void basics()
        {
            var engine = new Jint.Engine();

            engine.SetValue("log", new Action<object>(Console.WriteLine));
            engine.Execute("log('hello from Jint')");

            engine.Execute("var foo = 'bar';");
            engine.Execute("log(foo)");

            engine.Execute(@"
            var array = [ 1, 2, 3 ];
            var array2 = array.filter(function(i) { return i > 1; });
            log('filtered array length: ' + array2.length);
            ");
        }
        static void exceptions()
        {
            var engine = new Jint.Engine();
            try
            {
                engine.Execute("not_a_function()");
            }
            catch (Exception e)
            {
                Console.WriteLine($"caught expected exception: {e.Message}, {e.GetType()}");
            }
        }
        class SomeClass
        {
            public enum SomeEnum { First, Second };
            public int publicNum;
            public string publicString;
            public SomeEnum someEnum;
            public readonly string publicReadonlyString;
            public List<int> someList = new List<int> { 1, 2, 3 };
            private readonly string privateReadonlyString;
            public SomeClass(int num, string str)
            {
                publicNum = num;
                publicString = str;
                someEnum = SomeEnum.First;

                publicReadonlyString = $"{str} (readonly)";
                privateReadonlyString = $"{str} (private)";
            }
            public void SomeMethod(SomeClass someOther)
            {
                Console.WriteLine($"SomeClass.SomeMethod {publicNum} got other {someOther.publicNum}");
            }
            public string getPrivate() { return privateReadonlyString; }
        }
        static void classAccess()
        {
            var engine = new Jint.Engine();
            engine.Execute(@"
            function test1(obj) {
                callback(obj.publicNum);
                callback(obj.publicString);
                callback(obj.someEnum);
                callback(obj.publicReadonlyString);

                obj.someList.Add(12345);
                callback(obj.someList[obj.someList.Count - 1]);
            }
            function test2(obj1, obj2) {
                obj1.SomeMethod(obj2);
            }
            function test3(obj) {
                return obj;
            }
            ");
            
            engine.SetValue("callback", new Action<string>(s => { Console.WriteLine($"callback from js: {s}"); } ));

            // javascript access of class members
            var someObj = new SomeClass(21, "12");
            engine.Invoke("test1", someObj);

            // javascript calling a C# method, passing it a C# object
            var someOther = new SomeClass(22, "22");
            engine.Invoke("test2", someObj, someOther);

            // javascript returning a C# object
            var result = engine.Invoke("test3", someObj);

            Debug.Assert(result.ToObject() == someObj);
        }
        static void classAccessExceptions()
        {
            var engine = new Jint.Engine();
            var someObj = new SomeClass(21, "12");
            engine.SetValue("someObj", someObj);
            engine.SetValue("callback", new Action<string>(s => { Console.WriteLine($"callback from js: {s}"); } ));

            void runJs(string js)
            {
                try
                {
                    engine.Execute(js);
                    Console.WriteLine($"script <{js}> ran without issue");
                }
                catch(Exception e)
                {
                    Console.WriteLine($"script <{js}> threw {e.GetType()}: {e.Message}");
                }
            }
            runJs("callback(someObj)");
            runJs("callback(someObj.privateReadonlyString");
            runJs("someObj.publicReadonlyString = 'foo'");
            runJs("callback(someObj.publicReadonlyString)");
            Console.WriteLine($"someObj.publicReadonlyString: {someObj.publicReadonlyString}");
        }
        static void listOperations()
        {
            var engine = new Jint.Engine();
            var list = new List<SomeClass> { new SomeClass(20, "twenty"), new SomeClass(-20, "negative twenty"), new SomeClass(0, "zero") };

            IList<SomeClass> ilist = list;
            var e = ilist.GetEnumerator();
            while (e.MoveNext())
            {
                e.Current;
            }

            engine.SetValue("log", new Action<object>(Console.WriteLine));
            engine.SetValue("rng", new kaiGameUtil.RNG(2111));
            engine.SetValue("list", list);
            engine.Execute(@"
                log('testing random list operations:');
                log(typeof(list));
                log(list[0].publicNum);

                let count = list.Count;
                let newArray = [];
                for (let i = 0; i < count; ++i) {
                    if (list[i].publicNum >= 0) {
                        newArray.push(list[i]);
                    }
                }
                log('filtered: ' + newArray.length);
                function rngArray(a) { return a[ rng.Next(0, a.length - 1) ].publicNum; }

                for (let i = 0; i < 20; ++i) log('more rng: ' + rng.Next(0, 3));
            ");
            engine.SetValue("listArray", list.ToArray());
            engine.Execute(@"
                log('testing using list as array');
                log('listArray.length: ' + listArray.length);

                listArray = listArray.filter(function(i) { return i.publicNum >= 0 });
                log('filtered length: ' + listArray.length);
            ");
        }
    }
}
