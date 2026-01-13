using System;
using Quartz.Parsing;

namespace Quartz.Exceptions
{
    internal class ParseError : Exception
    {
        public Token Token { get; }

        public ParseError(Token token, string message) : base(message)
        {
            Token = token;
        }
    }
}

