using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Principal;

namespace rpn
{


    class Program
    {
        static bool isExit = false;
        static int repeat = 0;
        static string varName = "";

        static Stack<dynamic> Stack { get; set; } = new Stack<dynamic>();

        static Dictionary<string, Action> Operators = new Dictionary<string, Action> 
        {
            // Arithmetic.
            ["+"] = () => { Stack.Push(Stack.Pop() + Stack.Pop()); },
            ["-"] = () => { var x = Stack.Pop();  Stack.Push(Stack.Pop() - x); },
            ["*"] = () => { Stack.Push(Stack.Pop() * Stack.Pop()); },
            ["/"] = () => { var x = Stack.Pop();  Stack.Push(Stack.Pop() / x); },

            // Numeraic utilities.
            ["round"] = () => { Stack.Push(Math.Round(Stack.Pop())); },


            // Constants.
            ["pi"] = () => { Stack.Push(Math.PI); },


            // Mathematic functions.
            ["sqrt"] = () => { Stack.Push(Math.Sqrt(Stack.Pop())); },

            // Stack manipulation.
            ["repeat"] = () => { repeat = (int)Stack.Pop(); },
            ["drop"] = () => { _ = Stack.Pop(); },
            ["dup"] = () => { Stack.Push(Stack.Peek()); },
            ["swap"] = () => { var x = Stack.Pop(); var y = Stack.Pop(); Stack.Push(x); Stack.Push(y); },


            // Macros and variables.
            ["macro"] = () => { },
            ["var"] = () => { Variables.Add(varName.TrimEnd('='), Stack.Pop()); },

            // Other.
            ["exit"] = () => { isExit = true; },


        };

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
                //CheckAndCreateVariables(ref tokens);
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
                try
                {

                    // If operator?
                    if (repeat == 0)
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

                        // Variable preconditioning.
                        if (token.EndsWith("="))
                        {
                            varName = token;
                            Operators["var"].Invoke();
                        }
                        else
                        {
                            Operators[token].Invoke();
                        }
                    }
                    else
                    {
                        for (int i = 0; i < repeat; i++)
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

                            if (token.EndsWith("="))
                            {
                                varName = token;
                                Operators["var"].Invoke();
                            }
                            else
                            {
                                Operators[token].Invoke();
                            }
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

        static void DisplayVariablesAndStack()
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
