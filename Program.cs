﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Numerics;
//using System.Numerics;
using System.Security.Principal;
using System.Text;

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

        enum StackDirection
        {
            Horizontal,
            Vertical,
        }
        static StackDirection stackDirection = StackDirection.Horizontal;

        static Stack<BigFloat> Stack { get; set; } = new Stack<BigFloat>();
        static Dictionary<string, List<string>> Macros = new Dictionary<string, List<string>>();
        static Dictionary<string, BigFloat> Variables = new Dictionary<string, BigFloat>();

        // !!! IMPORTANT !!!
        // C# has builtin BigInteger but not BigFloat support.
        // I looked for 3rd party libs and to my surprise there are not many!!!
        // The best I found so far is Lykke.BigFloat, but this one is missing Trigonometric and 
        // Mathematic functions. 
        // Due to time constrains, I will not add support for these functions, but wrap them to use
        // decimal in extension functions. Please see BigFloat.cs file for implementation.
        // That means there will be no support for very big numbers in these functions.
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
            ["!"] = () => { var x = Stack.Pop(); Stack.Push(x == BigFloat.Zero ? BigFloat.One : BigFloat.Zero); },
            ["!="] = () => { Stack.Push(Stack.Pop() == Stack.Pop() ? BigFloat.Zero : BigFloat.One); },
            ["%"] = () => { var x = Stack.Pop(); Stack.Push(Stack.Pop() % x); },
            ["++"] = () => { var x = Stack.Pop(); x++; Stack.Push(x); },
            ["--"] = () => { var x = Stack.Pop(); x--; Stack.Push(x); },

            //// Bitwise.
            ["&"] = () => { Stack.Push(Stack.Pop().Numerator & Stack.Pop().Numerator); },
            ["|"] = () => { Stack.Push(Stack.Pop().Numerator | Stack.Pop().Numerator); },
            ["^"] = () => { Stack.Push(Stack.Pop().Numerator ^ Stack.Pop().Numerator); },
            ["~"] = () => { Stack.Push(~Stack.Pop()); },
            ////["<<"] = () => { var x = Stack.Pop(); Stack.Push(Stack.Pop() << x); },
            ////            [">>"] = () => { var x = (ulong)Stack.Pop(); Stack.Push((ulong)Stack.Pop() >> x); }, in C# x must be constant

            // Boolean.
            ["&&"] = () => 
            { 
                var x = Stack.Pop().Numerator == BigInteger.Zero ? false : true; 
                var y = Stack.Pop().Numerator == BigInteger.Zero ? false : true; 
                Stack.Push(x && y ? BigFloat.One : BigFloat.Zero); 
            },
            ["||"] = () => 
            { 
                var x = Stack.Pop().Numerator == BigInteger.Zero ? false : true; 
                var y = Stack.Pop().Numerator == BigInteger.Zero ? false : true; 
                Stack.Push(x || y ? BigFloat.One : BigFloat.Zero); 
            },
            ["^^"] = () => 
            { 
                var x = Stack.Pop().Numerator == BigFloat.Zero ? false : true; 
                var y = Stack.Pop().Numerator == BigFloat.Zero ? false : true; 
                Stack.Push(x ^ y ? BigFloat.One : BigFloat.Zero); 
            },

            // Comparison.
            ["<"] = () => { var x = Stack.Pop(); Stack.Push(Stack.Pop() < x ? BigFloat.One : BigFloat.Zero); },
            ["<="] = () => { var x = Stack.Pop(); Stack.Push(Stack.Pop() <= x ? BigFloat.One : BigFloat.Zero); },
            ["=="] = () => { Stack.Push(Stack.Pop() == Stack.Pop() ? BigFloat.One : BigFloat.Zero); },
            [">"] = () => { var x = Stack.Pop(); Stack.Push(Stack.Pop() > x ? BigFloat.One : BigFloat.Zero); },
            [">="] = () => { var x = Stack.Pop(); Stack.Push(Stack.Pop() >= x ? BigFloat.One : BigFloat.Zero); },

            //Trigonometric functions.
            //["acos"] = () => { Stack.Push((BigFloat)Math.Acos((Math.PI / 180) * (decimal)Stack.Pop())); },
            //["asin"] = () => { Stack.Push((decimal)Math.Asin((Math.PI / 180) * (double)Stack.Pop())); },
            //["atan"] = () => { Stack.Push((decimal)Math.Atan((Math.PI / 180) * (double)Stack.Pop())); },
            //["cos"] = () => { Stack.Push((decimal)Math.Cos((Math.PI / 180) * (double)Stack.Pop())); },
            //["cosh"] = () => { Stack.Push((decimal)Math.Cosh((Math.PI / 180) * (double)Stack.Pop())); },
            //["sin"] = () => { Stack.Push(Math.Sin((Math.PI / 180) * Stack.Pop())); },
            //["sinh"] = () => { Stack.Push((decimal)Math.Sinh((Math.PI / 180) * (double)Stack.Pop())); },
            //["tanh"] = () => { Stack.Push((decimal)Math.Tanh((Math.PI / 180) * (double)Stack.Pop())); },


            //// Numeric utilities.
            ["ceil"] = () => { Stack.Push(BigFloat.Ceil(Stack.Pop())); },
            ["floor"] = () => { Stack.Push(BigFloat.Floor(Stack.Pop())); },
            ["round"] = () => { Stack.Push(BigFloat.Round(Stack.Pop())); },
            ["ip"] = () => { Stack.Push(BigFloat.Floor(Stack.Pop())); },
            ["fp"] = () => { var x = Stack.Pop(); Stack.Push(x - BigFloat.Floor(x)); },
            ["sign"] = () => { var x = Stack.Pop(); if (x >= BigFloat.Zero) x = BigFloat.Zero; else x = -1;  Stack.Push(x); },
            ["abs"] = () => { Stack.Push(BigFloat.Abs(Stack.Pop())); },
