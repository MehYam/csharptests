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
            accessingCSharpClass();
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
                Console.WriteLine($"caught lua/moonsharp exception: {e.Message}");
            }
        }
        class SomeGameClass
        {
            public int publicNum;
            public string publicString;
            public readonly string publicReadonlyString;
            private readonly string privateReadonlyString;
            public SomeGameClass(int num, string str)
            {
                publicNum = num;
                publicString = str;
                publicReadonlyString = $"{str} (readonly)";
                privateReadonlyString = $"{str} (private)";
            }
            public string getPrivate() { return privateReadonlyString; }
        }
        static void accessingCSharpClass()
        {
            // create an object of a type Lua knows about
            UserData.RegisterType<SomeGameClass>();
            var someGameObject = new SomeGameClass(2112, "we have assumed control");

            // create an execution context with a C# callback
            var script = new Script();
            Action<string> report = s => { Console.WriteLine($"lua: {s}"); };
            script.Globals["report"] = report;

            // run the script
            string code =
            @"
            function doThings(someGameObject)
                report(someGameObject.publicNum + 1)
                report(someGameObject.publicString .. "" member accessed from lua "")
                report(someGameObject.getPrivate() .. "" method called from lua "")

                someGameObject.publicNum = 1221
                report(""set publicNum to "" .. someGameObject.publicNum)

                -- this throws, correctly
                -- report(someGameObject.privateReadonlyString)

                -- someGameObject.publicReadonlyString = ""this should throw""
            end
            ";
            script.DoString(code);
            script.Call(script.Globals["doThings"], UserData.Create(someGameObject));
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
