using System;
using System.Collections.Generic;
using System.IO;
using Quartz.Runtime;
using Quartz.Runtime.Native;
using Quartz.Interfaces;

namespace Quartz.Runtime.Modules
{
    internal class IOModule : Module
    {
        public IOModule() : base(new Environment())
        {
            ExportedEnv.Define("readFile", new NativeReadOnlyFile());
            ExportedEnv.Define("writeFile", new NativeWriteFile());
            ExportedEnv.Define("fileExists", new NativeFileExists());
            ExportedEnv.Define("appendFile", new NativeAppendFile());
            ExportedEnv.Define("deleteFile", new NativeDeleteFile());
        }

        private class NativeReadOnlyFile : ICallable
        {
            public int Arity() => 1;

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                try
                {
                    string path = (string)arguments[0];
                    return File.ReadAllText(path);
                }
                catch (Exception ex)
                {
                    throw new Exceptions.RuntimeError(interpreter.CurrentToken ?? new Parsing.Token(), $"Failed to read file '{arguments[0]}': {ex.Message}");
                }
            }
            public override string ToString() => "<native fn readFile>";
        }

        private class NativeWriteFile : ICallable
        {
            public int Arity() => 2;

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                try
                {
                    string path = (string)arguments[0];
                    string content = (string)arguments[1];
                    File.WriteAllText(path, content);
                    return true;
                }
                catch (Exception ex)
                {
                    throw new Exceptions.RuntimeError(interpreter.CurrentToken ?? new Parsing.Token(), $"Failed to write file '{arguments[0]}': {ex.Message}");
                }
            }
            public override string ToString() => "<native fn writeFile>";
        }

        private class NativeFileExists : ICallable
        {
            public int Arity() => 1;

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                string path = (string)arguments[0];
                return File.Exists(path);
            }
            public override string ToString() => "<native fn fileExists>";
        }

        private class NativeAppendFile : ICallable
        {
            public int Arity() => 2;

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                try
                {
                    string path = (string)arguments[0];
                    string content = (string)arguments[1];
                    File.AppendAllText(path, content);
                    return true;
                }
                catch (Exception ex)
                {
                    throw new Exceptions.RuntimeError(interpreter.CurrentToken ?? new Parsing.Token(), $"Failed to append to file '{arguments[0]}': {ex.Message}");
                }
            }
            public override string ToString() => "<native fn appendFile>";
        }

        private class NativeDeleteFile : ICallable
        {
            public int Arity() => 1;

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                try
                {
                    string path = (string)arguments[0];
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                        return true;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    throw new Exceptions.RuntimeError(interpreter.CurrentToken ?? new Parsing.Token(), $"Failed to delete file '{arguments[0]}': {ex.Message}");
                }
            }
            public override string ToString() => "<native fn deleteFile>";
        }
    }
}


