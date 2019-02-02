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
            luaCallingCSharp();
            luaUsingIEnumerable();

            // for examples using lists and tables, http://www.moonsharp.org/callback.html

        }
        static void luaUsingIEnumerable()
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
        static void luaCallingCSharp()
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
