using System;
using System.Collections.Generic;
using System.IO;
using Quartz.AST;
using Quartz.Parsing;
using Quartz.Runtime;
using Quartz.Interfaces;

namespace Quartz.Runtime.Native
{
    internal class ImportFunction : ICallable
    {
        public int Arity() => 1;
        private Dictionary<string, Module> cache = new Dictionary<string, Module>();

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            string path = (string)arguments[0];
            string fullPath = Path.GetFullPath(path);

            if (cache.ContainsKey(fullPath))
            {
                return cache[fullPath];
            }

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"Module file not found: {path}");
            }

            string content = File.ReadAllText(fullPath);

            // Execute module in a new environment that inherits from global
            // Note: In some languages imports are isolated. Here we allow access to global builtins.
            Environment moduleEnv = new Environment(interpreter.global);
            Module module = new Module(moduleEnv);

            // Cache early to support circular dependencies
            cache[fullPath] = module;

            try
            {
                Lexer lexer = new Lexer(content);
                lexer.Tokenize();
                Parser parser = new Parser(lexer.Tokens);
                List<Stmt> statements = parser.Parse();

                interpreter.ExecuteBlock(statements, moduleEnv);
            }
            catch
            {
                // If execution fails, remove from cache to allow retry? 
                // Or leave it broken. Usually cleaner to remove.
                cache.Remove(fullPath);
                throw;
            }

            return module;
        }
    }
}

