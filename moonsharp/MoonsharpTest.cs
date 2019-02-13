using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace moonsharp
{
    class MoonsharpTest
    {
        static void Main(string[] args)
        {
            theBasics();
            holdAScriptReference();
            injectVariables();
            callFunctionDirectly();
            useGetInsteadOfIndexerAndCreateDynValuesDirectly();
            callingBackIntoCSharp();
            luaUsingCSharpIEnumerable();
            accessClass();
            testLuaThrow();
            // for examples using lists and tables, http://www.moonsharp.org/callback.html
        }
        static void testLuaThrow()
        {
            string lua = @"
                ur_nofun()
            ";
            try
            {
                DynValue val = Script.RunString(lua);
                Console.WriteLine(val);
            }
            catch (ScriptRuntimeException e)
            {
                Console.WriteLine($"expected and caught lua/moonsharp exception: {e.Message}");
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
        static void accessClass()
        {
            // create an object of a type Lua knows about
            UserData.RegisterType<SomeClass>();
            UserData.RegisterType<SomeClass.SomeEnum>();

            var someInstance = new SomeClass(2112, "we have assumed control");
            someInstance.someEnum = SomeClass.SomeEnum.First;

            // create an execution context with a C# callback
            var script = new Script();
            Action<string> report = s => { Console.WriteLine($"lua: {s}"); };
            script.Globals["report"] = report;
            script.Globals["SomeEnum"] = UserData.CreateStatic<SomeClass.SomeEnum>();

            // run the script
            string code =
            @"
            function doThings(someInstance)
                report(someInstance.publicNum + 1)
                report(someInstance.publicString .. "" member accessed from lua "")
                report(someInstance.getPrivate() .. "" method called from lua "")

                someInstance.publicNum = 1221
                report(""set publicNum to "" .. someInstance.publicNum)

                -- this throws, correctly
                -- report(someInstance.privateReadonlyString)

                -- someInstance.publicReadonlyString = ""this should throw""
                someInstance.someEnum = SomeEnum.Second
                report(""testing C# enums:"")
                report(SomeEnum.First)
                report(someInstance.someEnum)

                -- tests with List<>
                report("" list count: "" .. someInstance.someList.Count)
            end
            ";
            script.DoString(code);
            script.Call(script.Globals["doThings"], UserData.Create(someInstance));
        }
        static void luaUsingCSharpIEnumerable()
        {
            string lua = @"
                total = 0
                for i in numberSequence() do
                    total = total + i
                end
                return total
            ";

            IEnumerable<int> numberSequence()
            {
                for (int i = 0; i < 10; ++i)
                {
                    yield return i;
                }
            }
            var script = new Script();
            script.Globals["numberSequence"] = (Func<IEnumerable<int>>)numberSequence;

            var res = script.DoString(lua);
            Console.WriteLine(res);
        }
        static void callingBackIntoCSharp()
        {
            string lua = @"
                function getAndAddNumbers()
                    return getnum(1) + getnum(2)
                end
            ";

            var script = new Script();
            Func<int, int> getnum = n => n*2;

            script.Globals["getnum"] = getnum;
            script.DoString(lua);

            var res = script.Call(script.Globals["getAndAddNumbers"]);
            Console.WriteLine(res);
        }
        static void useGetInsteadOfIndexerAndCreateDynValuesDirectly()
        {
            string lua = @"
                function add(x, y)
                    return x + y
                end
            ";
            var script = new Script();
            script.DoString(lua);

            var add = script.Globals.Get("add");
            var result = script.Call(add, DynValue.NewNumber(11), DynValue.NewNumber(12));
            Console.WriteLine(result);
        }
        static void callFunctionDirectly()
        {
            string lua = @"
                function add(x, y)
                    return x + y
                end
            ";
            var script = new Script();
            script.DoString(lua);

            var result = script.Call(script.Globals["add"], 9, 10);
            Console.WriteLine(result);
        }
        static void injectVariables()
        {
            string lua = @"
                function add(x, y)
                    return x + y
                end

                return add(num1, num2)
            ";
            Script script = new Script();
            script.Globals["num1"] = 7;
            script.Globals["num2"] = 8;
            DynValue val = script.DoString(lua);
            Console.WriteLine(val);
        }
        static void holdAScriptReference()
        {
            string lua = @"
                function add(x, y)
                    return x + y
                end
                return add(15, 15)
            ";
            Script script = new Script();
            DynValue val = script.DoString(lua);
            Console.WriteLine(val);
        }
        static void theBasics()
        {
            string lua = @"
                function add(x, y)
                    return x + y
                end
                return add(10, 15)
            ";
            DynValue val = Script.RunString(lua);
            Console.WriteLine(val);
        }
    }
}
