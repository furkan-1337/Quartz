using System;
using Quartz.Parsing;
using Quartz.Runtime;

namespace Quartz.Exceptions
{
    internal static class ErrorReporter
    {
        public static void Error(Token token, string message)
        {
            if (token.Type == TokenType.EndOfFile)
            {
                Report(token.File, token.Line, token.Column, " at end", message);
            }
            else
            {
                Report(token.File, token.Line, token.Column, $" at '{token.Value}'", message);
            }
        }

        public static void Report(string file, int line, int col, string where, string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[Error] {file}:{line}:{col} Error{where}: {message}");

            var stack = Interpreter.Current?.CallStack;
            if (stack != null && stack.Count > 0)
            {
                Console.WriteLine("Quartz Stack Trace:");
                for (int i = stack.Count - 1; i >= 0; i--)
                {
                    var frame = stack[i];
                    Console.WriteLine($"  at {frame.FunctionName} in {frame.File}:line {frame.Line}");
                }
            }

            Console.ResetColor();
        }
    }
}

