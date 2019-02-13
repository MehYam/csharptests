using System;
using System.Collections.Generic;

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
            public string getPrivate() { return privateReadonlyString; }
        }
        static void classAccess()
        {
            var engine = new Jint.Engine();
            engine.Execute(@"
            function testFunction(obj) {
                callback(obj.publicNum);
                callback(obj.publicString);
                callback(obj.someEnum);
                callback(obj.publicReadonlyString);

                obj.someList.Add(12345);
                callback(obj.someList[obj.someList.Count - 1]);
            }
            ");
            
            engine.SetValue("callback", new Action<string>(s => { Console.WriteLine($"callback from js: {s}"); } ));

            var someObj = new SomeClass(21, "12");
            engine.Invoke("testFunction", someObj);
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

            engine.SetValue("log", new Action<object>(Console.WriteLine));
            engine.SetValue("rng", new kaiGameUtil.RNG(2111));
            engine.SetValue("list", list);
            engine.Execute(@"
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
        }
    }
}
