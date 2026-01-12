using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Quartz.AST;

namespace Quartz.Parsing
{
    internal class Parser
    {
        private readonly List<Token> tokens;
        private int current;

        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        public List<Stmt> Parse()
        {
            List<Stmt> statements = new();
            while (!IsAtEnd())
            {
                statements.Add(ParseStatement());
            }
            return statements;
        }

        private Stmt ParseStatement()
        {
            if (Check(TokenType.Auto) || Check(TokenType.Int) || Check(TokenType.Double) || Check(TokenType.Bool) || Check(TokenType.StringType) || Check(TokenType.Pointer))
                return VarDeclaration();

            if (Match(TokenType.LeftBrace))
                return new BlockStmt { Statements = Block() };

            if (Match(TokenType.If))
                return IfStatement();

            if (Match(TokenType.While))
                return WhileStatement();

            if (Match(TokenType.For))
                return ForStatement();

            if (Match(TokenType.Func))
                return Function("function");

            if (Match(TokenType.Class))
                return ClassDeclaration();

            if (Match(TokenType.Return))
                return ReturnStatement();

            return ExpressionStatement();
        }

        private Stmt Function(string kind)
        {
            Token name = Consume(TokenType.Identifier, $"Expected {kind} name.");
            Consume(TokenType.LeftParen, $"Expected '(' after {kind} name.");
            List<Token> parameters = new();
            if (!Check(TokenType.RightParen))
            {
                do
                {
                    if (parameters.Count >= 255)
                    {
                        // Error but don't throw
                        Console.WriteLine("Can't have more than 255 parameters.");
                    }

                    parameters.Add(Consume(TokenType.Identifier, "Expected parameter name."));
                } while (Match(TokenType.Comma));
            }
            Consume(TokenType.RightParen, "Expected ')' after parameters.");

            Consume(TokenType.LeftBrace, $"Expected '{{' before {kind} body.");
            List<Stmt> body = Block();
            return new FunctionStmt { Name = name, Params = parameters, Body = body };
        }

        private Stmt ReturnStatement()
        {
            Token keyword = Previous();
            Expr value = null;
            if (!Check(TokenType.Semicolon))
            {
                value = Expression();
            }

            Consume(TokenType.Semicolon, "Expected ';' after return value.");
            return new ReturnStmt { Keyword = keyword, Value = value };
        }

        private Stmt IfStatement()
        {
            Consume(TokenType.LeftParen, "Expected '(' after 'if'.");
            Expr condition = Expression();
            Consume(TokenType.RightParen, "Expected ')' after if condition.");

            Stmt thenBranch = ParseStatement();
            Stmt elseBranch = null;

            if (Match(TokenType.Else))
            {
                elseBranch = ParseStatement();
            }

            return new IfStmt { Condition = condition, ThenBranch = thenBranch, ElseBranch = elseBranch };
        }

        private Stmt WhileStatement()
        {
            Consume(TokenType.LeftParen, "Expected '(' after 'while'.");
            Expr condition = Expression();
            Consume(TokenType.RightParen, "Expected ')' after condition.");
            Stmt body = ParseStatement();

            return new WhileStmt { Condition = condition, Body = body };
        }

        private Stmt ForStatement()
        {
            Consume(TokenType.LeftParen, "Expected '(' after 'for'.");

            Stmt initializer;
            if (Match(TokenType.Semicolon))
            {
                initializer = null;
            }
            else if (Check(TokenType.Auto) || Check(TokenType.Int) || Check(TokenType.Double) || Check(TokenType.Bool) || Check(TokenType.StringType))
            {
                initializer = VarDeclaration();
            }
            else
            {
                initializer = ExpressionStatement();
            }

            Expr condition = null;
            if (!Check(TokenType.Semicolon))
            {
                condition = Expression();
            }
            Consume(TokenType.Semicolon, "Expected ';' after loop condition.");

            Expr increment = null;
            if (!Check(TokenType.RightParen))
            {
                increment = Expression();
            }
            Consume(TokenType.RightParen, "Expected ')' after for clauses.");

            Stmt body = ParseStatement();

            if (increment != null)
            {
                body = new BlockStmt
                {
                    Statements = new List<Stmt>
                    {
                        body,
                        new ExpressionStmt { Expression = increment }
                    }
                };
            }

            if (condition == null)
            {
                condition = new LiteralExpr { Value = true };
            }

            body = new WhileStmt { Condition = condition, Body = body };

            if (initializer != null)
            {
                body = new BlockStmt { Statements = new List<Stmt> { initializer, body } };
            }

            return body;
        }

