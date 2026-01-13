using Quartz.Parsing;
using Quartz.AST;
using Quartz.Runtime;
using Quartz.Exceptions;

namespace Quartz
{
    internal class Program
    {
        static Interpreter interpreter = new Interpreter();
        static bool hadError = false;

        static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: quartz [script]");
                System.Environment.Exit(64);
            }
            else if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunPrompt();
            }
        }

        private static void RunFile(string path)
        {
            try
            {
                string source = File.ReadAllText(path);
                Run(source);
                if (hadError) System.Environment.Exit(65);
            }
            catch (IOException e)
            {
                Console.WriteLine($"Could not read file: {e.Message}");
            }
            catch (ParseError error)
            {
                ErrorReporter.Error(error.Token, error.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
        }

        private static void RunPrompt()
        {
            Console.WriteLine("Quartz Language REPL (Type 'exit' to quit)");
            while (true)
            {
                Console.Write("> ");
                string line = Console.ReadLine();
                if (line == null || line == "exit") break;
                try
                {
                    Run(line);
                }
                catch (ParseError error)
                {
                    ErrorReporter.Error(error.Token, error.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
                hadError = false;
            }
        }

        private static void Run(string source)
        {
            Lexer lexer = new Lexer(source);
            lexer.Tokenize();

            Parser parser = new Parser(lexer.Tokens);
            List<Stmt> statements = parser.Parse();

            if (statements == null || statements.Count == 0) return;

            try
            {
                interpreter.Interpret(statements);
            }
            catch (RuntimeError error)
            {
                ErrorReporter.Error(error.Token, error.Message);
                hadError = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Unexpected: {ex.Message}");
                hadError = true;
            }
        }
    }
}


