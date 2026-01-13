using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Quartz.Runtime;
using Quartz.Interfaces;
using Quartz.Runtime.Native;

namespace Quartz.Runtime.FFI
{
    internal class NativeCallable : ICallable
    {
        private readonly IntPtr functionPointer;
        private readonly Type delegateType;
        private readonly Delegate nativeDelegate;
        private readonly Type returnType;

        private static ModuleBuilder? moduleBuilder;

        public NativeCallable(IntPtr functionPointer, Type returnType, List<Type> paramTypes)
        {
            this.functionPointer = functionPointer;
            this.returnType = returnType;
            this.delegateType = CreateDelegateType(returnType, paramTypes.ToArray());
            this.nativeDelegate = Marshal.GetDelegateForFunctionPointer(functionPointer, delegateType);
        }

        private Type CreateDelegateType(Type returnType, Type[] paramTypes)
        {
            if (moduleBuilder == null)
            {
                var assemblyName = new AssemblyName("QuartzDynamicDelegates");
                var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
                moduleBuilder = assemblyBuilder.DefineDynamicModule("QuartzDynamicDelegates");
            }

            var typeBuilder = moduleBuilder.DefineType("NativeDelegate_" + Guid.NewGuid(),
                TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.AutoClass,
                typeof(System.MulticastDelegate));

            typeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                CallingConventions.Standard, new[] { typeof(object), typeof(IntPtr) })
                .SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);

            var methodBuilder = typeBuilder.DefineMethod("Invoke",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                returnType, paramTypes);

            methodBuilder.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);

            return typeBuilder.CreateType()!;
        }

        public int Arity() => -1; 

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            
            ParameterInfo[] parameters = nativeDelegate.Method.GetParameters();
            object[] args = new object[arguments.Count];

            for (int i = 0; i < arguments.Count; i++)
            {
                object arg = arguments[i];
                Type targetType = parameters[i].ParameterType;

                if (targetType == typeof(IntPtr) && arg is IntPtr pVal)
                {
                    args[i] = pVal;
                }
                else if (targetType == typeof(IntPtr) && arg is Quartz.Runtime.Types.QPointer qPtr)
                {
                    args[i] = new IntPtr(qPtr.Address);
                }
                else if (targetType == typeof(IntPtr) && arg is int iVal)
                {
                    args[i] = new IntPtr(iVal);
                }
                else if (targetType == typeof(IntPtr) && arg is long lVal)
                {
                    args[i] = new IntPtr(lVal);
                }
                else
                {
                    args[i] = arg;
                }
            }

            
            return nativeDelegate.DynamicInvoke(args);
        }

        public override string ToString() => "<native function>";
    }
}


