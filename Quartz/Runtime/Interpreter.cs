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

    internal class StackFrame
    {
        public string FunctionName { get; set; } = string.Empty;
        public string File { get; set; } = string.Empty;
        public int Line { get; set; }

        public void Initialize(string functionName, string file, int line)
        {
            FunctionName = functionName;
            File = file;
            Line = line;
        }
    }

    internal class Interpreter
    {
        private static readonly Stack<StackFrame> _framePool = new Stack<StackFrame>();
        private static readonly object _poolLock = new object();
        public bool DebugMode { get; set; } = true;

        [ThreadStatic]
        public static Interpreter? Current;

        private List<StackFrame> callStack = new List<StackFrame>();
        public IReadOnlyList<StackFrame> CallStack => callStack;

        private StackFrame RentFrame(string functionName, string file, int line)
        {
            lock (_poolLock)
            {
                if (_framePool.Count > 0)
                {
                    var frame = _framePool.Pop();
                    frame.Initialize(functionName, file, line);
                    return frame;
                }
            }
            var newFrame = new StackFrame();
            newFrame.Initialize(functionName, file, line);
            return newFrame;
        }

        private void ReturnFrame(StackFrame frame)
        {
            lock (_poolLock)
            {
                _framePool.Push(frame);
            }
        }

        public Environment global;
        private Environment environment;
        internal Environment CurrentEnvironment => environment;
        public Token? CurrentToken { get; private set; }

        public Interpreter()
        {
            Current = this;
            global = new Environment();
            environment = global;

            InitGlobals();
        }

        public Interpreter(Environment sharedGlobal)
        {
            Current = this;
            global = sharedGlobal;
            environment = global;
        }

        private void InitGlobals()
        {
            global.Define("extern", new ExternFunction());
            global.Define("import", new ImportFunction());
            global.Define("load", new LoadFunction());
            global.Define("null", (object?)null);

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

        public void Resolve(List<Stmt> statements)
        {
            Resolver resolver = new Resolver(this);
            resolver.Resolve(statements);
        }


        private object EvalWithEnv(Expr expr, Environment environment)
        {
            Environment previous = this.environment;
            try
            {
                this.environment = environment;
                return Evaluate(expr);
            }
            finally
            {
                this.environment = previous;
            }
        }

        private void ExecWithEnv(Stmt stmt, Environment environment)
        {
            Environment previous = this.environment;
            try
            {
                this.environment = environment;
                Execute(stmt);
            }
            finally
            {
                this.environment = previous;
            }
        }



        public void Interpret(List<Stmt> statements)
        {
            Resolve(statements);
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
                    CurrentToken = new Token { Line = 0, File = "unknown" }; // TODO: Stmt should have tokens
                    object value = Evaluate(varDecl.Initializer);
                    if (varDecl.SlotIndex.HasValue)
                    {
                        environment.DefineSlot(varDecl.SlotIndex.Value, value);
                    }
                    else
                    {
                        environment.Define(varDecl.Name, value);
                    }

                    break;

                case ExpressionStmt exprStmt:
                    Evaluate(exprStmt.Expression);
                    break;

                case BlockStmt block:
                    {
                        var blockEnv = Environment.Rent(environment);
                        try
                        {
                            ExecuteBlock(block.Statements, blockEnv);
                        }
                        finally
                        {
                            Environment.Return(blockEnv);
                        }
                    }
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
                    try
                    {
                        while (IsTruthy(Evaluate(whileStmt.Condition)))
                        {
                            try
                            {
                                Execute(whileStmt.Body);
                            }
                            catch (ContinueException) { }
                        }
                    }
                    catch (BreakException) { }
                    break;

                case ForStmt forStmt:
                    {
                        Environment forEnv = Environment.Rent(environment);
                        try
                        {
                            if (forStmt.Initializer != null) ExecWithEnv(forStmt.Initializer, forEnv);

                            try
                            {
                                while (IsTruthy(EvalWithEnv(forStmt.Condition, forEnv)))
                                {
                                    try
                                    {
                                        ExecuteBlock(new List<Stmt> { forStmt.Body }, forEnv);
                                    }
                                    catch (ContinueException) { }

                                    if (forStmt.Increment != null) EvalWithEnv(forStmt.Increment, forEnv);
                                }
                            }
                            catch (BreakException) { }
                        }
                        finally
                        {
                            Environment.Return(forEnv);
                        }
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

                    try
                    {
                        foreach (var item in enumerator)
                        {
                            Environment loopEnv = Environment.Rent(environment);
                            try
                            {
                                if (foreachStmt.SlotIndex.HasValue)
                                {
                                    loopEnv.DefineSlot(foreachStmt.SlotIndex.Value, item);
                                }
                                else
                                {
                                    loopEnv.Define(foreachStmt.VariableName.Value, item);
                                }
                                try
                                {
                                    ExecWithEnv(foreachStmt.Body, loopEnv);
                                }
                                catch (ContinueException) { }
                            }
                            finally
                            {
                                Environment.Return(loopEnv);
                            }
                        }
                    }
                    catch (BreakException) { }
                    break;

                case TryStmt tryStmt:
                    int stackDepth = callStack.Count;
                    try
                    {
                        Execute(tryStmt.TryBlock);
                    }
                    catch (Exception ex)
                    {
                        // Clean up leaked stack frames
                        while (callStack.Count > stackDepth)
                        {
                            var leakedFrame = callStack[^1];
                            callStack.RemoveAt(callStack.Count - 1);
                            ReturnFrame(leakedFrame);
                        }

                        Environment catchEnv = Environment.Rent(environment);
                        try
                        {
                            string errorMessage = ex.Message;
                            if (ex is ReturnException || ex is BreakException || ex is ContinueException) throw;

                            if (tryStmt.SlotIndex.HasValue)
                            {
                                catchEnv.DefineSlot(tryStmt.SlotIndex.Value, errorMessage);
                            }
                            else
                            {
                                catchEnv.Define(tryStmt.ErrorVariable.Value, errorMessage);
                            }
                            ExecWithEnv(tryStmt.CatchBlock, catchEnv);
                        }
                        finally
                        {
                            Environment.Return(catchEnv);
                        }
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
                                var caseEnv = Environment.Rent(environment);
                                try
                                {
                                    ExecuteBlock(caseStmt.Body, caseEnv);
                                }
                                finally
                                {
                                    Environment.Return(caseEnv);
                                }

                                return;
                            }
                        }

                        if (!matched && switchStmt.DefaultBranch != null)
                        {
                            var defaultEnv = Environment.Rent(environment);
                            try
                            {
                                ExecuteBlock(switchStmt.DefaultBranch, defaultEnv);
                            }
                            finally
                            {
                                Environment.Return(defaultEnv);
                            }
                        }
                    }
                    catch (BreakException)
                    {

                    }
                    break;

                case BreakStmt breakStmt:
                    throw new BreakException();

                case ContinueStmt continueStmt:
                    throw new ContinueException();

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
                case BinaryExpr binary:
                    CurrentToken = binary.Operator;
                    break;
                case CallExpr call:
                    CurrentToken = call.Paren;
                    break;
                case UnaryExpr unary:
                    CurrentToken = unary.Operator;
                    break;
            }

            switch (expr)
            {
                case LiteralExpr literal:
                    return literal.Value;

                case VariableExpr variable:
                    if (variable.Distance.HasValue)
                    {
                        return environment.GetAt(variable.Distance.Value, variable.SlotIndex.Value);
                    }
                    return environment.Get(variable.Name);

                case AssignExpr assign:
                    {
                        object val = Evaluate(assign.Value);
                        if (assign.Distance.HasValue)
                        {
                            environment.AssignAt(assign.Distance.Value, assign.SlotIndex.Value, val);
                        }
                        else
                        {
                            environment.Assign(assign.Name, val);
                        }

                        return val;
                    }

                case BinaryExpr binary:
                    return EvaluateBinary(binary);

                case CompoundAssignExpr compound:
                    return EvaluateCompoundAssign(compound);

                case PostfixExpr postfix:
                    return EvaluatePostfix(postfix);

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
                        case TokenType.BitwiseNot:
                            if (right is int i) return ~i;
                            if (right is long l) return ~l;
                            throw new RuntimeError(unary.Operator, "Operand must be an integer for bitwise not.");
                        case TokenType.PlusPlus:
                        case TokenType.MinusMinus:
                            return EvaluateIncrement(unary, true);
                    }
                    throw new Exception("Unknown unary operator.");

                case CallExpr call:
                    {
                        object callee = Evaluate(call.Callee);

                        List<object?> arguments = new List<object?>();
                        foreach (Expr argument in call.Arguments)
                        {
                            arguments.Add(Evaluate(argument));
                        }

                        if (!(callee is ICallable))
                        {
                            throw new RuntimeError(call.Paren, "Can only call functions and classes.");
                        }

                        ICallable function = (ICallable)callee;
                        if (function.Arity() != -1 && arguments.Count != function.Arity())
                        {
                            throw new RuntimeError(call.Paren, $"Expected {function.Arity()} arguments but got {arguments.Count}.");
                        }

                        try
                        {
                            StackFrame? frame = null;
                            if (DebugMode)
                            {
                                string funcName = "anonymous";
                                if (call.Callee is VariableExpr v) funcName = v.Name;
                                else if (call.Callee is GetExpr g) funcName = g.Name;

                                frame = RentFrame(funcName, call.Paren.File, call.Paren.Line);
                                callStack.Add(frame);
                            }

                            var result = function.Call(this, arguments);

                            if (frame != null)
                            {
                                callStack.RemoveAt(callStack.Count - 1);
                                ReturnFrame(frame);
                            }
                            return result;
                        }
                        catch (Exception ex) when (!(ex is RuntimeError || ex is ReturnException || ex is BreakException))
                        {
                            if (DebugMode && callStack.Count > 0 && callStack[^1].Line == call.Paren.Line)
                            {
                                var frame = callStack[^1];
                                callStack.RemoveAt(callStack.Count - 1);
                                ReturnFrame(frame);
                            }
                            throw new RuntimeError(call.Paren, ex.Message);
                        }
                    }

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
                        if (obj is string str)
                        {
                            if (getExpr.Name == "length") return str.Length;
                            throw new RuntimeError(new Token { Value = getExpr.Name }, "Strings only have a 'length' property.");
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
            if (obj is int n) return n != 0;
            if (obj is long l) return l != 0;
            if (obj is double d) return d != 0.0;
            if (obj is string s) return s.Length > 0;
            return true;
        }

        private object EvaluateBinary(BinaryExpr binary)
        {
            return EvaluateBinaryValue(Evaluate(binary.Left), Evaluate(binary.Right), binary.Operator);
        }

        private object EvaluateBinaryValue(object left, object right, Token op)
        {
            if (left is int li && right is int ri)
            {
                switch (op.Type)
                {
                    case TokenType.Plus: return li + ri;
                    case TokenType.Minus: return li - ri;
                    case TokenType.Star: return li * ri;
                    case TokenType.Slash: return (double)li / ri;
                    case TokenType.Greater: return li > ri;
                    case TokenType.GreaterEqual: return li >= ri;
                    case TokenType.Less: return li < ri;
                    case TokenType.LessEqual: return li <= ri;
                    case TokenType.EqualEqual: return li == ri;
                    case TokenType.BangEqual: return li != ri;
                    case TokenType.BitwiseAnd: return li & ri;
                    case TokenType.BitwiseOr: return li | ri;
                    case TokenType.BitwiseXor: return li ^ ri;
                    case TokenType.ShiftLeft: return li << ri;
                    case TokenType.ShiftRight: return li >> ri;
                }
            }

            if ((left is int || left is long || left is double) && (right is int || right is long || right is double))
            {
                if (left is double || right is double)
                {
                    double l = Convert.ToDouble(left);
                    double r = Convert.ToDouble(right);
                    switch (op.Type)
                    {
                        case TokenType.Plus: return l + r;
                        case TokenType.Minus: return l - r;
                        case TokenType.Star: return l * r;
                        case TokenType.Slash: return l / r;
                        case TokenType.Greater: return l > r;
                        case TokenType.GreaterEqual: return l >= r;
                        case TokenType.Less: return l < r;
                        case TokenType.LessEqual: return l <= r;
                        case TokenType.EqualEqual: return l == r;
                        case TokenType.BangEqual: return l != r;
                    }
                }
                else if (left is long || right is long)
                {
                    long l = Convert.ToInt64(left);
                    long r = Convert.ToInt64(right);
                    switch (op.Type)
                    {
                        case TokenType.Plus: return l + r;
                        case TokenType.Minus: return l - r;
                        case TokenType.Star: return l * r;
                        case TokenType.Slash: return (double)l / r;
                        case TokenType.Greater: return l > r;
                        case TokenType.GreaterEqual: return l >= r;
                        case TokenType.Less: return l < r;
                        case TokenType.LessEqual: return l <= r;
                        case TokenType.EqualEqual: return l == r;
                        case TokenType.BangEqual: return l != r;
                        case TokenType.BitwiseAnd: return l & r;
                        case TokenType.BitwiseOr: return l | r;
                        case TokenType.BitwiseXor: return l ^ r;
                        case TokenType.ShiftLeft: return (long)(l << (int)r);
                        case TokenType.ShiftRight: return (long)(l >> (int)r);
                    }
                }
                else
                {
                    int l = Convert.ToInt32(left);
                    int r = Convert.ToInt32(right);
                    switch (op.Type)
                    {
                        case TokenType.Plus: return l + r;
                        case TokenType.Minus: return l - r;
                        case TokenType.Star: return l * r;
                        case TokenType.Slash: return (double)l / r;
                        case TokenType.Greater: return l > r;
                        case TokenType.GreaterEqual: return l >= r;
                        case TokenType.Less: return l < r;
                        case TokenType.LessEqual: return l <= r;
                        case TokenType.EqualEqual: return l == r;
                        case TokenType.BangEqual: return l != r;
                        case TokenType.BitwiseAnd: return l & r;
                        case TokenType.BitwiseOr: return l | r;
                        case TokenType.BitwiseXor: return l ^ r;
                        case TokenType.ShiftLeft: return l << (int)r;
                        case TokenType.ShiftRight: return l >> (int)r;
                    }
                }
            }

            if (left is QStructInstance structInstance)
            {
                string methodName = "";
                switch (op.Type)
                {
                    case TokenType.Plus: methodName = "add"; break;
                    case TokenType.Minus: methodName = "subtract"; break;
                    case TokenType.Star: methodName = "multiply"; break;
                    case TokenType.Slash: methodName = "divide"; break;
                }

                if (!string.IsNullOrEmpty(methodName))
                {
                    object methodObj;
                    try
                    {
                        methodObj = structInstance.Get(new Token { Value = methodName, Type = TokenType.Identifier });
                    }
                    catch
                    {
                        throw new RuntimeError(op, $"Struct '{structInstance.Template.Name}' does not implement '{methodName}' method for '{op.Value}' operator.");
                    }

                    if (methodObj is ICallable method)
                    {
                        return method.Call(this, new List<object> { right });
                    }
                }
            }

            if ((left is QPointer ptr && (right is int || right is long || right is double)) ||
                ((left is int || left is long || left is double) && right is QPointer))
            {
                QPointer p = left is QPointer lp ? lp : (QPointer)right;
                int offset = Convert.ToInt32(left is QPointer ? right : left);

                if (op.Type == TokenType.Plus) return p + offset;
                if (op.Type == TokenType.Minus && left is QPointer) return p - offset;
            }

            switch (op.Type)
            {
                case TokenType.EqualEqual: return IsEqual(left, right);
                case TokenType.BangEqual: return !IsEqual(left, right);
            }

            if (op.Type == TokenType.Plus && (left is string || right is string))
            {
                return (left?.ToString() ?? "null") + (right?.ToString() ?? "null");
            }

            throw new RuntimeError(op, $"Invalid operands for operator {op.Value}: {left} ({left?.GetType().Name}), {right} ({right?.GetType().Name})");
        }

        private object EvaluateCompoundAssign(CompoundAssignExpr expr)
        {
            object right = Evaluate(expr.Value);
            object current = null;

            if (expr.Left is VariableExpr varExpr)
            {
                current = Evaluate(varExpr);
                object newVal = ExecuteMath(current, right, expr.Operator);
                if (expr.Distance.HasValue)
                    environment.AssignAt(expr.Distance.Value, expr.SlotIndex.Value, newVal);
                else
                    environment.Assign(varExpr.Name, newVal);
                return newVal;
            }
            else if (expr.Left is GetExpr getExpr)
            {
                object obj = Evaluate(getExpr.Object);
                if (obj is QInstance instance) current = instance.Get(new Token { Value = getExpr.Name, Type = TokenType.Identifier });
                else if (obj is QStructInstance si) current = si.Get(new Token { Value = getExpr.Name, Type = TokenType.Identifier });
                else throw new RuntimeError(expr.Operator, "Only instances and structs have fields.");

                object newVal = ExecuteMath(current, right, expr.Operator);
                if (obj is QInstance i2) i2.Set(new Token { Value = getExpr.Name }, newVal);
                else if (obj is QStructInstance si2) si2.Set(new Token { Value = getExpr.Name }, newVal);
                return newVal;
            }
            else if (expr.Left is IndexExpr indexExpr)
            {
                object obj = Evaluate(indexExpr.Object);
                object index = Evaluate(indexExpr.Index);

                if (obj is QArray arr)
                {
                    int i = (int)Convert.ChangeType(index, typeof(int));
                    current = arr.Elements[i];
                    object newVal = ExecuteMath(current, right, expr.Operator);
                    arr.Elements[i] = newVal;
                    return newVal;
                }
                else if (obj is QDictionary dict)
                {
                    current = dict.Elements[index];
                    object newVal = ExecuteMath(current, right, expr.Operator);
                    dict.Elements[index] = newVal;
                    return newVal;
                }
                else throw new RuntimeError(expr.Operator, "Only arrays and dictionaries are indexable.");
            }

            throw new RuntimeError(expr.Operator, "Invalid compound assignment target.");
        }

        private object ExecuteMath(object left, object right, Token op)
        {
            TokenType mathOp = TokenType.Plus;
            switch (op.Type)
            {
                case TokenType.PlusEqual: mathOp = TokenType.Plus; break;
                case TokenType.MinusEqual: mathOp = TokenType.Minus; break;
                case TokenType.StarEqual: mathOp = TokenType.Star; break;
                case TokenType.SlashEqual: mathOp = TokenType.Slash; break;
            }
            return EvaluateBinaryValue(left, right, new Token { Type = mathOp, Value = op.Value.Substring(0, 1) });
        }

        private object EvaluatePostfix(PostfixExpr postfix)
        {
            if (postfix.Left is VariableExpr varExpr)
            {
                object current = Evaluate(varExpr);
                object newVal = AddOne(current, postfix.Operator.Type == TokenType.PlusPlus);
                if (postfix.Distance.HasValue)
                    environment.AssignAt(postfix.Distance.Value, postfix.SlotIndex.Value, newVal);
                else
                    environment.Assign(varExpr.Name, newVal);
                return current;
            }
            else if (postfix.Left is GetExpr getExpr)
            {
                object obj = Evaluate(getExpr.Object);
                object current = null;
                if (obj is QInstance instance) current = instance.Get(new Token { Value = getExpr.Name, Type = TokenType.Identifier });
                else if (obj is QStructInstance si) current = si.Get(new Token { Value = getExpr.Name, Type = TokenType.Identifier });
                else throw new RuntimeError(postfix.Operator, "Only instances and structs have fields.");

                object newVal = AddOne(current, postfix.Operator.Type == TokenType.PlusPlus);
                if (obj is QInstance i2) i2.Set(new Token { Value = getExpr.Name }, newVal);
                else if (obj is QStructInstance si2) si2.Set(new Token { Value = getExpr.Name }, newVal);
                return current;
            }
            else if (postfix.Left is IndexExpr indexExpr)
            {
                object obj = Evaluate(indexExpr.Object);
                object index = Evaluate(indexExpr.Index);
                object current = null;

                if (obj is QArray arr)
                {
                    int i = (int)Convert.ChangeType(index, typeof(int));
                    current = arr.Elements[i];
                    object newVal = AddOne(current, postfix.Operator.Type == TokenType.PlusPlus);
                    arr.Elements[i] = newVal;
                }
                else if (obj is QDictionary dict)
                {
                    current = dict.Elements[index];
                    object newVal = AddOne(current, postfix.Operator.Type == TokenType.PlusPlus);
                    dict.Elements[index] = newVal;
                }
                else throw new RuntimeError(postfix.Operator, "Only arrays and dictionaries are indexable.");

                return current;
            }

            throw new RuntimeError(postfix.Operator, "Invalid postfix increment/decrement target.");
        }

        private object EvaluateIncrement(UnaryExpr unary, bool isPrefix)
        {
            Expr target = unary.Right;
            if (target is VariableExpr varExpr)
            {
                object current = Evaluate(varExpr);
                object newVal = AddOne(current, unary.Operator.Type == TokenType.PlusPlus);
                if (unary.Distance.HasValue)
                    environment.AssignAt(unary.Distance.Value, unary.SlotIndex.Value, newVal);
                else
                    environment.Assign(varExpr.Name, newVal);
                return newVal;
            }
            else if (target is GetExpr getExpr)
            {
                object obj = Evaluate(getExpr.Object);
                object current = null;
                if (obj is QInstance instance) current = instance.Get(new Token { Value = getExpr.Name, Type = TokenType.Identifier });
                else if (obj is QStructInstance si) current = si.Get(new Token { Value = getExpr.Name, Type = TokenType.Identifier });
                else throw new RuntimeError(unary.Operator, "Only instances and structs have fields.");

                object newVal = AddOne(current, unary.Operator.Type == TokenType.PlusPlus);
                if (obj is QInstance i2) i2.Set(new Token { Value = getExpr.Name }, newVal);
                else if (obj is QStructInstance si2) si2.Set(new Token { Value = getExpr.Name }, newVal);
                return newVal;
            }
            else if (target is IndexExpr indexExpr)
            {
                object obj = Evaluate(indexExpr.Object);
                object index = Evaluate(indexExpr.Index);
                object current = null;

                object newVal;
                if (obj is QArray arr)
                {
                    int i = (int)Convert.ChangeType(index, typeof(int));
                    current = arr.Elements[i];
                    newVal = AddOne(current, unary.Operator.Type == TokenType.PlusPlus);
                    arr.Elements[i] = newVal;
                }
                else if (obj is QDictionary dict)
                {
                    current = dict.Elements[index];
                    newVal = AddOne(current, unary.Operator.Type == TokenType.PlusPlus);
                    dict.Elements[index] = newVal;
                }
                else throw new RuntimeError(unary.Operator, "Only arrays and dictionaries are indexable.");

                return newVal;
            }

            throw new RuntimeError(unary.Operator, "Invalid increment/decrement target.");
        }

        private object AddOne(object val, bool increment)
        {
            if (val is int i) return increment ? i + 1 : i - 1;
            if (val is long l) return increment ? l + 1 : l - 1;
            if (val is double d) return increment ? d + 1 : d - 1;
            if (val is float f) return increment ? f + 1 : f - 1;
            throw new Exception($"Cannot increment/decrement type {val?.GetType().Name}");
        }

        private bool IsEqual(object a, object b)
        {
            if (a == null && b == null) return true;
            if (a == null) return false;
            return a.Equals(b);
        }
    }
}


