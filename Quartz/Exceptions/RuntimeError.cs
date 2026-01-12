using System;
using Quartz.Parsing;

namespace Quartz.Exceptions
{
    internal class RuntimeError : Exception
    {
        public Token Token { get; }

        public RuntimeError(Token token, string message) : base(message)
        {
            Token = token;
        }
    }
}

