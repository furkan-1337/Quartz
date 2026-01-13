using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Quartz.Interfaces;

namespace Quartz.Runtime.Modules
{
    internal class NetworkModule : Module
    {
        private static readonly HttpClient client = new HttpClient();

        public NetworkModule() : base(new Environment())
        {
            ExportedEnv.Define("get", new NativeGet());
            ExportedEnv.Define("post", new NativePost());
            ExportedEnv.Define("downloadString", new NativeGet());
            ExportedEnv.Define("downloadBytes", new NativeDownloadBytes());
            ExportedEnv.Define("downloadFile", new NativeDownloadFile());
        }

        private class NativeGet : ICallable
        {
            public int Arity() => 1;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                string url = (string)arguments[0];
                try
                {
                    return client.GetStringAsync(url).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    throw new Exceptions.RuntimeError(interpreter.CurrentToken ?? new Parsing.Token(), ex.Message);
                }
            }
            public override string ToString() => "<native fn Network.get>";
        }

        private class NativePost : ICallable
        {
            public int Arity() => 2;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                string url = (string)arguments[0];
                string data = (string)arguments[1];
                try
                {
                    var content = new StringContent(data);
                    var response = client.PostAsync(url, content).GetAwaiter().GetResult();
                    return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    throw new Exceptions.RuntimeError(interpreter.CurrentToken ?? new Parsing.Token(), ex.Message);
                }
            }
            public override string ToString() => "<native fn Network.post>";
        }

        private class NativeDownloadBytes : ICallable
        {
            public int Arity() => 1;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                string url = (string)arguments[0];
                try
                {
                    byte[] bytes = client.GetByteArrayAsync(url).GetAwaiter().GetResult();
                    var list = new List<object>();
                    foreach (byte b in bytes) list.Add((int)b);
                    return list;
                }
                catch (Exception ex)
                {
                    throw new Exceptions.RuntimeError(interpreter.CurrentToken ?? new Parsing.Token(), $"Failed to download bytes: {ex.Message}");
                }
            }
            public override string ToString() => "<native fn Network.downloadBytes>";
        }

        private class NativeDownloadFile : ICallable
        {
            public int Arity() => 2;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                string url = (string)arguments[0];
                string path = (string)arguments[1];
                try
                {
                    byte[] bytes = client.GetByteArrayAsync(url).GetAwaiter().GetResult();
                    System.IO.File.WriteAllBytes(path, bytes);
                    return true;
                }
                catch (Exception ex)
                {
                    throw new Exceptions.RuntimeError(interpreter.CurrentToken ?? new Parsing.Token(), $"Failed to download file: {ex.Message}");
                }
            }
            public override string ToString() => "<native fn Network.downloadFile>";
        }
    }
}
