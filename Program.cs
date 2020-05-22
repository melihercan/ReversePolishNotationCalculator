using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace rpn
{


    class Program
    {
        static Stack<object> Stack { get; set; } = new Stack<object>(/*100*/);

        static Dictionary<string, Action> Operators = new Dictionary<string, Action> 
        {
            ["+"] = () => { Console.WriteLine("add is called"); },
            ["-"] = () => { Console.WriteLine("substract is called"); },
        };

        private static void Add()
        {
            Console.WriteLine("add is called");
        }

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
            bool isExit = false;
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
      //Console.WriteLine(token);

                try
                {
                    // If operator?
                    Operators[token].Invoke();
                }
                catch (KeyNotFoundException)
                {
                    // Not an operator. Could be anything, find it out.
                    try
                    {
                        var item = GetObject(token);
       Console.WriteLine($"{item.GetType()}: {item}");
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

            Console.Write(prompt);
        }

        private static void DisplayError(string error)
        {
            Console.WriteLine($"Error: {error}");
        }


        static Type[] ValueTypes = new Type[] 
        {
            typeof(int),
            typeof(long),
            typeof(double),
        };

        static int[] Bases = new int[]
        {
            2,
            8,
            16
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

            //Type type = Type.GetType(token);
            //var converter = TypeDescriptor.GetConverter(type);
            //obj = converter.ConvertFromString(token);

            throw new Exception("Invalid input");
        }

    }
}