        private List<Stmt> Block()
        {
            List<Stmt> statements = new();

            while (!Check(TokenType.RightBrace) && !IsAtEnd())
            {
                statements.Add(ParseStatement());
            }

            Consume(TokenType.RightBrace, "Expected '}' after block.");
            return statements;
        }

        private Stmt ExpressionStatement()
        {
            Expr expr = Expression();
            Consume(TokenType.Semicolon, "Expected ';' after expression.");
            return new ExpressionStmt { Expression = expr };
        }


        Stmt VarDeclaration()
        {
            // Try to consume type (auto, int, double, etc.)
            // Note: We don't enforce validation yet, just parsing.
            if (Match(TokenType.Auto, TokenType.Int, TokenType.Double, TokenType.Bool, TokenType.StringType, TokenType.Pointer))
            {
                // Type consumed
            }

            Token name = Consume(TokenType.Identifier, "Expected variable name");

            Expr initializer = null;
            if (Match(TokenType.Equal))
            {
                initializer = Expression();
            }

            Consume(TokenType.Semicolon, "Expected ';'");

            return new VarDeclStmt
            {
                Name = name.Value,
                Initializer = initializer
            };
        }

        private Stmt ClassDeclaration()
        {
            Token name = Consume(TokenType.Identifier, "Expected class name.");
            Consume(TokenType.LeftBrace, "Expected '{' before class body.");

            List<FunctionStmt> methods = new();
            while (!Check(TokenType.RightBrace) && !IsAtEnd())
            {
                // Methods within a class don't use the 'func' keyword in this design (like C# or Java)
                // BUT user example showed usage like C++/C style function?
                // Wait, user just said "func ReadInt(...)". So it probably looks like normal func decls usage.

                // Let's assume standard Quartz func declaration inside class:
                // class Memory { func ReadInt(...) { ... } }

                if (Match(TokenType.Func))
                {
                    // Cast Stmt to FunctionStmt
                    var funcStmt = (FunctionStmt)Function("method");
                    methods.Add(funcStmt);
                }
                else
                {
                    // Maybe allow fields later, or error for now needed
                    throw new Exception("Only methods are allowed in class body for now.");
                }
            }

            Consume(TokenType.RightBrace, "Expected '}' after class body.");

            return new ClassStmt { Name = name, Methods = methods };
        }

        Expr Expression()
        {
            return Assignment();
        }

        Expr Assignment()
        {
            Expr expr = Equality();

            if (Match(TokenType.Equal))
            {
                Token equals = Previous();
                Expr value = Assignment();

                if (expr is VariableExpr v)
                {
                    Token name = new Token { Type = TokenType.Identifier, Value = v.Name };
                    return new AssignExpr { Name = name.Value, Value = value };
                }
                else if (expr is GetExpr get)
                {
                    return new SetExpr { Object = get.Object, Name = get.Name, Value = value };
                }

                throw new Exception("Invalid assignment target.");
            }

            return expr;
        }

        // Comparisions
        Expr Equality()
        {
            Expr expr = Comparison();

            while (Match(TokenType.BangEqual, TokenType.EqualEqual))
            {
                Token op = Previous();
                Expr right = Comparison();
                expr = new BinaryExpr { Left = expr, Operator = op, Right = right };
            }

            return expr;
        }

        Expr Comparison()
        {
            Expr expr = Term();

            while (Match(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual))
            {
                Token op = Previous();
                Expr right = Term();
                expr = new BinaryExpr { Left = expr, Operator = op, Right = right };
            }

            return expr;
        }

        // + -
        Expr Term()
        {
            Expr expr = Factor();

            while (Match(TokenType.Plus, TokenType.Minus))
            {
                Token op = Previous();
                Expr right = Factor();

                expr = new BinaryExpr
                {
                    Left = expr,
                    Operator = op,
                    Right = right
                };
            }

            return expr;
        }

