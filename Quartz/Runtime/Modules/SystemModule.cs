using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Quartz.Interfaces;

namespace Quartz.Runtime.Modules
{
    internal class SystemModule : Module
    {
        public SystemModule() : base(new Environment())
        {
            ExportedEnv.Define("getOSVersion", new NativeGetOSVersion());
            ExportedEnv.Define("getMemoryStats", new NativeGetMemoryStats());
            ExportedEnv.Define("getCPUUsage", new NativeGetCPUUsage());
            ExportedEnv.Define("getMachineName", new NativeGetMachineName());
            ExportedEnv.Define("getUserName", new NativeGetUserName());
        }

        private class NativeGetOSVersion : ICallable
        {
            public int Arity() => 0;
            public object Call(Interpreter interpreter, List<object> arguments) => System.Environment.OSVersion.ToString();
            public override string ToString() => "<native fn System.getOSVersion>";
        }

        private class NativeGetMachineName : ICallable
        {
            public int Arity() => 0;
            public object Call(Interpreter interpreter, List<object> arguments) => System.Environment.MachineName;
            public override string ToString() => "<native fn System.getMachineName>";
        }

        private class NativeGetUserName : ICallable
        {
            public int Arity() => 0;
            public object Call(Interpreter interpreter, List<object> arguments) => System.Environment.UserName;
            public override string ToString() => "<native fn System.getUserName>";
        }

        private class NativeGetMemoryStats : ICallable
        {
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            private class MEMORYSTATUSEX
            {
                public uint dwLength;
                public uint dwMemoryLoad;
                public ulong ullTotalPhys;
                public ulong ullAvailPhys;
                public ulong ullTotalPageFile;
                public ulong ullAvailPageFile;
                public ulong ullTotalVirtual;
                public ulong ullAvailVirtual;
                public ulong ullAvailExtendedVirtual;
                public MEMORYSTATUSEX() { dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX)); }
            }

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

            public int Arity() => 0;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                var dict = new Dictionary<string, object>();
                var memStatus = new MEMORYSTATUSEX();
                if (GlobalMemoryStatusEx(memStatus))
                {
                    dict["totalPhys"] = (long)memStatus.ullTotalPhys;
                    dict["availPhys"] = (long)memStatus.ullAvailPhys;
                    dict["memoryLoad"] = (int)memStatus.dwMemoryLoad;
                }
                return dict;
            }
            public override string ToString() => "<native fn System.getMemoryStats>";
        }

        private class NativeGetCPUUsage : ICallable
        {
            private static DateTime lastTime;
            private static TimeSpan lastTotalProcessorTime;
            private static double cpuUsage;

            public int Arity() => 0;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                // Simple process-specific CPU usage calculation
                if (lastTime == DateTime.MinValue)
                {
                    lastTime = DateTime.UtcNow;
                    lastTotalProcessorTime = Process.GetCurrentProcess().TotalProcessorTime;
                    return 0.0;
                }

                var currentTime = DateTime.UtcNow;
                var currentTotalProcessorTime = Process.GetCurrentProcess().TotalProcessorTime;

                var CPUUsage = (currentTotalProcessorTime.TotalMilliseconds - lastTotalProcessorTime.TotalMilliseconds) /
                               (currentTime.Subtract(lastTime).TotalMilliseconds * System.Environment.ProcessorCount);

                lastTime = currentTime;
                lastTotalProcessorTime = currentTotalProcessorTime;

                return CPUUsage * 100;
            }
            public override string ToString() => "<native fn System.getCPUUsage>";
        }
    }
}
