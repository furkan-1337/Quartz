using System;
using System.Collections.Generic;
using System.IO;
using Quartz.AST;
using Quartz.Parsing;
using Quartz.Runtime;
using Quartz.Interfaces;

namespace Quartz.Runtime.Native
{
    internal class LoadFunction : ICallable
    {
        public int Arity() => 1;

        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            string path = arguments[0]?.ToString() ?? "";
            string fullPath = Path.GetFullPath(path);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"File not found: {path}");
            }

            string content = File.ReadAllText(fullPath);

            try
            {
                Lexer lexer = new Lexer(content);
                lexer.Tokenize();
                Parser parser = new Parser(lexer.Tokens);
                List<Stmt> statements = parser.Parse();

                interpreter.Interpret(statements);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading '{path}': {ex.Message}");
            }

            return null;
        }

        public override string ToString() => "<native fn load>";
    }
}
