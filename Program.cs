using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace rpn
{


    class Program
    {
        static bool isExit = false;

        static Stack<dynamic> Stack { get; set; } = new Stack<object>();

        static Dictionary<string, Action> Operators = new Dictionary<string, Action> 
        {
            ["+"] = () => { Stack.Push(Stack.Pop() + Stack.Pop()); },
            ["-"] = () => { Stack.Push(Stack.Pop() - Stack.Pop()); },
            ["*"] = () => { Stack.Push(Stack.Pop() * Stack.Pop()); },
            ["/"] = () => { Stack.Push(Stack.Pop() / Stack.Pop()); },



            ["exit"] = () => { isExit = true; },

        };

        const string prompt = "> ";

        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                // Interactive.
                CommandLoop();
            }
            else
            {
                // Evaluate one line expression and exit.

            }


        }

        static void CommandLoop()
        {
            while (!isExit)
            {
                DisplayStack();
                var readLine = Console.ReadLine();
                if(readLine == null)
                {
                    // CTRL-C.
                    isExit = true;
                }

                ParseInputAndExecute(readLine);


            }
        }

        private static void ParseInputAndExecute(string readLine)
        {
            var tokens = readLine.Split(" ").Where(_ => _ != string.Empty);
            foreach (var token in tokens)
            {
                try
                {
                    // If operator?
                    Operators[token].Invoke();
                }
                catch (KeyNotFoundException)
                {
                    // Not an operator. Should be a value.
                    try
                    {
                        var item = GetObject(token);
                        Stack.Push(item);
                    }
                    catch (Exception ex)
                    {
                        DisplayError(ex.Message);
                    }
                }
            }
        }

        private static void DisplayStack()
        {
            //!!!!CONSIDER DISPLAY MODES)

            var entries = Stack.ToArray().Reverse();
            foreach (var entry in entries)
            {
                Console.Write(entry + " ");
            }
            Console.Write(prompt);
        }

        private static void DisplayError(string error)
        {
            Console.WriteLine($"Error: {error}");
        }


        static Type[] ValueTypes = new Type[] 
        {
//            typeof(int),
  //          typeof(long),
            typeof(double),
        };

        private static object GetObject(string token)
        {
            object obj = null;

            // Try hex, octal, bin.
            if (token.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                var t = token.Substring(2);
                try
                {
                    obj = Convert.ToInt32(t, 16);
                    if (obj != null) return obj;
                }
                catch { }
                try
                {
                    obj = Convert.ToInt64(t, 16);
                    if (obj != null) return obj;
                }
                catch { }
            }
            else if (token.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
            {
                var t = token.Substring(2);
                try
                {
                    obj = Convert.ToInt32(t, 2);
                    if (obj != null) return obj;
                }
                catch { }
                try
                {
                    obj = Convert.ToInt64(t, 2);
                    if (obj != null) return obj;
                }
                catch { }
            }
            else if (token.StartsWith("0"))
            {
                var t = token.Substring(1);
                try
                {
                    obj = Convert.ToInt32(t, 8);
                    if (obj != null) return obj;
                }
                catch { }
                try
                {
                    obj = Convert.ToInt64(t, 8);
                    if (obj != null) return obj;
                }
                catch { }
            }
            else
            {
                // Try value types.
                foreach (var type in ValueTypes)
                {
                    try
                    {
                        obj = Convert.ChangeType(token, type);
                        if (obj != null) return obj;
                    }
                    catch { }
                }
            }

            throw new Exception("Invalid input");
        }
    }
}
