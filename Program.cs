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
        static int repeat = 0;

        static Stack<dynamic> Stack { get; set; } = new Stack<dynamic>();

        static Dictionary<string, Action> Operators = new Dictionary<string, Action> 
        {
            ["+"] = () => { Stack.Push(Stack.Pop() + Stack.Pop()); },
            ["-"] = () => { Stack.Push(Stack.Pop() - Stack.Pop()); },
            ["*"] = () => { Stack.Push(Stack.Pop() * Stack.Pop()); },
            ["/"] = () => { Stack.Push(Stack.Pop() / Stack.Pop()); },

            ["repeat"] = () =>  {repeat = (int)Stack.Pop(); },
            ["dup"] = () => { Stack.Push(Stack.Peek()); },
            
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
                    if (repeat == 0)
                    {
                        Operators[token].Invoke();
                    }
                    else
                    {
                        for (int i = 0; i < repeat; i++)
                        {
                            Operators[token].Invoke();
                        }
                        repeat = 0;
                    }
                }
                catch (KeyNotFoundException)
                {
                    // Not an operator. Should be a value.
                    try
                    {
                        var item = GetValue(token);
                        Stack.Push(item);
                    }
                    catch (Exception ex)
                    {
                        DisplayError(ex.Message);
                    }
                }
                catch (Exception ex)
                {
                    DisplayError(ex.Message);
                }
            }
        }


        static void DisplayStack()
        {
            //!!!!CONSIDER DISPLAY MODES)

            var entries = Stack.ToArray().Reverse();
            foreach (var entry in entries)
            {
                Console.Write(entry + " ");
            }
            Console.Write(prompt);
        }

        static void DisplayError(string error)
        {
            Console.WriteLine($"Error: {error}");
        }

        static Type[] ValueTypes = new Type[] 
        {
//            typeof(int),
  //          typeof(long),
            typeof(double),
        };

        static object GetValue(string token)
        {
            object obj;

            // Try hex, octal, binary.
            obj = ParseHexOctalBinary(token);
            if (obj != null) return obj;

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

            throw new Exception("Invalid input");
        }

        static object ParseHexOctalBinary(string token)
        {
            object obj;

            int _base;
            string t;

            // Try hex, octal, bin.
            if (token.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                t = token.Substring(2);
                _base = 16;
            }
            else if (token.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
            {
                t = token.Substring(2);
                _base = 2;
            }
            else if (token.StartsWith("0"))
            {
                t = token.Substring(1);
                _base = 8;
            }
            else
            {
                return null;
            }

            try
            {
                obj = Convert.ToInt32(t, _base);
                if (obj != null) return (double)obj;
            }
            catch { }
            try
            {
                obj = Convert.ToInt64(t, _base);
                if (obj != null) return (double)obj;
            }
            catch { }

            return null;
        }
    }
}
