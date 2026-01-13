using System;
using System.Collections.Generic;
using Quartz.AST;
using Quartz.Parsing;

namespace Quartz.Runtime
{
    internal class Resolver
    {
        private readonly Interpreter interpreter;
        private readonly Stack<Dictionary<string, int>> scopes = new Stack<Dictionary<string, int>>();
        private readonly Stack<int> slotEscapements = new Stack<int>();

        public Resolver(Interpreter interpreter)
        {
            this.interpreter = interpreter;
        }

        public void Resolve(List<Stmt> statements)
        {
            foreach (var statement in statements)
            {
                Resolve(statement);
            }
        }

        private void Resolve(Stmt stmt)
        {
            switch (stmt)
            {
                case BlockStmt block:
                    BeginScope();
                    Resolve(block.Statements);
                    EndScope();
                    break;

                case VarDeclStmt varDecl:
                    Declare(varDecl.Name, varDecl);
                    if (varDecl.Initializer != null) Resolve(varDecl.Initializer);
                    Define(varDecl.Name);
                    break;

                case FunctionStmt function:
                    Declare(function.Name.Value);
                    Define(function.Name.Value);
                    ResolveFunction(function.Params, function.Body);
                    break;

                case ExpressionStmt exprStmt:
                    Resolve(exprStmt.Expression);
                    break;

                case IfStmt ifStmt:
                    Resolve(ifStmt.Condition);
                    Resolve(ifStmt.ThenBranch);
                    if (ifStmt.ElseBranch != null) Resolve(ifStmt.ElseBranch);
                    break;

                case WhileStmt whileStmt:
                    Resolve(whileStmt.Condition);
                    Resolve(whileStmt.Body);
                    break;

                case ForStmt forStmt:
                    BeginScope();
                    if (forStmt.Initializer != null) Resolve(forStmt.Initializer);
                    Resolve(forStmt.Condition);
                    Resolve(forStmt.Body);
                    if (forStmt.Increment != null) Resolve(forStmt.Increment);
                    EndScope();
                    break;

                case ReturnStmt returnStmt:
                    if (returnStmt.Value != null) Resolve(returnStmt.Value);
                    break;

                case ForeachStmt foreachStmt:
                    Resolve(foreachStmt.Collection);
                    BeginScope();
                    Declare(foreachStmt.VariableName.Value, foreachStmt);
                    Define(foreachStmt.VariableName.Value);
                    Resolve(foreachStmt.Body);
                    EndScope();
                    break;

                case TryStmt tryStmt:
                    Resolve(tryStmt.TryBlock);
                    if (tryStmt.CatchBlock != null)
                    {
                        BeginScope();
                        if (tryStmt.ErrorVariable != null)
                        {
                            Declare(tryStmt.ErrorVariable.Value, tryStmt);
                            Define(tryStmt.ErrorVariable.Value);
                        }
                        Resolve(tryStmt.CatchBlock);
                        EndScope();
                    }
                    break;

                case SwitchStmt switchStmt:
                    Resolve(switchStmt.Expression);
                    foreach (var c in switchStmt.Cases)
                    {
                        if (c.Value != null) Resolve(c.Value);
                        BeginScope();
                        Resolve(c.Body);
                        EndScope();
                    }
                    if (switchStmt.DefaultBranch != null)
                    {
                        BeginScope();
                        Resolve(switchStmt.DefaultBranch);
                        EndScope();
                    }
                    break;

                case ClassStmt classStmt:
                    Declare(classStmt.Name.Value);
                    Define(classStmt.Name.Value);

                    BeginScope();
                    scopes.Peek()["this"] = 0;

                    foreach (var method in classStmt.Methods)
                    {
                        ResolveFunction(method.Params, method.Body);
                    }
                    EndScope();
                    break;
                case StructStmt structStmt:
                    Declare(structStmt.Name.Value);
                    Define(structStmt.Name.Value);

                    BeginScope();
                    scopes.Peek()["this"] = 0;

                    foreach (var method in structStmt.Methods)
                    {
                        ResolveFunction(method.Params, method.Body);
                    }
                    EndScope();
                    break;
            }
        }

