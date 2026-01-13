using System;
using Quartz.Parsing;
using Quartz.Exceptions;

namespace Quartz.Runtime
{
    internal static class RuntimeErrorHandler
    {
        public static void Error(Token token, string message)
        {
            throw new RuntimeError(token, message);
        }
    }
}


