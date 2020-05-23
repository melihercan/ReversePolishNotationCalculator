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

        static Stack<dynamic> Stack { get; set; } = new Stack<dynamic>();

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
            ["!"] = () => { var x = Stack.Pop(); Stack.Push(x == 0 ? 1 : 0); },
            ["!="] = () => { Stack.Push(Stack.Pop() == Stack.Pop() ? 0 : 1); },
            ["%"] = () => { var x = Stack.Pop(); Stack.Push(Stack.Pop() % x); },
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
            ["acos"] = () => { Stack.Push(Math.Acos((Math.PI / 180) * Stack.Pop())); },
            ["asin"] = () => { Stack.Push(Math.Asin((Math.PI / 180) * Stack.Pop())); },
            ["atan"] = () => { Stack.Push(Math.Atan((Math.PI / 180) * Stack.Pop())); },
            ["cos"] = () => { Stack.Push(Math.Cos((Math.PI / 180) * Stack.Pop())); },
            ["cosh"] = () => { Stack.Push(Math.Cosh((Math.PI / 180) * Stack.Pop())); },
            ["sin"] = () => { Stack.Push(Math.Sin((Math.PI / 180) * Stack.Pop())); },
            ["sinh"] = () => { Stack.Push(Math.Sinh((Math.PI / 180) * Stack.Pop())); },
            ["tanh"] = () => { Stack.Push(Math.Tanh((Math.PI / 180) * Stack.Pop())); },


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


            // Constants.
            ["pi"] = () => { Stack.Push(Math.PI); },
            ["e"] = () => { Stack.Push(Math.E); },
            ["rand"] = () => { Stack.Push(new Random().NextDouble()); },

            // Mathematic functions.
            ["exp"] = () => { Stack.Push(Math.Exp(Stack.Pop())); },
            ["fact"] = () => { Stack.Push(Factorial((long)Stack.Pop())); },
            ["sqrt"] = () => { Stack.Push(Math.Sqrt(Stack.Pop())); },
            ["exp"] = () => { Stack.Push(Math.Log2(Stack.Pop())); },
            ["log"] = () => { Stack.Push(Math.Log(Stack.Pop())); },
            ["pow"] = () => { var x = Stack.Pop(); Stack.Push(Math.Pow(Stack.Pop(), x)); },

            // Networking.
            ["hnl"] = () => { Stack.Push(Math.Log(IPAddress.HostToNetworkOrder((long)Stack.Pop()))); },
            ["hns"] = () => { Stack.Push(Math.Log(IPAddress.HostToNetworkOrder((short)Stack.Pop()))); },
            ["nhl"] = () => { Stack.Push(Math.Log(IPAddress.NetworkToHostOrder((long)Stack.Pop()))); },
            ["nhs"] = () => { Stack.Push(Math.Log(IPAddress.NetworkToHostOrder((short)Stack.Pop()))); },

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
            //// TODO: add help content
            ["help"] = () => {  },


        };

        static long Factorial(long f)
        {
            if (f == 0) 
                return 1;
            else 
                return f * Factorial(f - 1);
        }

        static Dictionary<string, List<string>> Macros = new Dictionary<string, List<string>>();
        static Dictionary<string, object> Variables = new Dictionary<string, object>();


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
                Execute(args);
                DisplayVariablesAndStack(false);
            }
        }

        static void CommandLoop()
        {
            while (!isExit)
            {
                DisplayVariablesAndStack();

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


                    //// If operator?
                    //if (repeat == 0)
                    //{
                    //    // First check if it is macro.
                    //    if (Macros.ContainsKey(token))
                    //    {
                    //        // Execute macro recursive.
                    //        Execute(Macros[token].ToArray());
                    //        continue;
                    //    }

                    //    // Then check if variable.
                    //    if (Variables.ContainsKey(token))
                    //    {
                    //        // Use variable value.
                    //        Stack.Push(Variables[token]);
                    //        continue;
                    //    }

                    //    // Variable preconditioning.
                    //    if (token.EndsWith("=") && token.Length > 1 && token[token.Length-2]>0x41)
                    //    {
                    //        varName = token;
                    //        Operators["var"].Invoke();
                    //    }
                    //    else
                    //    {
                    //        Operators[token].Invoke();
                    //    }
                    //}
                    //else
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
                                // Use variable value.
                                Stack.Push(Variables[token]);
                                continue;
                            }

                            if (token.EndsWith("=") && token.Length > 1 && token[token.Length - 2] > 0x41)
                            {
                                varName = token;
                                Operators["var"].Invoke();
                            }
                            else
                            {
                                Operators[token].Invoke();
                            }
                        }

                        if(localRepeat > 1)
                            repeat = 1;
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

        static void DisplayVariablesAndStack(bool isPromptVisible = true)
        {
            //!!!!CONSIDER DISPLAY MODES)

            // First variables.
            foreach(var variable in Variables)
            {
                Console.Write($"[ {variable.Key}={variable.Value} ] ");
            }

            var entries = Stack.ToArray().Reverse();
            foreach (var entry in entries)
            {
                Console.Write(entry + " ");
            }
            if(isPromptVisible)
                Console.Write(prompt);
        }

        static void DisplayError(string error)
        {
            Console.WriteLine($"Error: {error}");
        }

        static object GetValue(string token)
        {
            object obj;

            // Try hex, octal, binary.
            obj = ParseHexOctalBinary(token);
            if (obj != null) return obj;

            obj = Convert.ChangeType(token, typeof(double));
            if (obj != null) return obj;

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
