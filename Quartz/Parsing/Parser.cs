using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Quartz.AST;
using Quartz.Exceptions;

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
            if (Check(TokenType.Auto) || Check(TokenType.Int) || Check(TokenType.Long) || Check(TokenType.Double) || Check(TokenType.Float) || Check(TokenType.Bool) || Check(TokenType.StringType) || Check(TokenType.Pointer))
                return VarDeclaration();

            if (Match(TokenType.LeftBrace))
                return new BlockStmt { Statements = Block() };

            if (Match(TokenType.If))
                return IfStatement();

            if (Match(TokenType.While))
                return WhileStatement();

            if (Match(TokenType.For))
                return ForStatement();

            if (Match(TokenType.Foreach))
                return ForeachStatement();

            if (Match(TokenType.Func))
                return Function("function");

            if (Match(TokenType.Class))
                return ClassDeclaration();

            if (Match(TokenType.Struct))
                return StructDeclaration();

            if (Match(TokenType.Enum))
                return EnumDeclaration();

            if (Match(TokenType.Return))
                return ReturnStatement();

            if (Match(TokenType.Try))
                return TryStatement();

            if (Match(TokenType.Switch))
                return SwitchStatement();

            if (Match(TokenType.Break))
                return BreakStatement();

            if (Match(TokenType.Break))
                return BreakStatement();

            if (Match(TokenType.Continue))
                return ContinueStatement();

            return ExpressionStatement();
        }

        private Stmt ContinueStatement()
        {
            Token keyword = Previous();
            Consume(TokenType.Semicolon, "Expected ';' after 'continue'.");
            return new ContinueStmt { Keyword = keyword };
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
            else if (Check(TokenType.Auto) || Check(TokenType.Int) || Check(TokenType.Long) || Check(TokenType.Double) || Check(TokenType.Float) || Check(TokenType.Bool) || Check(TokenType.StringType))
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

            return new ForStmt
            {
                Initializer = initializer,
                Condition = condition,
                Increment = increment,
                Body = body
            };
        }

        private Stmt ForeachStatement()
        {
            Consume(TokenType.LeftParen, "Expected '(' after 'foreach'.");



            if (Match(TokenType.Auto)) { }

            Token variable = Consume(TokenType.Identifier, "Expected variable name after 'foreach ('.");
            Consume(TokenType.In, "Expected 'in' after variable name.");

            Expr collection = Expression();
            Consume(TokenType.RightParen, "Expected ')' after foreach clauses.");

            Stmt body = ParseStatement();

            return new ForeachStmt { VariableName = variable, Collection = collection, Body = body };
        }

        private Stmt TryStatement()
        {
            Consume(TokenType.LeftBrace, "Expected '{' before try block.");
            List<Stmt> tryBody = Block();
            Stmt tryBlock = new BlockStmt { Statements = tryBody };

            Consume(TokenType.Catch, "Expected 'catch' after try block.");
            Consume(TokenType.LeftParen, "Expected '(' after 'catch'.");
            Token errorVariable = Consume(TokenType.Identifier, "Expected error variable name.");
            Consume(TokenType.RightParen, "Expected ')' after error variable.");

            Consume(TokenType.LeftBrace, "Expected '{' before catch block.");
            List<Stmt> catchBody = Block();
            Stmt catchBlock = new BlockStmt { Statements = catchBody };

            return new TryStmt { TryBlock = tryBlock, CatchBlock = catchBlock, ErrorVariable = errorVariable };
        }

        private Stmt SwitchStatement()
        {
            Consume(TokenType.LeftParen, "Expected '(' after 'switch'.");
            Expr expression = Expression();
            Consume(TokenType.RightParen, "Expected ')' after switch expression.");
            Consume(TokenType.LeftBrace, "Expected '{' before switch body.");

            List<SwitchCase> cases = new();
            List<Stmt> defaultBranch = null;

            while (!Check(TokenType.RightBrace) && !IsAtEnd())
            {
                if (Match(TokenType.Case))
                {
                    Expr value = Expression();
                    Consume(TokenType.Colon, "Expected ':' after case value.");

                    List<Stmt> statements = new();

                    while (!Check(TokenType.Case) && !Check(TokenType.Default) && !Check(TokenType.RightBrace) && !IsAtEnd())
                    {
                        statements.Add(ParseStatement());
                    }
                    cases.Add(new SwitchCase { Value = value, Body = statements });
                }
                else if (Match(TokenType.Default))
                {
                    if (defaultBranch != null)
                        throw Error(Previous(), "A switch statement can only have one default case.");

                    Consume(TokenType.Colon, "Expected ':' after 'default'.");
                    List<Stmt> statements = new();
                    while (!Check(TokenType.Case) && !Check(TokenType.Default) && !Check(TokenType.RightBrace) && !IsAtEnd())
                    {
                        statements.Add(ParseStatement());
                    }
                    defaultBranch = statements;
                }
                else
                {
                    throw Error(Peek(), "Expected 'case' or 'default' inside switch.");
                }
            }

            Consume(TokenType.RightBrace, "Expected '}' after switch body.");
            return new SwitchStmt { Expression = expression, Cases = cases, DefaultBranch = defaultBranch };
        }

        private Stmt BreakStatement()
        {
            Token keyword = Previous();
            Consume(TokenType.Semicolon, "Expected ';' after 'break'.");
            return new BreakStmt { Keyword = keyword };
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


            if (Match(TokenType.Auto, TokenType.Int, TokenType.Long, TokenType.Double, TokenType.Float, TokenType.Bool, TokenType.StringType, TokenType.Pointer))
            {

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

            Expr superclass = null;
            if (Match(TokenType.Colon))
            {
                Consume(TokenType.Identifier, "Expected superclass name.");
                superclass = new VariableExpr { Name = Previous().Value };
            }

            Consume(TokenType.LeftBrace, "Expected '{' before class body.");

            List<FunctionStmt> methods = new();
            while (!Check(TokenType.RightBrace) && !IsAtEnd())
            {

                if (Match(TokenType.Func))
                {
                    var funcStmt = (FunctionStmt)Function("method");
                    methods.Add(funcStmt);
                }
                else
                {
                    throw new Exception("Only methods are allowed in class body for now.");
                }
            }

            Consume(TokenType.RightBrace, "Expected '}' after class body.");

            return new ClassStmt { Name = name, Superclass = superclass, Methods = methods };
        }

        private Stmt StructDeclaration()
        {
            Token name = Consume(TokenType.Identifier, "Expected struct name.");
            Consume(TokenType.LeftBrace, "Expected '{' before struct body.");

            List<StructField> fields = new();
            List<FunctionStmt> methods = new();

            while (!Check(TokenType.RightBrace) && !IsAtEnd())
            {
                if (Match(TokenType.Func))
                {
                    var method = (FunctionStmt)Function("method");
                    methods.Add(method);
                }
                else
                {
                    if (!Match(TokenType.Int, TokenType.Long, TokenType.Double, TokenType.Float, TokenType.Bool, TokenType.StringType, TokenType.Pointer, TokenType.Auto))
                        throw new Exception($"Expected type in struct field at line {Peek().Line}");

                    Token type = Previous();
                    Token fieldName = Consume(TokenType.Identifier, "Expected field name.");
                    Consume(TokenType.Semicolon, "Expected ';' after field declaration.");
                    fields.Add(new StructField { Type = type, Name = fieldName });
                }
            }

            Consume(TokenType.RightBrace, "Expected '}' after struct body.");

            return new StructStmt { Name = name, Fields = fields, Methods = methods };
        }

        private Stmt EnumDeclaration()
        {
            Token name = Consume(TokenType.Identifier, "Expected enum name.");
            Consume(TokenType.LeftBrace, "Expected '{' before enum body.");

            List<Token> members = new List<Token>();
            if (!Check(TokenType.RightBrace))
            {
                do
                {
                    members.Add(Consume(TokenType.Identifier, "Expected enum member name."));
                } while (Match(TokenType.Comma));
            }

            Consume(TokenType.RightBrace, "Expected '}' after enum body.");

            return new EnumStmt { Name = name, Members = members };
        }

        Expr Expression()
        {
            return Assignment();
        }

        Expr Assignment()
        {
            Expr expr = LogicOr();

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
                else if (expr is IndexExpr index)
                {

























                    return new SetIndexExpr { Object = index.Object, Bracket = index.Bracket, Index = index.Index, Value = value };
                }

                throw Error(equals, "Invalid assignment target.");
            }

            return expr;
        }

        Expr LogicOr()
        {
            Expr expr = LogicAnd();

            while (Match(TokenType.Or))
            {
                Token op = Previous();
                Expr right = LogicAnd();
                expr = new LogicalExpr { Left = expr, Operator = op, Right = right };
            }

            return expr;
        }

        Expr LogicAnd()
        {
            Expr expr = BitwiseOr();

            while (Match(TokenType.And))
            {
                Token op = Previous();
                Expr right = BitwiseOr();
                expr = new LogicalExpr { Left = expr, Operator = op, Right = right };
            }

            return expr;
        }

        Expr BitwiseOr()
        {
            Expr expr = BitwiseXor();
            while (Match(TokenType.BitwiseOr))
            {
                Token op = Previous();
                Expr right = BitwiseXor();
                expr = new BinaryExpr { Left = expr, Operator = op, Right = right };
            }
            return expr;
        }

        Expr BitwiseXor()
        {
            Expr expr = BitwiseAnd();
            while (Match(TokenType.BitwiseXor))
            {
                Token op = Previous();
                Expr right = BitwiseAnd();
                expr = new BinaryExpr { Left = expr, Operator = op, Right = right };
            }
            return expr;
        }

        Expr BitwiseAnd()
        {
            Expr expr = Equality();
            while (Match(TokenType.BitwiseAnd))
            {
                Token op = Previous();
                Expr right = Equality();
                expr = new BinaryExpr { Left = expr, Operator = op, Right = right };
            }
            return expr;
        }


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
            Expr expr = Shift();

            while (Match(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual))
            {
                Token op = Previous();
                Expr right = Shift();
                expr = new BinaryExpr { Left = expr, Operator = op, Right = right };
            }

            return expr;
        }

        Expr Shift()
        {
            Expr expr = Term();
            while (Match(TokenType.ShiftLeft, TokenType.ShiftRight))
            {
                Token op = Previous();
                Expr right = Term();
                expr = new BinaryExpr { Left = expr, Operator = op, Right = right };
            }
            return expr;
        }


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
            if (Match(TokenType.Bang, TokenType.Minus, TokenType.BitwiseNot))
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

                    }
                    arguments.Add(Expression());
                } while (Match(TokenType.Comma));
            }

            Token paren = Consume(TokenType.RightParen, "Expected ')' after arguments.");

            return new CallExpr { Callee = callee, Paren = paren, Arguments = arguments };
        }


        Expr Primary()
        {
            if (Match(TokenType.Number))
            {
                string value = Previous().Value;
                if (value.StartsWith("0x") || value.StartsWith("0X"))
                {
                    return new LiteralExpr { Value = Convert.ToInt64(value.Substring(2), 16) };
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

            if (Match(TokenType.Base))
            {
                Token keyword = Previous();
                Consume(TokenType.Dot, "Expected '.' after 'base'.");
                Token method = Consume(TokenType.Identifier, "Expected superclass method name.");
                return new BaseExpr { Keyword = keyword, Method = method };
            }

            if (Match(TokenType.LeftBracket))
                return Array();

            if (Match(TokenType.LeftParen))
            {





                int savedCurrent = current;
                bool isLambda = false;


                if (Check(TokenType.RightParen))
                {

                    if (current + 1 < tokens.Count && tokens[current + 1].Type == TokenType.Arrow)
                        isLambda = true;
                }
                else
                {


                    int temp = current;
                    while (temp < tokens.Count && tokens[temp].Type == TokenType.Identifier)
                    {
                        temp++;
                        if (temp < tokens.Count && tokens[temp].Type == TokenType.Comma)
                        {
                            temp++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (temp < tokens.Count && tokens[temp].Type == TokenType.RightParen)
                    {
                        if (temp + 1 < tokens.Count && tokens[temp + 1].Type == TokenType.Arrow)
                            isLambda = true;
                    }
                }

                if (isLambda)
                {
                    List<Token> parameters = new List<Token>();
                    if (!Check(TokenType.RightParen))
                    {
                        do
                        {
                            parameters.Add(Consume(TokenType.Identifier, "Expected parameter name."));
                        } while (Match(TokenType.Comma));
                    }
                    Consume(TokenType.RightParen, "Expected ')' after parameters.");
                    Consume(TokenType.Arrow, "Expected '=>' after lambda parameters.");

                    List<Stmt> body = new List<Stmt>();
                    if (Match(TokenType.LeftBrace))
                    {
                        body = Block();
                    }
                    else
                    {
                        Expr lambdaExprBody = Expression();
                        body.Add(new ReturnStmt { Keyword = new Token { Type = TokenType.Return, Line = Peek().Line }, Value = lambdaExprBody });
                    }
                    return new LambdaExpr { Params = parameters, Body = body };
                }

                Expr expr = Expression();
                Consume(TokenType.RightParen, "Expected ')'");
                return expr;
            }

            if (Match(TokenType.LeftBrace))
                return Dictionary();

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

        Token Consume(TokenType type, string message)
        {
            if (Check(type)) return Advance();
            throw Error(Peek(), message);
        }

        private ParseError Error(Token token, string message)
        {
            return new ParseError(token, message);
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

        Expr Dictionary()
        {
            List<Expr> keys = new();
            List<Expr> values = new();

            if (!Check(TokenType.RightBrace))
            {
                do
                {
                    keys.Add(Expression());
                    Consume(TokenType.Colon, "Expected ':' after dictionary key.");
                    values.Add(Expression());
                } while (Match(TokenType.Comma));
            }

            Consume(TokenType.RightBrace, "Expected '}' after dictionary.");
            return new DictionaryExpr { Keys = keys, Values = values };
        }



    }
}


