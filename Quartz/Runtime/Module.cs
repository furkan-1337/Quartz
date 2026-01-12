using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quartz.Runtime
{
    public class Module
    {
        public Environment ExportedEnv { get; private set; }

        public Module(Environment env)
        {
            ExportedEnv = env;
        }

        public object? Get(string name)
        {
            return ExportedEnv.Get(name);
        }
    }
}

