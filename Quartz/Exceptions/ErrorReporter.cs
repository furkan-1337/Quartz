using System;
using Quartz.Parsing;

namespace Quartz.Exceptions
{
    internal static class ErrorReporter
    {
        public static void Error(Token token, string message)
        {
            if (token.Type == TokenType.EndOfFile)
            {
                Report(token.Line, token.Column, " at end", message);
            }
            else
            {
                Report(token.Line, token.Column, $" at '{token.Value}'", message);
            }
        }

        public static void Report(int line, int col, string where, string message)
        {
            Console.WriteLine($"[Error] Error{where} (Line {line}, Column {col}): {message}");
        }
    }
}