        // * /
        Expr Factor()
        {
            Expr expr = Unary();

            while (Match(TokenType.Star, TokenType.Slash))
            {
                Token op = Previous();
                Expr right = Unary();

                expr = new BinaryExpr
                {
                    Left = expr,
                    Operator = op,
                    Right = right
                };
            }

            return expr;
        }

        Expr Unary()
        {
            if (Match(TokenType.Bang, TokenType.Minus))
            {
                Token @operator = Previous();
                Expr right = Unary();
                return new UnaryExpr { Operator = @operator, Right = right };
            }

            return Call();
        }

        Expr Call()
        {
            Expr expr = Primary();

            while (true)
            {
                if (Match(TokenType.LeftParen))
                {
                    expr = FinishCall(expr);
                }
                else if (Match(TokenType.Dot))
                {
                    Token name = Consume(TokenType.Identifier, "Expected property name after '.'.");
                    expr = new GetExpr { Object = expr, Name = name.Value };
                }
                else if (Match(TokenType.LeftBracket))
                {
                    Token bracket = Previous();
                    Expr index = Expression();
                    Consume(TokenType.RightBracket, "Expected ']' after index.");
                    expr = new IndexExpr { Object = expr, Bracket = bracket, Index = index };
                }
                else
                {
                    break;
                }
            }

            return expr;
        }

        Expr FinishCall(Expr callee)
        {
            List<Expr> arguments = new();
            if (!Check(TokenType.RightParen))
            {
                do
                {
                    if (arguments.Count >= 255)
                    {
                        // Limit args
                    }
                    arguments.Add(Expression());
                } while (Match(TokenType.Comma));
            }

            Token paren = Consume(TokenType.RightParen, "Expected ')' after arguments.");

            return new CallExpr { Callee = callee, Paren = paren, Arguments = arguments };
        }

        // literals, identifiers, ()
        Expr Primary()
        {
            if (Match(TokenType.Number))
            {
                string value = Previous().Value;
                if (value.StartsWith("0x") || value.StartsWith("0X"))
                {
                    return new LiteralExpr { Value = Convert.ToInt32(value.Substring(2), 16) };
                }
                if (value.Contains('.'))
                    return new LiteralExpr { Value = double.Parse(value, System.Globalization.CultureInfo.InvariantCulture) };
                return new LiteralExpr { Value = int.Parse(value) };
            }

            if (Match(TokenType.String))
                return new LiteralExpr { Value = Previous().Value };

            if (Match(TokenType.Boolean))
                return new LiteralExpr { Value = Previous().Value == "True" };

            if (Match(TokenType.Identifier))
                return new VariableExpr { Name = Previous().Value };

            if (Match(TokenType.This))
                return new ThisExpr { Keyword = Previous() };

            if (Match(TokenType.LeftBracket))
                return Array();

            if (Match(TokenType.LeftParen))
            {
                Expr expr = Expression();
                Consume(TokenType.RightParen, "Expected ')'");
                return expr;
            }

            throw new Exception($"Expected expression. Got: {Peek().Type} ('{Peek().Value}') at line {Peek().Line}");
        }

        Token Peek() => tokens[current];
        Token Previous() => tokens[current - 1];
        bool IsAtEnd() => Peek().Type == TokenType.EndOfFile;

        Token Advance()
        {
            if (!IsAtEnd()) current++;
            return Previous();
        }

        bool Check(TokenType type)
            => !IsAtEnd() && Peek().Type == type;

        bool Match(params TokenType[] types)
        {
            foreach (var type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }

        Token Consume(TokenType type, string error)
        {
            if (Check(type)) return Advance();
            throw new Exception(error);
        }
        Expr Array()
        {
            List<Expr> elements = new();
            if (!Check(TokenType.RightBracket))
            {
                do
                {
                    elements.Add(Expression());
                } while (Match(TokenType.Comma));
            }

            Consume(TokenType.RightBracket, "Expected ']' after array elements.");
            return new ArrayExpr { Values = elements };
        }

    }
}

