using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Quartz.Interfaces;
using Quartz.Runtime.Types;

namespace Quartz.Runtime.Modules
{
    internal class ProcessModule : Module
    {
        public ProcessModule() : base(new Environment())
        {
            ExportedEnv.Define("list", new NativeList());
            ExportedEnv.Define("isRunning", new NativeIsRunning());
            ExportedEnv.Define("getModuleAddress", new NativeGetModuleAddress());
            ExportedEnv.Define("getProcessIdByName", new NativeGetProcessIdByName());
            ExportedEnv.Define("getModules", new NativeGetModules());
            ExportedEnv.Define("terminate", new NativeTerminate());
            ExportedEnv.Define("getCurrentProcess", new NativeGetCurrentProcess());
            ExportedEnv.Define("getExecutablePath", new NativeGetExecutablePath());
            ExportedEnv.Define("getWorkingPath", new NativeGetWorkingPath());
        }

        private class NativeList : ICallable
        {
            public int Arity() => 0;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                return Process.GetProcesses()
                    .Select(p => (object)p.ProcessName)
                    .ToList();
            }
            public override string ToString() => "<native fn Process.list>";
        }

        private class NativeGetModuleAddress : ICallable
        {
            public int Arity() => 2;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                int pid = Convert.ToInt32(arguments[0]);
                string moduleName = (string)arguments[1];

                try
                {
                    using (Process proc = Process.GetProcessById(pid))
                    {
                        foreach (System.Diagnostics.ProcessModule module in proc.Modules)
                        {
                            if (module.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase))
                            {
                                return new QPointer((long)module.BaseAddress);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to get module address: {ex.Message}");
                }

                return new QPointer(0);
            }
            public override string ToString() => "<native fn Process.getModuleAddress>";
        }

        private class NativeIsRunning : ICallable
        {
            public int Arity() => 1;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                int pid = Convert.ToInt32(arguments[0]);
                try
                {
                    using (Process proc = Process.GetProcessById(pid))
                    {
                        return !proc.HasExited;
                    }
                }
                catch
                {
                    return false;
                }
            }
            public override string ToString() => "<native fn Process.isRunning>";
        }

        private class NativeGetProcessIdByName : ICallable
        {
            public int Arity() => 1;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                string name = (string)arguments[0];
                var procs = Process.GetProcessesByName(name);
                if (procs.Length > 0)
                {
                    int pid = procs[0].Id;
                    foreach (var p in procs) p.Dispose();
                    return pid;
                }
                return -1;
            }
            public override string ToString() => "<native fn Process.getProcessIdByName>";
        }

        private class NativeGetModules : ICallable
        {
            public int Arity() => 1;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                int pid = Convert.ToInt32(arguments[0]);
                try
                {
                    using (Process proc = Process.GetProcessById(pid))
                    {
                        var modules = new List<object>();
                        foreach (System.Diagnostics.ProcessModule module in proc.Modules)
                        {
                            modules.Add(module.ModuleName);
                        }
                        return modules;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to get modules: {ex.Message}");
                }
            }
            public override string ToString() => "<native fn Process.getModules>";
        }

        private class NativeTerminate : ICallable
        {
            public int Arity() => 1;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                int pid = Convert.ToInt32(arguments[0]);
                try
                {
                    using (Process proc = Process.GetProcessById(pid))
                    {
                        proc.Kill();
                        return true;
                    }
                }
                catch { return false; }
            }
            public override string ToString() => "<native fn Process.terminate>";
        }

        private class NativeGetCurrentProcess : ICallable
        {
            public int Arity() => 0;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                return Process.GetCurrentProcess().Id;
            }
            public override string ToString() => "<native fn Process.getCurrentProcess>";
        }

        private class NativeGetExecutablePath : ICallable
        {
            public int Arity() => 1;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                int pid = Convert.ToInt32(arguments[0]);
                try
                {
                    using (Process proc = Process.GetProcessById(pid))
                    {
                        return proc.MainModule?.FileName ?? "";
                    }
                }
                catch { return ""; }
            }
            public override string ToString() => "<native fn Process.getExecutablePath>";
        }

        private class NativeGetWorkingPath : ICallable
        {
            public int Arity() => 1;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                int pid = Convert.ToInt32(arguments[0]);
                try
                {
                    if (pid == System.Diagnostics.Process.GetCurrentProcess().Id)
                    {
                        return System.Environment.CurrentDirectory;
                    }

                    using (System.Diagnostics.Process proc = System.Diagnostics.Process.GetProcessById(pid))
                    {
                        string? exePath = proc.MainModule?.FileName;
                        if (!string.IsNullOrEmpty(exePath))
                        {
                            return System.IO.Path.GetDirectoryName(exePath) ?? "";
                        }
                    }
                }
                catch { }
                return "";
            }
            public override string ToString() => "<native fn Process.getWorkingPath>";
        }
    }
}
