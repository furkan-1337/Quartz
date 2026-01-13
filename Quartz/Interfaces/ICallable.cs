using System.Collections.Generic;
using Quartz.Runtime;

namespace Quartz.Interfaces
{
    internal interface ICallable
    {
        int Arity();
        object Call(Interpreter interpreter, List<object> arguments);
    }
}


