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

        private Func<List<object>, object> invoker;

        public NativeCallable(IntPtr functionPointer, Type returnType, List<Type> paramTypes)
        {
            this.functionPointer = functionPointer;
            this.returnType = returnType;
            this.delegateType = CreateDelegateType(returnType, paramTypes.ToArray());
            this.nativeDelegate = Marshal.GetDelegateForFunctionPointer(functionPointer, delegateType);
            this.invoker = CreateInvoker(nativeDelegate, returnType, paramTypes.ToArray());
        }

        private Func<List<object>, object> CreateInvoker(Delegate del, Type retType, Type[] paramTypes)
        {
            var argsParam = Expression.Parameter(typeof(List<object>), "args");
            var callArgs = new Expression[paramTypes.Length];

            for (int i = 0; i < paramTypes.Length; i++)
            {
                var inputArg = Expression.Property(argsParam, "Item", Expression.Constant(i));
                var targetType = paramTypes[i];

                Expression convertedArg;

                if (targetType == typeof(IntPtr))
                {
                    var argObj = inputArg;
                    convertedArg = Expression.Call(typeof(NativeCallable), nameof(ConvertToIntPtr), null, argObj);
                }
                else
                {
                    convertedArg = Expression.Convert(
                        Expression.Call(typeof(NativeCallable), nameof(CoerceType), null, inputArg, Expression.Constant(targetType)),
                        targetType
                    );
                }

                callArgs[i] = convertedArg;
            }

            var invokeCall = Expression.Invoke(Expression.Constant(del), callArgs);

            Expression body;
            if (retType == typeof(void))
            {
                body = Expression.Block(invokeCall, Expression.Constant(null));
            }
            else
            {
                Expression result = invokeCall;
                if (retType == typeof(IntPtr))
                {
                    result = Expression.New(typeof(Quartz.Runtime.Types.QPointer).GetConstructor(new[] { typeof(long) })!,
                        Expression.Convert(result, typeof(long)));
                }

                body = Expression.Convert(result, typeof(object));
            }

            return Expression.Lambda<Func<List<object>, object>>(body, argsParam).Compile();
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

        public static object CoerceType(object value, Type targetType)
        {
            if (value == null) return null;
            if (targetType.IsInstanceOfType(value)) return value;

            if (targetType == typeof(int) && value is long l)
            {
                return unchecked((int)l);
            }

            if (targetType.IsEnum)
            {
                return Enum.ToObject(targetType, value);
            }

            return Convert.ChangeType(value, targetType);
        }

        public static IntPtr ConvertToIntPtr(object arg)
        {
            if (arg is IntPtr p) return p;
            if (arg is Quartz.Runtime.Types.QPointer qp) return new IntPtr(qp.Address);
            if (arg is int i) return new IntPtr(i);
            if (arg is long l) return new IntPtr(l);
            return IntPtr.Zero;
        }

        public int Arity() => -1;

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            return invoker(arguments);
        }

        public override string ToString() => "<native function>";
    }
}
