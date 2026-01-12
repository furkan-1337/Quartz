using Quartz.AST;
using Quartz.Parsing;

using Quartz.Runtime.FFI;
using Quartz.Runtime.Native;
using Quartz.Runtime.Modules;
using Quartz.Runtime.Types;
using Quartz.Runtime.Functions;
using Quartz.Interfaces;
using Quartz.Exceptions;

namespace Quartz.Runtime
{
    internal class Interpreter
    {
        public Environment global;
        private Environment environment;
        internal Environment CurrentEnvironment => environment;

        public Interpreter()
        {
            global = new Environment();
            environment = global;

            global.Define("extern", new ExternFunction());
            global.Define("import", new ImportFunction());
            global.Define("time", new TimeFunction());

            // Modules
            global.Define("Console", new ConsoleModule());
            global.Define("Math", new MathModule());
            global.Define("Thread", new ThreadModule());
            global.Define("IO", new IOModule());
            global.Define("Random", new RandomModule());
            global.Define("String", new StringModule());
            global.Define("Array", new ArrayModule());
            global.Define("Marshal", new MarshalModule());
            global.Define("Converter", new ConverterModule());
            global.Define("Process", new ProcessModule());

            // Global Native Functions
            global.Define("typeof", new TypeOfFunction());
        }

        public void Interpret(List<Stmt> statements)
        {
            foreach (var statement in statements)
            {
                Execute(statement);
            }
        }

        private void Execute(Stmt stmt)
        {
            switch (stmt)
            {
                case VarDeclStmt varDecl:
                    object value = Evaluate(varDecl.Initializer);
                    environment.Define(varDecl.Name, value);

                    break;

                case ExpressionStmt exprStmt:
                    Evaluate(exprStmt.Expression);
                    break;

                case BlockStmt block:
                    ExecuteBlock(block.Statements, new Environment(environment));
                    break;

                case IfStmt ifStmt:
                    if (IsTruth(Evaluate(ifStmt.Condition)))
                    {
                        Execute(ifStmt.ThenBranch);
                    }
                    else if (ifStmt.ElseBranch != null)
                    {
                        Execute(ifStmt.ElseBranch);
                    }
                    break;

                case WhileStmt whileStmt:
                    while (IsTruth(Evaluate(whileStmt.Condition)))
                    {
                        Execute(whileStmt.Body);
                    }
                    break;

                case FunctionStmt function:
                    Function func = new Function(function, environment);
                    environment.Define(function.Name.Value, func);
                    break;

                case ClassStmt classStmt:
                    environment.Define(classStmt.Name.Value, null);

                    Dictionary<string, Function> methods = new Dictionary<string, Function>();
                    foreach (var method in classStmt.Methods)
                    {
                        Function f = new Function(method, environment);
                        methods[method.Name.Value] = f;
                    }

                    QClass klass = new QClass(classStmt.Name.Value, methods);
                    environment.Assign(classStmt.Name.Value, klass);
                    break;

                case ReturnStmt returnStmt:
                    object returnValue = null;
                    if (returnStmt.Value != null) returnValue = Evaluate(returnStmt.Value);
                    throw new ReturnException(returnValue);

                default:
                    throw new Exception($"Unknown statement type: {stmt.GetType().Name}");
            }
        }

