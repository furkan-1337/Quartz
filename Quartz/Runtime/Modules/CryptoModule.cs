using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Quartz.Interfaces;

namespace Quartz.Runtime.Modules
{
    internal class CryptoModule : Module
    {
        public CryptoModule() : base(new Environment())
        {
            ExportedEnv.Define("sha256", new NativeSha256());
            ExportedEnv.Define("md5", new NativeMd5());
            ExportedEnv.Define("base64Encode", new NativeBase64Encode());
            ExportedEnv.Define("base64Decode", new NativeBase64Decode());
        }

        private class NativeSha256 : ICallable
        {
            public int Arity() => 1;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                string data = (string)arguments[0];
                using (var sha256 = SHA256.Create())
                {
                    byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
                    return BitConverter.ToString(bytes).Replace("-", "").ToLower();
                }
            }
            public override string ToString() => "<native fn Crypto.sha256>";
        }

        private class NativeMd5 : ICallable
        {
            public int Arity() => 1;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                string data = (string)arguments[0];
                using (var md5 = MD5.Create())
                {
                    byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(data));
                    return BitConverter.ToString(bytes).Replace("-", "").ToLower();
                }
            }
            public override string ToString() => "<native fn Crypto.md5>";
        }

        private class NativeBase64Encode : ICallable
        {
            public int Arity() => 1;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                string data = (string)arguments[0];
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
            }
            public override string ToString() => "<native fn Crypto.base64Encode>";
        }

        private class NativeBase64Decode : ICallable
        {
            public int Arity() => 1;
            public object Call(Interpreter interpreter, List<object> arguments)
            {
                string data = (string)arguments[0];
                try
                {
                    byte[] bytes = Convert.FromBase64String(data);
                    return Encoding.UTF8.GetString(bytes);
                }
                catch { return ""; }
            }
            public override string ToString() => "<native fn Crypto.base64Decode>";
        }
    }
}

