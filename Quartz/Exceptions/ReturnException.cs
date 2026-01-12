using System;

namespace Quartz.Exceptions
{
    public class ReturnException : Exception
    {
        public object Value { get; }

        public ReturnException(object value)
        {
            Value = value;
        }
    }
}

