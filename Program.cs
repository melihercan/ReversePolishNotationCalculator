using System;
using System.Collections.Generic;
using System.Linq;

namespace rpn
{


    class Program
    {
        static Stack<object> Stack { get; set; } = new Stack<object> { };

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
                DisplayPrompt();
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
                    // TODDO: try parse convert to ValueTypes (!!!!CONSIDER DISPLAY MODES)
                }
            }
        }

        private static void DisplayPrompt()
        {
            //!!!!CONSIDER DISPLAY MODES)
            Console.Write(prompt);
        }

    }
}