        public void ExecuteBlock(List<Stmt> statements, Environment environment)
        {
            Environment previous = this.environment;
            try
            {
                this.environment = environment;

                foreach (Stmt statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                this.environment = previous;
            }
        }

        private object Evaluate(Expr expr)
        {
            switch (expr)
            {
                case LiteralExpr literal:
                    return literal.Value;

                case VariableExpr variable:
                    return environment.Get(variable.Name);

                case AssignExpr assign:
                    object val = Evaluate(assign.Value);
                    environment.Assign(assign.Name, val);

                    return val;

                case BinaryExpr binary:
                    return EvaluateBinary(binary);

                case UnaryExpr unary:
                    object right = Evaluate(unary.Right);
                    switch (unary.Operator.Type)
                    {
                        case TokenType.Minus:
                            if (right is int n) return -n;
                            if (right is double d) return -d;
                            RuntimeErrorHandler.Error(unary.Operator, "Operand must be a number.");
                            break; // Added break to avoid fall-through after error
                        case TokenType.Bang:
                            return !IsTruth(right);
                    }
                    throw new Exception("Unknown unary operator.");

                case CallExpr call:
                    object callee = Evaluate(call.Callee);

                    List<object> arguments = new List<object>();
                    foreach (Expr argument in call.Arguments)
                    {
                        arguments.Add(Evaluate(argument));
                    }

                    if (!(callee is ICallable))
                    {
                        throw new Exception("Can only call functions and classes.");
                    }

                    ICallable function = (ICallable)callee;
                    if (function.Arity() != -1 && arguments.Count != function.Arity())
                    {
                        throw new Exception($"Expected {function.Arity()} arguments but got {arguments.Count}.");
                    }

                    return function.Call(this, arguments);

                case GetExpr getExpr:
                    object obj = Evaluate(getExpr.Object);
                    if (obj is QInstance instance)
                    {
                        return instance.Get(new Token { Value = getExpr.Name, Type = TokenType.Identifier });
                    }
                    if (obj is Module module)
                    {
                        return module.Get(getExpr.Name);
                    }
                    throw new RuntimeError(new Token { Value = getExpr.Name }, "Only instances have properties.");

                case SetExpr set:
                    object objectSet = Evaluate(set.Object);
                    if (!(objectSet is QInstance instanceSet))
                    {
                        throw new RuntimeError(new Token { Value = set.Name }, "Only instances have fields.");
                    }
                    object valueSet = Evaluate(set.Value);
                    instanceSet.Set(new Token { Value = set.Name }, valueSet);
                    return valueSet;

                case ThisExpr thisExpr:
                    return environment.Get(thisExpr.Keyword.Value);

                case ArrayExpr array:
                    List<object> values = new List<object>();
                    foreach (Expr element in array.Values)
                    {
                        values.Add(Evaluate(element));
                    }
                    return values;

                case IndexExpr indexExpr:
                    object collection = Evaluate(indexExpr.Object);
                    object index = Evaluate(indexExpr.Index);

                    if (collection is List<object> list)
                    {
                        int i = -1;
                        if (index is int n) i = n;
                        else if (index is double d) i = (int)d;
                        else throw new Exception("Index must be a number.");

                        if (i >= 0 && i < list.Count) return list[i];
                        throw new Exception("Index out of bounds.");
                    }
                    throw new Exception("Only arrays are indexable.");

                default:
                    throw new Exception($"Unknown expression type: {expr.GetType().Name}");
            }
        }

        private bool IsTruth(object? obj)
        {
            if (obj == null) return false;
            if (obj is bool b) return b;
            return true;
        }

        private object EvaluateBinary(BinaryExpr binary)
        {
            object left = Evaluate(binary.Left);
            object right = Evaluate(binary.Right);

            // Handle numeric operations
            if ((left is int || left is double) && (right is int || right is double))
            {
                double l = Convert.ToDouble(left);
                double r = Convert.ToDouble(right);

                // If both are int, return int (except division if we want float division?)
                // For now, let's auto-promote to double if any is double.
                // Or if we want strict typing? Quartz seems dynamic.
                // Let's use simple logic: if either is double, result is double.
                // If both are int, result is int -> actually maybe standard division for int is ok?
                // But Convert.ToDouble(left) makes it double.

                // Let's do the operation in double first.
                double result = 0;
                bool isBoolResult = false;
                bool boolResult = false;

                switch (binary.Operator.Type)
                {
                    case TokenType.Plus: result = l + r; break;
                    case TokenType.Minus: result = l - r; break;
                    case TokenType.Star: result = l * r; break;
                    case TokenType.Slash: result = l / r; break;
                    case TokenType.Greater: boolResult = l > r; isBoolResult = true; break;
                    case TokenType.GreaterEqual: boolResult = l >= r; isBoolResult = true; break;
                    case TokenType.Less: boolResult = l < r; isBoolResult = true; break;
                    case TokenType.LessEqual: boolResult = l <= r; isBoolResult = true; break;
                    case TokenType.EqualEqual: boolResult = l == r; isBoolResult = true; break;
                    case TokenType.BangEqual: boolResult = l != r; isBoolResult = true; break;
                }

                if (isBoolResult) return boolResult;

                // Return int if both operands were int and result is whole number? 
                // No, standard dynamic languages behavior: 
                // 5 / 2 = 2.5 (double). 5 + 2 = 7 (int).

                if (left is int && right is int && binary.Operator.Type != TokenType.Slash)
                {
                    return (int)result;
                }

                // If result is effectively an integer, we could return int, but let's stick to double 
                // if we want 'pure' float support or mixed.
                // For division of integers, 5/2, usually 2 in C# integer div, but here we cast to double earlier.
                // So l / r is 2.5. If we want integer division like C, we shouldn't cast to double first.
                // User wants "float" support. So 1 / 2 should probably be 0.5.

                return result;
            }

            // Handle Pointer Arithmetic
            if ((left is QPointer ptr && (right is int || right is double)) ||
                ((left is int || left is double) && right is QPointer))
            {
                QPointer p = left is QPointer lp ? lp : (QPointer)right;
                int offset = Convert.ToInt32(left is QPointer ? right : left);

                if (binary.Operator.Type == TokenType.Plus) return p + offset;
                if (binary.Operator.Type == TokenType.Minus && left is QPointer) return p - offset;
            }

            switch (binary.Operator.Type)
            {
                case TokenType.EqualEqual: return IsEqual(left, right);
                case TokenType.BangEqual: return !IsEqual(left, right);
            }

            // Handle string concatenation
            if (binary.Operator.Type == TokenType.Plus && (left is string || right is string))
            {
                return left.ToString() + right.ToString();
            }

            throw new Exception($"Runtime Error: Invalid operands for operator {binary.Operator.Value}: {left} ({left?.GetType()}), {right} ({right?.GetType()})");
        }

        private bool IsEqual(object a, object b)
        {
            if (a == null && b == null) return true;
            if (a == null) return false;
            return a.Equals(b);
        }
    }
}

