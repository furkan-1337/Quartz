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
                var cachedModule = cache[fullPath];
                foreach (var kvp in cachedModule.ExportedEnv.Values)
                {
                    interpreter.CurrentEnvironment.Define(kvp.Key, kvp.Value);
                }
                return cachedModule;
            }

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"Module file not found: {path}");
            }

            string content = File.ReadAllText(fullPath);

            
            
            Environment moduleEnv = new Environment(interpreter.global);
            Module module = new Module(moduleEnv);

            
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
                
                cache.Remove(fullPath);
                throw;
            }

            
            foreach (var kvp in module.ExportedEnv.Values)
            {
                interpreter.CurrentEnvironment.Define(kvp.Key, kvp.Value);
            }

            return module;
        }
    }
}


