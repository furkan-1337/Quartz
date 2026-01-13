using System;
using System.Collections.Generic;
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
    public class ReturnException : Exception
    {
        public object Value { get; }
        public ReturnException(object value) { Value = value; }
    }

    public class BreakException : Exception { }

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
            global.Define("System", new SystemModule());
            global.Define("Network", new NetworkModule());
            global.Define("Input", new InputModule());
            global.Define("Crypto", new CryptoModule());
            global.Define("Env", new EnvModule());
            global.Define("Window", new WindowModule());
            global.Define("Callback", new CallbackModule());

            
            global.Define("typeof", new TypeOfFunction());
            global.Define("sizeof", new SizeOfFunction());
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
                    if (IsTruthy(Evaluate(ifStmt.Condition)))
                    {
                        Execute(ifStmt.ThenBranch);
                    }
                    else if (ifStmt.ElseBranch != null)
                    {
                        Execute(ifStmt.ElseBranch);
                    }
                    break;

                case WhileStmt whileStmt:
                    while (IsTruthy(Evaluate(whileStmt.Condition)))
                    {
                        Execute(whileStmt.Body);
                    }
                    break;

                case ForeachStmt foreachStmt:
                    object collection = Evaluate(foreachStmt.Collection);

                    IEnumerable<object> enumerator = null;

                    if (collection is QArray array)
                    {
                        enumerator = array.Elements;
                    }
                    else if (collection is QDictionary dictionary)
                    {
                        enumerator = dictionary.Elements.Keys;
                    }
                    else
                    {
                        throw new Exception("Can only iterate over arrays or dictionaries.");
                    }

                    foreach (var item in enumerator)
                    {
                        
                        
                        
                        

                        
                        

                        
                        Environment loopEnv = new Environment(environment);
                        loopEnv.Define(foreachStmt.VariableName.Value, item);

                        
                        
                        

                        
                        
                        

                        EvaluateWithEnvironment(foreachStmt.Body, loopEnv);
                    }
                    break;

                case TryStmt tryStmt:
                    try
                    {
                        Execute(tryStmt.TryBlock);
                    }
                    catch (Exception ex)
                    {
                        
                        Environment catchEnv = new Environment(environment);

                        
                        
                        string errorMessage = ex.Message;
                        if (ex is ReturnException) throw; 

                        
                        
                        catchEnv.Define(tryStmt.ErrorVariable.Value, errorMessage);

                        EvaluateWithEnvironment(tryStmt.CatchBlock, catchEnv);
                    }
                    break;

                case SwitchStmt switchStmt:
                    object switchValue = Evaluate(switchStmt.Expression);
                    bool matched = false;

                    try
                    {
                        foreach (var caseStmt in switchStmt.Cases)
                        {
                            object caseValue = Evaluate(caseStmt.Value);
                            
                            if (IsEqual(switchValue, caseValue))
                            {
                                matched = true;
                                ExecuteBlock(caseStmt.Body, new Environment(environment));
                                
                                return;
                            }
                        }

                        if (!matched && switchStmt.DefaultBranch != null)
                        {
                            ExecuteBlock(switchStmt.DefaultBranch, new Environment(environment));
                        }
                    }
                    catch (BreakException)
                    {
                        
                    }
                    break;

                case BreakStmt breakStmt:
                    throw new BreakException();

                case FunctionStmt function:
                    Function func = new Function(function, environment);
                    environment.Define(function.Name.Value, func);
                    break;

                case ClassStmt classStmt:
                    object superclass = null;
                    if (classStmt.Superclass != null)
                    {
                        superclass = Evaluate(classStmt.Superclass);
                        if (!(superclass is QClass))
                        {
                            throw new Exception($"Superclass must be a class. {classStmt.Superclass} is {superclass?.GetType().Name}");
                        }
                    }

                    environment.Define(classStmt.Name.Value, null);

                    Environment methodEnv = environment;
                    if (classStmt.Superclass != null)
                    {
                        methodEnv = new Environment(methodEnv);
                        methodEnv.Define("base", superclass);
                    }

                    Dictionary<string, Function> methods = new Dictionary<string, Function>();
                    foreach (var method in classStmt.Methods)
                    {
                        Function f = new Function(method, methodEnv);
                        methods[method.Name.Value] = f;
                    }

                    QClass klass = new QClass(classStmt.Name.Value, (QClass)superclass, methods);
                    environment.Assign(classStmt.Name.Value, klass);
                    break;

                case EnumStmt enumStmt:
                    Dictionary<string, object> enumMembers = new Dictionary<string, object>();
                    for (int i = 0; i < enumStmt.Members.Count; i++)
                    {
                        enumMembers[enumStmt.Members[i].Value] = i;
                    }
                    QEnum qEnum = new QEnum(enumStmt.Name.Value, enumMembers);
                    environment.Define(enumStmt.Name.Value, qEnum);
                    break;

                case StructStmt structStmt:
                    Dictionary<string, Function> structMethods = new Dictionary<string, Function>();
                    if (structStmt.Methods != null)
                    {
                        foreach (var method in structStmt.Methods)
                        {
                            Function f = new Function(method, environment);
                            structMethods[method.Name.Value] = f;
                        }
                    }
                    QStruct @struct = new QStruct(structStmt.Name.Value, structStmt.Fields, structMethods);
                    environment.Define(structStmt.Name.Value, @struct);
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

        private void EvaluateWithEnvironment(Stmt stmt, Environment env)
        {
            Environment previous = this.environment;
            try
            {
                this.environment = env;
                Execute(stmt);
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
                    {
                        object val = Evaluate(assign.Value);
                        environment.Assign(assign.Name, val);

                        return val;
                    }

                case BinaryExpr binary:
                    return EvaluateBinary(binary);

                case LogicalExpr logical:
                    {
                        object left = Evaluate(logical.Left);

                        if (logical.Operator.Type == TokenType.Or)
                        {
                            if (IsTruthy(left)) return left;
                        }
                        else
                        {
                            if (!IsTruthy(left)) return left;
                        }

                        return Evaluate(logical.Right);
                    }



                case UnaryExpr unary:
                    object right = Evaluate(unary.Right);
                    switch (unary.Operator.Type)
                    {
                        case TokenType.Minus:
                            if (right is int n) return -n;
                            if (right is double d) return -d;
                            RuntimeErrorHandler.Error(unary.Operator, "Operand must be a number.");
                            break; 
                        case TokenType.Bang:
                            return !IsTruthy(right);
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
                    {
                        object obj = Evaluate(getExpr.Object);
                        if (obj is QInstance instance)
                        {
                            return instance.Get(new Token { Value = getExpr.Name, Type = TokenType.Identifier });
                        }
                        if (obj is Module module)
                        {
                            return module.Get(getExpr.Name);
                        }
                        if (obj is QStructInstance structInstance)
                        {
                            return structInstance.Get(new Token { Value = getExpr.Name, Type = TokenType.Identifier });
                        }
                        if (obj is QEnum qEnum)
                        {
                            return qEnum.Get(new Token { Value = getExpr.Name, Type = TokenType.Identifier });
                        }
                        if (obj is QPointer ptr)
                        {
                            if (getExpr.Name.ToLower() == "address") return ptr.Address;
                            throw new RuntimeError(new Token { Value = getExpr.Name }, "Pointers only have an 'address' property.");
                        }
                        if (obj is IDictionary<string, object> dictionary)
                        {
                            if (dictionary.TryGetValue(getExpr.Name, out var value))
                            {
                                return value;
                            }
                            return null;
                        }
                        throw new RuntimeError(new Token { Value = getExpr.Name }, "Only instances have properties.");
                    }

                case SetExpr set:
                    {
                        object objectSet = Evaluate(set.Object);
                        if (!(objectSet is QInstance instanceSet))
                        {
                            if (objectSet is QStructInstance structSet)
                            {
                                object valSet = Evaluate(set.Value);
                                structSet.Set(new Token { Value = set.Name }, valSet);
                                return valSet;
                            }
                            throw new RuntimeError(new Token { Value = set.Name }, "Only instances and structs have fields.");
                        }
                        object valueSet = Evaluate(set.Value);
                        instanceSet.Set(new Token { Value = set.Name }, valueSet);
                        return valueSet;
                    }

                case ThisExpr thisExpr:
                    return environment.Get(thisExpr.Keyword.Value);

                case BaseExpr baseExpr:
                    object super = environment.Get(baseExpr.Keyword.Value);
                    object objectInstance = environment.Get("this");
                    Function method = ((QClass)super).FindMethod(baseExpr.Method.Value);
                    if (method == null)
                    {
                        throw new RuntimeError(baseExpr.Method, $"Undefined property '{baseExpr.Method.Value}'.");
                    }
                    return method.Bind((QInstance)objectInstance);

                case ArrayExpr array:
                    {
                        List<object> values = new List<object>();
                        foreach (Expr element in array.Values)
                        {
                            values.Add(Evaluate(element));
                        }
                        return new QArray(values);
                    }

                case DictionaryExpr dictionaryExpr:
                    {
                        var dict = new Dictionary<object, object>();
                        for (int i = 0; i < dictionaryExpr.Keys.Count; i++)
                        {
                            object key = Evaluate(dictionaryExpr.Keys[i]);
                            object value = Evaluate(dictionaryExpr.Values[i]);
                            dict[key] = value;
                        }
                        return new QDictionary(dict);
                    }

                case LambdaExpr lambda:
                    return new Function(null, lambda.Params, lambda.Body, environment);

                case IndexExpr indexExpr:
                    {
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
                        if (collection is QArray array)
                        {
                            int i = -1;
                            if (index is int n) i = n;
                            else if (index is double d) i = (int)d;
                            else throw new Exception("Index must be a number.");

                            if (i >= 0 && i < array.Elements.Count) return array.Elements[i];
                            throw new Exception("Index out of bounds.");
                        }
                        if (collection is QDictionary qDict)
                        {
                            if (qDict.Elements.TryGetValue(index, out var val)) return val;
                            return null; 
                        }
                        if (collection is IDictionary<string, object> dictionary)
                        {
                            string key = index?.ToString() ?? "";
                            if (dictionary.TryGetValue(key, out var val)) return val;
                            return null;
                        }
                        throw new Exception("Only arrays and maps are indexable.");
                    }

                case SetIndexExpr setIndex:
                    {
                        object collection = Evaluate(setIndex.Object);
                        object index = Evaluate(setIndex.Index);
                        object value = Evaluate(setIndex.Value);

                        if (collection is List<object> list)
                        {
                            int i = -1;
                            if (index is int n) i = n;
                            else if (index is double d) i = (int)d;
                            else throw new Exception("Index must be a number.");

                            if (i >= 0 && i < list.Count)
                            {
                                list[i] = value;
                                return value;
                            }
                            throw new Exception("Index out of bounds.");
                        }
                        if (collection is QArray array)
                        {
                            int i = -1;
                            if (index is int n) i = n;
                            else if (index is double d) i = (int)d;
                            else throw new Exception("Index must be a number.");

                            if (i >= 0 && i < array.Elements.Count)
                            {
                                array.Elements[i] = value;
                                return value;
                            }
                            
                            throw new Exception("Index out of bounds.");
                        }
                        if (collection is QDictionary qDict)
                        {
                            qDict.Elements[index] = value;
                            return value;
                        }
                        if (collection is IDictionary<string, object> dictionary)
                        {
                            string key = index?.ToString() ?? "";
                            dictionary[key] = value;
                            return value;
                        }

                        throw new Exception("Only arrays and maps are indexable.");
                    }

                default:
                    throw new Exception($"Unknown expression type: {expr.GetType().Name}");
            }
        }

        private bool IsTruthy(object? obj)
        {
            if (obj == null) return false;
            if (obj is bool b) return b;
            return true;
        }

        private object EvaluateBinary(BinaryExpr binary)
        {
            object left = Evaluate(binary.Left);
            object right = Evaluate(binary.Right);

            
            if ((left is int || left is double) && (right is int || right is double))
            {
                double l = Convert.ToDouble(left);
                double r = Convert.ToDouble(right);

                
                
                
                
                
                

                
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

                
                
                

                if (left is int && right is int && binary.Operator.Type != TokenType.Slash)
                {
                    return (int)result;
                }

                
                
                
                
                

                return result;
            }

            
            if (left is QStructInstance structInstance)
            {
                string methodName = "";
                switch (binary.Operator.Type)
                {
                    case TokenType.Plus: methodName = "add"; break;
                    case TokenType.Minus: methodName = "subtract"; break;
                    case TokenType.Star: methodName = "multiply"; break;
                    case TokenType.Slash: methodName = "divide"; break;
                }

                if (!string.IsNullOrEmpty(methodName))
                {
                    
                    
                    try
                    {
                        object methodObj = structInstance.Get(new Token { Value = methodName, Type = TokenType.Identifier });
                        if (methodObj is ICallable method)
                        {
                            return method.Call(this, new List<object> { right });
                        }
                    }
                    catch
                    {
                        throw new RuntimeError(binary.Operator, $"Struct '{structInstance.Template.Name}' does not implement '{methodName}' method for '{binary.Operator.Value}' operator.");
                    }
                }
            }

            
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

            
            if (binary.Operator.Type == TokenType.Plus && (left is string || right is string))
            {
                return left.ToString() + right.ToString();
            }

            throw new RuntimeError(binary.Operator, $"Invalid operands for operator {binary.Operator.Value}: {left} ({left?.GetType().Name}), {right} ({right?.GetType().Name})");
        }

        private bool IsEqual(object a, object b)
        {
            if (a == null && b == null) return true;
            if (a == null) return false;
            return a.Equals(b);
        }
    }
}


