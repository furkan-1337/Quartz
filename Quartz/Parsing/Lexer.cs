using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quartz.Parsing
{
    internal class Lexer
    {
        public readonly string Source;
        public readonly List<Token> Tokens = new();

        public int Start = 0;
        public int Current = 0;
        public int Line = 1;
        public int Column = 1;

        public Lexer(string source)
        {
            Source = source;
        }

        public void Tokenize()
        {
            while (!IsAtEnd())
            {
                Start = Current;
                ScanToken();
            }

            Tokens.Add(new Token { Type = TokenType.EndOfFile, Line = Line, Column = Column });
        }

        private void ScanToken()
        {
            char c = Advance();

            switch (c)
            {
                case ' ':
                case '\t':
                case '\r':
                    break;
                case '\n':
                    Line++;
                    Column = 1;
                    break;

                case '=':
                    if (Match('='))
                        Add(TokenType.EqualEqual, "==");
                    else if (Match('>'))
                        Add(TokenType.Arrow, "=>");
                    else
                        Add(TokenType.Equal, "=");
                    break;

                
                
                
                

                
                
                

                
                
                

                

                case '!':
                    Add(Match('=') ? TokenType.BangEqual : TokenType.Bang, Match('=') ? "!=" : "!");
                    break;
                case '&':
                    if (Match('&')) Add(TokenType.And, "&&");
                    else throw new Exception($"Unexpected character: {c} at line {Line}");
                    break;
                case '|':
                    if (Match('|')) Add(TokenType.Or, "||");
                    else throw new Exception($"Unexpected character: {c} at line {Line}");
                    break;
                case '<':
                    Add(Match('=') ? TokenType.LessEqual : TokenType.Less, Match('=') ? "<=" : "<");
                    break;
                case '>':
                    Add(Match('=') ? TokenType.GreaterEqual : TokenType.Greater, Match('=') ? ">=" : ">");
                    break;

                case '+':
                    Add(TokenType.Plus, "+");
                    break;

                case '-':
                    Add(TokenType.Minus, "-");
                    break;

                case '*':
                    Add(TokenType.Star, "*");
                    break;

                case '/':
                    if (Match('/'))
                    {
                        while (Peek() != '\n' && !IsAtEnd()) Advance();
                    }
                    else
                    {
                        Add(TokenType.Slash, "/");
                    }
                    break;

                case ';':
                    Add(TokenType.Semicolon, ";");
                    break;

                case ',':
                    Add(TokenType.Comma, ",");
                    break;

                case '.':
                    Add(TokenType.Dot, ".");
                    break;

                case '(':
                    Add(TokenType.LeftParen, "(");
                    break;
                case ')':
                    Add(TokenType.RightParen, ")");
                    break;
                case '{':
                    Add(TokenType.LeftBrace, "{");
                    break;
                case '}':
                    Add(TokenType.RightBrace, "}");
                    break;
                case '[':
                    Add(TokenType.LeftBracket, "[");
                    break;
                case ']':
                    Add(TokenType.RightBracket, "]");
                    break;
                case ':':
                    Add(TokenType.Colon, ":");
                    break;

                case '"':
                    String();
                    break;

                case '$':
                    if (Peek() == '"') InterpolatedString();
                    else throw new Exception($"Unexpected character: {c} at line {Line}");
                    break;

                default:
                    if (char.IsDigit(c))
                    {
                        Number();
                    }
                    else if (char.IsLetter(c) || c == '_')
                    {
                        Identifier();
                    }
                    else
                    {
                        throw new Exception($"Unexpected character: {c} at line {Line}");
                    }
                    break;
            }
        }


        private bool Match(char expected)
        {
            if (IsAtEnd()) return false;
            if (Source[Current] != expected) return false;

            Current++;
            Column++;
            return true;
        }

        public char Advance()
        {
            Column++;
            return Source[Current++];
        }

        public char Peek() => Current >= Source.Length ? '\0' : Source[Current];
        public bool IsAtEnd() => Current >= Source.Length;

        public void Add(TokenType type, string value)
            => Tokens.Add(new Token { Type = type, Value = value, Line = Line, Column = Column - value.Length });

        
        public void Identifier()
        {
            int start = Current - 1;

            while (!IsAtEnd() && (char.IsLetterOrDigit(Peek()) || Peek() == '_'))
                Advance();

            string text = Source[start..Current];

            switch (text)
            {
                case "auto":
                    Add(TokenType.Auto, text);
                    break;
                case "int":
                    Add(TokenType.Int, text);
                    break;
                case "double":
                    Add(TokenType.Double, text);
                    break;
                case "float":
                    Add(TokenType.Float, text);
                    break;
                case "bool":
                    Add(TokenType.Bool, text);
                    break;
                case "string":
                    Add(TokenType.StringType, text);
                    break;
                case "pointer":
                    Add(TokenType.Pointer, text);
                    break;
                case "if":
                    Add(TokenType.If, text);
                    break;
                case "else":
                    Add(TokenType.Else, text);
                    break;
                case "true":
                    Add(TokenType.Boolean, "True");
                    break;
                case "false":
                    Add(TokenType.Boolean, "False");
                    break;
                case "while":
                    Add(TokenType.While, text);
                    break;
                case "for":
                    Add(TokenType.For, text);
                    break;
                case "func":
                    Add(TokenType.Func, text);
                    break;
                case "return":
                    Add(TokenType.Return, text);
                    break;
                case "class":
                    Add(TokenType.Class, text);
                    break;
                case "struct":
                    Add(TokenType.Struct, text);
                    break;
                case "this":
                    Add(TokenType.This, text);
                    break;
                case "foreach":
                    Add(TokenType.Foreach, text);
                    break;
                case "in":
                    Add(TokenType.In, text);
                    break;
                case "try":
                    Add(TokenType.Try, text);
                    break;
                case "catch":
                    Add(TokenType.Catch, text);
                    break;
                case "switch":
                    Add(TokenType.Switch, text);
                    break;
                case "case":
                    Add(TokenType.Case, text);
                    break;
                case "default":
                    Add(TokenType.Default, text);
                    break;
                case "break":
                    Add(TokenType.Break, text);
                    break;
                case "base":
                    Add(TokenType.Base, text);
                    break;
                case "enum":
                    Add(TokenType.Enum, text);
                    break;

                case "True":
                case "False":
                    Add(TokenType.Boolean, text);
                    break;

                default:
                    Add(TokenType.Identifier, text);
                    break;
            }
        }

        public void Number()
        {
            int start = Current - 1;

            if (Source[start] == '0' && (Peek() == 'x' || Peek() == 'X'))
            {
                Advance(); 
                while (!IsAtEnd() && IsHexDigit(Peek()))
                    Advance();
            }
            else
            {
                while (!IsAtEnd() && char.IsDigit(Peek()))
                    Advance();

                
                if (Peek() == '.' && char.IsDigit(PeekNext()))
                {
                    
                    Advance();

                    while (char.IsDigit(Peek())) Advance();
                }
            }

            string value = Source[start..Current];
            Add(TokenType.Number, value);
        }

        private bool IsHexDigit(char c)
        {
            return char.IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
        }

        private char PeekNext()
        {
            if (Current + 1 >= Source.Length) return '\0';
            return Source[Current + 1];
        }

        public void String()
        {
            int start = Current;

            while (!IsAtEnd() && Peek() != '"')
                Advance();

            Advance(); // closing "

            string value = Source[start..(Current - 1)];
            Add(TokenType.String, value);
        }

        public void InterpolatedString()
        {
            Advance(); 

            Add(TokenType.LeftParen, "(");
            Add(TokenType.String, ""); 

            while (!IsAtEnd())
            {
                if (Peek() == '"')
                {
                    Advance(); // consume "
                    break;
                }
                else if (Peek() == '{')
                {
                    Add(TokenType.Plus, "+");
                    Advance(); 

                    Add(TokenType.LeftParen, "(");
                    TokenizeInterpolatedExpression();
                    Add(TokenType.RightParen, ")");
                }
                else
                {
                    int startPart = Current;
                    while (Peek() != '"' && Peek() != '{' && !IsAtEnd())
                    {
                        if (Peek() == '\n') Line++;
                        Advance();
                    }
                    string part = Source.Substring(startPart, Current - startPart);

                    if (part.Length > 0)
                    {
                        Add(TokenType.Plus, "+");
                        Add(TokenType.String, part);
                    }
                }
            }

            Add(TokenType.RightParen, ")");
        }

        private void TokenizeInterpolatedExpression()
        {
            int braceCount = 1;
            while (braceCount > 0 && !IsAtEnd())
            {
                char c = Peek();
                if (c == '}')
                {
                    braceCount--;
                    if (braceCount == 0)
                    {
                        Advance(); 
                        return;
                    }
                }
                else if (c == '{')
                {
                    braceCount++;
                }

                if (char.IsWhiteSpace(c))
                {
                    if (c == '\n') Line++;
                    Advance();
                    continue;
                }

                Start = Current;
                ScanToken();
            }
        }
    }
}


