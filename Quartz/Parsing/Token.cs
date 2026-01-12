using System;
using System.Collections.Generic;

namespace Quartz.Parsing
{
    enum TokenType
    {
        LeftParen, RightParen,
        LeftBrace, RightBrace,
        LeftBracket, RightBracket,
        Comma, Dot, Minus, Plus, Colon,
        Semicolon, Slash, Star,
        Equal, EqualEqual, Bang, BangEqual,
        Greater, GreaterEqual, Less, LessEqual,
        Auto,
        Int, Double, Bool, StringType, Pointer,
        Boolean,
        Identifier, String, Number,
        If, Else, While, For,
        Func, Return,
        Class, This,
        EndOfFile
    }

    internal class Token
    {
        public TokenType Type { get; set; }
        public string Value { get; set; } = string.Empty;
        public int Line { get; init; }
        public int Column { get; init; }
        public override string ToString() => string.IsNullOrEmpty(Value) ? $" {Type} " : $" {Type} : {Value} ";
    }
}
