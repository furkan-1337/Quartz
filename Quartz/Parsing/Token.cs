using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quartz.Parsing
{
    enum TokenType
    {
        // Tek karakterli operatörler ve ayýrýcýlar
        LeftParen, RightParen,   // ( )
        LeftBrace, RightBrace,   // { }
        LeftBracket, RightBracket, // [ ]
        Comma, Dot, Minus, Plus, // , . - +
        Semicolon, Slash, Star,  // ; / *
        Equal, EqualEqual, Bang, BangEqual, // = == ! !=
        Greater, GreaterEqual, Less, LessEqual, // > >= < <=

        // Literals (Sabitler)
        Auto,
        Int, Double, Bool, StringType, Pointer, // Type Keywords
        Boolean,
        Identifier, String, Number,

        // Akýþ Kontrolü
        If, Else, While, For,
        Func, Return,
        Class, This,

        EndOfFile
    }

    internal class Token
    {
        public TokenType Type { get; set; }
        public string Value { get; set; }
        public int Line { get; init; }
        public int Column { get; init; }

        public override string ToString() => string.IsNullOrEmpty(Value) ? $"{Type}" : $"{Type} : {Value}";
    }
}

