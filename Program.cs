using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Security.Principal;

namespace rpn
{


    class Program
    {
        const string prompt = "> ";
        static bool isExit = false;
        static int repeat = 1;
        static string varName = "";

        enum DisplayMode
        {
            Dec,
            Hex,
            Bin,
            Oct,
        };
        static DisplayMode displayMode = DisplayMode.Dec;

        static Stack<decimal> Stack { get; set; } = new Stack<decimal>();
        static Dictionary<string, List<string>> Macros = new Dictionary<string, List<string>>();
        static Dictionary<string, decimal> Variables = new Dictionary<string, decimal>();

        static Dictionary<string, Action> Operators = new Dictionary<string, Action>
        {
            // Arithmetic.
            ["+"] = () => { Stack.Push(Stack.Pop() + Stack.Pop()); },
            ["-"] = () => { var x = Stack.Pop(); Stack.Push(Stack.Pop() - x); },
            ["*"] = () => { Stack.Push(Stack.Pop() * Stack.Pop()); },
            ["/"] = () => { var x = Stack.Pop(); Stack.Push(Stack.Pop() / x); },
            ["cla"] = () => { Stack.Clear(); Variables.Clear(); Macros.Clear(); },
            ["clr"] = () => { Stack.Clear(); },
            ["clv"] = () => { Variables.Clear(); Macros.Clear(); },
            ["!"] = () => { var x = (ulong)Stack.Pop(); Stack.Push(x == 0 ? 1 : 0); },
            ["!="] = () => { Stack.Push((ulong)Stack.Pop() == (ulong)Stack.Pop() ? 0 : 1); },
            ["%"] = () => { var x = (ulong)Stack.Pop(); Stack.Push((ulong)Stack.Pop() % x); },
            ["++"] = () => { var x = Stack.Pop(); x++; Stack.Push(x); },
            ["--"] = () => { var x = Stack.Pop(); x--; Stack.Push(x); },

            // Bitwise.
            ["&"] = () => { Stack.Push((ulong)Stack.Pop() & (ulong)Stack.Pop()); },
            ["|"] = () => { Stack.Push((ulong)Stack.Pop() | (ulong)Stack.Pop()); },
            ["^"] = () => { Stack.Push((ulong)Stack.Pop() ^ (ulong)Stack.Pop()); },
            ["~"] = () => { Stack.Push(~(ulong)Stack.Pop()); },
            ////            ["<<"] = () => { var x = (ulong)Stack.Pop(); Stack.Push((ulong)Stack.Pop() << x); }, in C# x must be constant
            ////            [">>"] = () => { var x = (ulong)Stack.Pop(); Stack.Push((ulong)Stack.Pop() >> x); }, in C# x must be constant

            // Boolean.
            ["&&"] = () => { var x = Stack.Pop() == 0 ? false : true; var y = Stack.Pop() == 0 ? false : true; Stack.Push(x && y ? 1 : 0); },
            ["||"] = () => { var x = Stack.Pop() == 0 ? false : true; var y = Stack.Pop() == 0 ? false : true; Stack.Push(x || y ? 1 : 0); },
            ["^^"] = () => { var x = Stack.Pop() == 0 ? false : true; var y = Stack.Pop() == 0 ? false : true; Stack.Push(x ^ y ? 1 : 0); },

            // Comparison.
            ["<"] = () => { var x = Stack.Pop(); Stack.Push(Stack.Pop() < x ? 1 : 0); },
            ["<="] = () => { var x = Stack.Pop(); Stack.Push(Stack.Pop() <= x ? 1 : 0); },
            ["=="] = () => { Stack.Push(Stack.Pop() == Stack.Pop() ? 1 : 0); },
            [">"] = () => { var x = Stack.Pop(); Stack.Push(Stack.Pop() > x ? 1 : 0); },
            [">="] = () => { var x = Stack.Pop(); Stack.Push(Stack.Pop() >= x ? 1 : 0); },

            //Trigonometric functions.
            ["acos"] = () => { Stack.Push((decimal)Math.Acos((Math.PI / 180) * (double)Stack.Pop())); },
            ["asin"] = () => { Stack.Push((decimal)Math.Asin((Math.PI / 180) * (double)Stack.Pop())); },
            ["atan"] = () => { Stack.Push((decimal)Math.Atan((Math.PI / 180) * (double)Stack.Pop())); },
            ["cos"] = () => { Stack.Push((decimal)Math.Cos((Math.PI / 180) * (double)Stack.Pop())); },
            ["cosh"] = () => { Stack.Push((decimal)Math.Cosh((Math.PI / 180) * (double)Stack.Pop())); },
            ["sin"] = () => { Stack.Push((decimal)Math.Sin((Math.PI / 180) * (double)Stack.Pop())); },
            ["sinh"] = () => { Stack.Push((decimal)Math.Sinh((Math.PI / 180) * (double)Stack.Pop())); },
            ["tanh"] = () => { Stack.Push((decimal)Math.Tanh((Math.PI / 180) * (double)Stack.Pop())); },


            // Numeric utilities.
            ["ceil"] = () => { Stack.Push(Math.Ceiling(Stack.Pop())); },
            ["floor"] = () => { Stack.Push(Math.Floor(Stack.Pop())); },
            ["round"] = () => { Stack.Push(Math.Round(Stack.Pop())); },
            ["ip"] = () => { Stack.Push(Math.Truncate(Stack.Pop())); },
            ["fp"] = () => { var x = Stack.Pop(); Stack.Push(x - Math.Floor(x)); },
            ["sign"] = () => { var x = Stack.Pop(); if (x > 0) x = 1; else if (x < 0) x = -1; else x = 0; Stack.Push(x); },
            ["abs"] = () => { Stack.Push(Math.Abs(Stack.Pop())); },
            ["max"] = () => { Stack.Push(Math.Max(Stack.Pop(), Stack.Pop())); },
            ["max"] = () => { Stack.Push(Math.Min(Stack.Pop(), Stack.Pop())); },

            // Display modes.
            ["hex"] = () => { displayMode = DisplayMode.Hex; },
            ["dec"] = () => { displayMode = DisplayMode.Dec; },
            ["bin"] = () => { displayMode = DisplayMode.Bin; },
            ["oct"] = () => { displayMode = DisplayMode.Oct; },

            // Constants.
            ["pi"] = () => { Stack.Push((decimal)Math.PI); },
            ["e"] = () => { Stack.Push((decimal)Math.E); },
            ["rand"] = () => { Stack.Push((decimal)new Random().NextDouble()); },

            //// Mathematic functions.
            ["exp"] = () => { Stack.Push((decimal)Math.Exp((double)Stack.Pop())); },
            ["fact"] = () => { Stack.Push((decimal)Factorial((long)Stack.Pop())); },
            ["sqrt"] = () => { Stack.Push((decimal)Math.Sqrt((double)Stack.Pop())); },
            ["exp"] = () => { Stack.Push((decimal)Math.Log2((double)Stack.Pop())); },
            ["log"] = () => { Stack.Push((decimal)Math.Log((double)Stack.Pop())); },
            ["pow"] = () => { var x = (double)Stack.Pop(); Stack.Push((decimal)Math.Pow((double)Stack.Pop(), x)); },

            //// Networking.
            ["hnl"] = () => { Stack.Push(IPAddress.HostToNetworkOrder((long)Stack.Pop())); },
            ["hns"] = () => { Stack.Push(IPAddress.HostToNetworkOrder((short)Stack.Pop())); },
            ["nhl"] = () => { Stack.Push(IPAddress.NetworkToHostOrder((long)Stack.Pop())); },
            ["nhs"] = () => { Stack.Push(IPAddress.NetworkToHostOrder((short)Stack.Pop())); },

            // Stack manipulation.
            ["pick"] = () => { var entries = Stack.Reverse().ToArray(); Stack.Push(entries[(int)Stack.Peek()]); },
            ["repeat"] = () => { repeat = (int)Stack.Pop(); },
            ["pick"] = () => { var entries = Stack.Reverse().ToArray(); Stack.Push(entries.Length); },
            ["drop"] = () => { _ = Stack.Pop(); },
            ["dropn"] = () => { var x = (int)Stack.Pop(); for (int i=0; i<x; i++)  _ = Stack.Pop();  },
            ["dup"] = () => { Stack.Push(Stack.Peek()); },
            ["swap"] = () => { var x = Stack.Pop(); var y = Stack.Pop(); Stack.Push(x); Stack.Push(y); },
            // TODO:
            //["roll"]
            //["rolln"]
            //["stack"]

            // Macros and variables.
            ["macro"] = () => { },
            ["var"] = () => { Variables.Add(varName.TrimEnd('='), Stack.Pop()); },

            // Other.
            ["exit"] = () => { isExit = true; },
            ["help"] = () => { Help(); },
        };


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
                var tokens = args;
                CheckAndCreateMacro(ref tokens);
                Execute(tokens);
                Display(false);
            }
        }

        static void CommandLoop()
        {
            while (!isExit)
            {
                Display();

                var readLine = Console.ReadLine();
                if(readLine == null)
                {
                    // CTRL-C.
                    isExit = true;
                }

                var tokens = ParseInput(readLine);
                CheckAndCreateMacro(ref tokens);
                Execute(tokens);
            }
        }

        static string[] ParseInput(string readLine)
        {
            var tokens = readLine.Split(" ").Where(_ => _ != string.Empty).ToArray();
            return tokens;
        }

        static void CheckAndCreateMacro(ref string[] tokens)
        {

            var index = Array.FindIndex(tokens, _ => _.Equals("macro"));
            if(index != -1)
            {
                var clone = new List<string>();
                clone.AddRange(tokens.Take(index));

                // Take the rest as macro.
                var macro = tokens.Skip(index).ToArray();
                if (macro.Length > 1)
                {
                    var macroName = macro[1];
                    Macros.Add(macroName, new List<string>());
                    foreach (var m in macro.Skip(2))
                    {
                        Macros[macroName].Add(m);
                    }
                }
                tokens = clone.ToArray();
            }
        }

        static void Execute(string[] tokens)
        {
            foreach (var token in tokens)
            {
                var localRepeat = repeat;
                try
                {
                    for (int i = 0; i < localRepeat; i++)
                    {
                        // First check if it is macro.
                        if (Macros.ContainsKey(token))
                        {
                            // Execute macro recursive.
                            Execute(Macros[token].ToArray());
                            continue;
                        }

                        // Then check if variable.
                        if (Variables.ContainsKey(token))
                        {
                            // Use variable value directly.
                            Stack.Push(Variables[token]);
                            continue;
                        }

                        if (token.EndsWith("=") && token.Length > 1 && char.IsLetter(token[^2]))
                        {
                            varName = token;
                            Operators["var"].Invoke();
                        }
                        else
                        {
                            Operators[token].Invoke();
                        }
                    }

                    if (localRepeat > 1)
                        repeat = 1;
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

        static void Display(bool isPromptVisible = true)
        {
            //!!!!CONSIDER DISPLAY MODES)

            // First variables.
            foreach(var variable in Variables)
            {
                Console.Write($"[ {variable.Key}={Format(variable.Value)} ] ");
            }

            var entries = Stack.ToArray().Reverse();
            foreach (var entry in entries)
            {
                Console.Write(Format(entry) + " ");
            }
            if(isPromptVisible)
                Console.Write(prompt);
        }

        static string Format(decimal item)
        {
            return displayMode switch
            {
                DisplayMode.Dec => $"{item}",
                DisplayMode.Hex => $"0x{Convert.ToUInt64(item):x}",
                DisplayMode.Oct => $"0{Convert.ToString((long)item, 8)}",
                DisplayMode.Bin => $"0b{Convert.ToString((long)item, 2)}",
                _ => $"{item}",
            };
        }

        static void DisplayError(string error)
        {
            Console.WriteLine($"Error: {error}");
        }

        static decimal GetValue(string token)
        {
            decimal? val;

            // Try hex, octal, binary.
            val = ParseHexOctalBinary(token);
            if (val != null) return (decimal)val;

            val = (decimal?)Convert.ChangeType(token, typeof(decimal));
            if (val != null) return (decimal)val;

            throw new Exception("Invalid input");
        }

        static decimal? ParseHexOctalBinary(string token)
        {
            decimal? val;

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
                // Hex, octal and binary uses long.
                val = Convert.ToInt64(t, _base);
                if (val != null) return (decimal)val;
            }
            catch { }

            return null;
        }

        static void Help()
        {
            Console.WriteLine("\t" + "Reverse polish notation (rpn) calculator.");
            Console.WriteLine();
            Console.WriteLine(operators);
        }

        static long Factorial(long f)
        {
            if (f == 0)
                return 1;
            else
                return f * Factorial(f - 1);
        }



        const string operators =
        @"
        Arithmetic Operators

            +          Add
            -          Subtract
            *          Multiply
            /          Divide
            cla        Clear the stack and variables
            clr        Clear the stack
            clv        Clear the variables
            !          Boolean NOT
            !=         Not equal to
            %          Modulus
            ++         Increment
            --         Decrement

        Bitwise Operators

            &          Bitwise AND
            |          Bitwise OR
            ^          Bitwise XOR
            ~          Bitwise NOT
            <<         Bitwise shift left
            >>         Bitwise shift right

        Boolean Operators

            &&         Boolean AND
            ||         Boolean OR
            ^^         Boolean XOR

        Comparison Operators

            <          Less than
            <=         Less than or equal to
            ==         Equal to
            >          Greater than
            >=         Greater than or equal to

        Trigonometric Functions

            acos       Arc Cosine
            asin       Arc Sine
            atan       Arc Tangent
            cos        Cosine
            cosh       Hyperbolic Cosine
            sin        Sine
            sinh       Hyperbolic Sine
            tanh       Hyperbolic tangent

        Numeric Utilities

            ceil       Ceiling
            floor      Floor
            round      Round
            ip         Integer part
            fp         Floating part
            sign       Push -1, 0, or 0 depending on the sign
            abs        Absolute value
            max        Max
            min        Min

        Display Modes

            hex        Switch display mode to hexadecimal
            dec        Switch display mode to decimal (default)
            bin        Switch display mode to binary
            oct        Switch display mode to octal

        Constants

            e          Push e
            pi         Push Pi
            rand       Generate a random number

        Mathematic Functions

            exp        Exponentiation
            fact       Factorial
            sqrt       Square Root
            ln         Natural Logarithm
            log        Logarithm
            pow        Raise a number to a power

        Networking

            hnl        Host to network long
            hns        Host to network short
            nhl        Network to host long
            nhs        Network to host short

        Stack Manipulation

            pick       Pick the -n'th item from the stack
            repeat     Repeat an operation n times, e.g. '3 repeat +'
            depth      Push the current stack depth
            drop       Drops the top item from the stack
            dropn      Drops n items from the stack
            dup        Duplicates the top stack item
            dupn       Duplicates the top n stack items in order
            roll       Roll the stack upwards by n
            rolld      Roll the stack downwards by n
            stack      Toggles stack display from horizontal to vertical
            swap       Swap the top 2 stack items

        Macros and Variables

            macro      Defines a macro, e.g. 'macro kib 1024 *'
            x=         Assigns a variable, e.g. '1024 x='

        Other

            help       Print the help message
            exit       Exit the calculator
        ";



    }
}