        private void Resolve(Expr expr)
        {
            switch (expr)
            {
                case VariableExpr variable:
                    ResolveLocal(variable, variable.Name);
                    break;

                case AssignExpr assign:
                    Resolve(assign.Value);
                    ResolveLocal(assign, assign.Name);
                    break;

                case CompoundAssignExpr compound:
                    Resolve(compound.Value);
                    Resolve(compound.Left);
                    if (compound.Left is VariableExpr cv) ResolveLocal(compound, cv.Name);
                    break;

                case BinaryExpr binary:
                    Resolve(binary.Left);
                    Resolve(binary.Right);
                    break;

                case CallExpr call:
                    Resolve(call.Callee);
                    foreach (var arg in call.Arguments) Resolve(arg);
                    break;

                case UnaryExpr unary:
                    Resolve(unary.Right);
                    if (unary.Operator.Type == TokenType.PlusPlus || unary.Operator.Type == TokenType.MinusMinus)
                    {
                        if (unary.Right is VariableExpr uv) ResolveLocal(unary, uv.Name);
                    }
                    break;

                case PostfixExpr postfix:
                    Resolve(postfix.Left);
                    if (postfix.Left is VariableExpr ve) ResolveLocal(postfix, ve.Name);
                    break;

                case LogicalExpr logical:
                    Resolve(logical.Left);
                    Resolve(logical.Right);
                    break;

                case GetExpr get:
                    Resolve(get.Object);
                    break;

                case SetExpr set:
                    Resolve(set.Value);
                    Resolve(set.Object);
                    break;

                case IndexExpr index:
                    Resolve(index.Object);
                    Resolve(index.Index);
                    break;

                case SetIndexExpr setIndex:
                    Resolve(setIndex.Value);
                    Resolve(setIndex.Object);
                    Resolve(setIndex.Index);
                    break;

                case ArrayExpr array:
                    foreach (var e in array.Values) Resolve(e);
                    break;

                case DictionaryExpr dict:
                    for (int i = 0; i < dict.Keys.Count; i++)
                    {
                        Resolve(dict.Keys[i]);
                        Resolve(dict.Values[i]);
                    }
                    break;

                case LambdaExpr lambda:
                    ResolveFunction(lambda.Params, lambda.Body);
                    break;

                case ThisExpr thisExpr:
                    ResolveLocal(thisExpr, "this");
                    break;
            }
        }

        private void ResolveFunction(List<Token> @params, List<Stmt> body)
        {
            BeginScope();
            foreach (var param in @params)
            {
                Declare(param.Value);
                Define(param.Value);
            }
            Resolve(body);
            EndScope();
        }

        private void BeginScope()
        {
            scopes.Push(new Dictionary<string, int>());
            slotEscapements.Push(0);
        }

        private void EndScope()
        {
            scopes.Pop();
            slotEscapements.Pop();
        }

        private void Declare(string name, Stmt? stmt = null)
        {
            if (scopes.Count == 0) return;
            var scope = scopes.Peek();
            if (scope.ContainsKey(name)) return;

            int index = slotEscapements.Peek();
            scope[name] = index;
            slotEscapements.Push(slotEscapements.Pop() + 1);

            if (stmt is VarDeclStmt v) v.SlotIndex = index;
            else if (stmt is ForeachStmt f) f.SlotIndex = index;
            else if (stmt is TryStmt t) t.SlotIndex = index;
        }

        private void Define(string name)
        {
        }

        private void ResolveLocal(Expr expr, string name)
        {
            var scopesArray = scopes.ToArray();
            for (int i = 0; i < scopesArray.Length; i++)
            {
                if (scopesArray[i].TryGetValue(name, out int index))
                {
                    if (expr is VariableExpr v) { v.Distance = i; v.SlotIndex = index; }
                    else if (expr is AssignExpr a) { a.Distance = i; a.SlotIndex = index; }
                    else if (expr is UnaryExpr u) { u.Distance = i; u.SlotIndex = index; }
                    else if (expr is PostfixExpr p) { p.Distance = i; p.SlotIndex = index; }
                    else if (expr is CompoundAssignExpr c) { c.Distance = i; c.SlotIndex = index; }
                    return;
                }
            }
        }

        private void ResolveLocal(ThisExpr expr, string name)
        {
            var scopesArray = scopes.ToArray();
            for (int i = 0; i < scopesArray.Length; i++)
            {
                if (scopesArray[i].TryGetValue(name, out int index))
                {
                    return;
                }
            }
        }
    }
}
