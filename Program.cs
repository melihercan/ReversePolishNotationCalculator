using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace rpn
{


    class Program
    {
        static Stack<object> Stack { get; set; } = new Stack<object>(100);

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
      Console.WriteLine(token);

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

        private static object GetObject(string token)
        {
            object obj = null;

            Type type = Type.GetType(token);
            var converter = TypeDescriptor.GetConverter(type);
            obj = converter.ConvertFromString(token);

            return obj;
        }

    }
}
