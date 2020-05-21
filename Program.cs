using System;
using System.Collections.Generic;
using System.Linq;

namespace rpn
{


    class Program
    {
        static Dictionary<string, Action> Operators = new Dictionary<string, Action> 
        {
            ["+"] = Add,
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

                ParseInput(readLine);

                Operators["+"].Invoke();

            }
        }

        private static void ParseInput(string readLine)
        {
            var tokens = readLine.Split(" ").Where(_ => _ != string.Empty);
            foreach (var token in tokens)
                Console.WriteLine(token);
        }

        private static void DisplayPrompt()
        {
            Console.Write(prompt);
        }

    }
}