//            ["max"] = () => { Stack.Push(BigFloat.Max(Stack.Pop(), Stack.Pop())); },
  //          ["min"] = () => { Stack.Push(BigFloat.Min(Stack.Pop(), Stack.Pop())); },

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
//            ["exp"] = () => { Stack.Push(BigFloat..Exp(Stack.Pop())); },
            //["fact"] = () => { Stack.Push((decimal) Factorial((ulong)Stack.Pop())); },
            ["sqrt"] = () => { Stack.Push(BigFloat.Sqrt(Stack.Pop())); },
            ["ln"] = () => { Stack.Push(BigFloat.Log(Stack.Pop(),2)); },
            ["log"] = () => { Stack.Push(BigFloat.Log10((double)Stack.Pop())); },
            //["pow"] = () => { var x = Stack.Pop(); Stack.Push(BigFloat.Pow(Stack.Pop(), x)); },

            //// Networking.
            //["hnl"] = () => { Stack.Push(IPAddress.HostToNetworkOrder((long)Stack.Pop())); },
            //["hns"] = () => { Stack.Push(IPAddress.HostToNetworkOrder((short)Stack.Pop())); },
            //["nhl"] = () => { Stack.Push(IPAddress.NetworkToHostOrder((long)Stack.Pop())); },
            //["nhs"] = () => { Stack.Push(IPAddress.NetworkToHostOrder((short)Stack.Pop())); },

            // Stack manipulation.
            //["pick"] = () => { var entries = Stack.Reverse().ToArray(); Stack.Push(entries[(int)Stack.Peek()]); },
            //["repeat"] = () => { repeat = (int)Stack.Pop(); },
            ["pick"] = () => { var entries = Stack.Reverse().ToArray(); Stack.Push(entries.Length); },
            ["drop"] = () => { _ = Stack.Pop(); },
            //["dropn"] = () => { var x = (int)Stack.Pop(); for (int i=0; i<x; i++)  _ = Stack.Pop();  },
            ["dup"] = () => { Stack.Push(Stack.Peek()); },
            ["swap"] = () => { var x = Stack.Pop(); var y = Stack.Pop(); Stack.Push(x); Stack.Push(y); },
            // TODO:
            //["roll"]
            //["rolln"]
            ["stack"] = () => 
            {
                if (stackDirection == StackDirection.Horizontal) 
                    stackDirection = StackDirection.Vertical;
                else 
                    stackDirection = StackDirection.Horizontal;
            },

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
                try
                {
                    var tokens = args;
                    CheckAndCreateMacro(ref tokens);
                    Execute(tokens);
                    Display(false);
                }
                catch (Exception ex)
                {
                    DisplayError(ex.Message);
                }
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

                try
                {
                    var tokens = ParseInput(readLine);
                    CheckAndCreateMacro(ref tokens);
                    Execute(tokens);
                }
                catch(Exception ex)
                {
                    DisplayError(ex.Message);
                }
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
            }
        }

        static void Display(bool isPromptVisible = true)
        {
            // First variables.
            foreach(var variable in Variables)
            {
                if(stackDirection == StackDirection.Horizontal)
                    Console.Write($"[ {variable.Key}={Format(variable.Value)} ] ");
                else
                    Console.WriteLine($"[ {variable.Key}={Format(variable.Value)} ] ");
            }

            var entries = Stack.ToArray().Reverse();
            foreach (var entry in entries)
            {
                if (stackDirection == StackDirection.Horizontal)
                    Console.Write(Format(entry) + " ");
                else
                    Console.WriteLine(Format(entry) + " ");
            }
            if (isPromptVisible)
                Console.Write(prompt);
        }

        static string Format(BigFloat item)
        {
            return displayMode switch
            {
                DisplayMode.Dec => $"{item}",
                DisplayMode.Hex => $"0x{item.Numerator:x}",
                DisplayMode.Oct => $"0{ToBaseString(item.Numerator, 8)}",
                DisplayMode.Bin => $"0b{ToBaseString(item.Numerator, 2)}",
                _ => $"{item}",
            };

            string ToBaseString(BigInteger bi, int n)
            {
                StringBuilder sb = new StringBuilder();
                while (bi > 0)
                {
                    sb.Insert(0, bi % n);
                    bi /= n;
                }
                return sb.ToString();
            }
        }

        static void DisplayError(string error)
        {
            Console.WriteLine($"Error: {error}");
        }

        static BigFloat GetValue(string token)
        {
            BigFloat? val;

            // Try hex, octal, binary.
            val = ParseHexOctalBinary(token);
            if (val != null) return (BigFloat)val;

            val = BigFloat.Parse(token);
            if (val != null) return (BigFloat)val;

            throw new Exception("Invalid input");
        }

        static BigInteger? ParseHexOctalBinary(string token)
        {
            try
            {
                // Try hex, octal, bin.
                if (token.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    return BigInteger.Parse("0" + token.Substring(2), System.Globalization.NumberStyles.HexNumber);
                }
                else if (token.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
                {
                    return BinToBigInteger(token.Substring(2));
                }
                else if (token.StartsWith("0"))
                {
                    return OctalToBigInteger(token.Substring(1));
                }
                else
                {
                    return null;
                }
            }
            catch { }

            return null;

            BigInteger BinToBigInteger(string token)
            {
                if (token.All(_ => _ >= '0' && _ <= '1'))
                {
                    return token.Aggregate(new BigInteger(), (b, c) => b * 2 + c - '0');
                }
                throw new Exception("Bad binary format");
            }

            BigInteger OctalToBigInteger(string token)
            {
                if (token.All(_ => _ >= '0' && _ <= '7'))
                {
                    return token.Aggregate(new BigInteger(), (b, c) => b * 8 + c - '0');
                }
                throw new Exception("Bad octal format");
            }
        }

        static void Help()
        {
            Console.WriteLine("\t" + "Reverse polish notation (rpn) calculator.");
            Console.WriteLine();
            Console.WriteLine(operators);
        }

        static BigInteger Factorial(ulong f)
        {
            var bi = new BigInteger(1);
            for(var i=1u; i<=f; i++ )
            {
                bi *= i;
            }
            //Console.WriteLine(bi);
            return bi;
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
