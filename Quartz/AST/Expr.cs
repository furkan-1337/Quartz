using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz.Parsing;

namespace Quartz.AST
{
    abstract class Expr { }

    class LiteralExpr : Expr
    {
        public object Value;
    }

    class VariableExpr : Expr
    {
        public string Name;
        internal int? Distance;
        internal int? SlotIndex;
    }

    class BinaryExpr : Expr
    {
        public Expr Left;
        public Token Operator;
        public Expr Right;
    }

    class AssignExpr : Expr
    {
        public string Name;
        public Expr Value;
        internal int? Distance;
        internal int? SlotIndex;
    }

    class CallExpr : Expr
    {
        public Expr Callee;
        public Token Paren;
        public List<Expr> Arguments;
    }

    class GetExpr : Expr
    {
        public Expr Object;
        public string Name;
    }

    class SetExpr : Expr
    {
        public Expr Object;
        public string Name;
        public Expr Value;
    }

    class ThisExpr : Expr
    {
        public Token Keyword;
    }

    class UnaryExpr : Expr
    {
        public Token Operator;
        public Expr Right;
        internal int? Distance;
        internal int? SlotIndex;
    }

    class PostfixExpr : Expr
    {
        public Expr Left;
        public Token Operator;
        internal int? Distance;
        internal int? SlotIndex;
    }

    class CompoundAssignExpr : Expr
    {
        public Expr Left;
        public Token Operator;
        public Expr Value;
        internal int? Distance;
        internal int? SlotIndex;
    }

    class ArrayExpr : Expr
    {
        public List<Expr> Values;
    }

    class IndexExpr : Expr
    {
        public Expr Object;
        public Token Bracket;
        public Expr Index;
    }

    class SetIndexExpr : Expr
    {
        public Expr Object;
        public Token Bracket;
        public Expr Index;
        public Expr Value;
    }

    class LogicalExpr : Expr
    {
        public Expr Left;
        public Token Operator;
        public Expr Right;
    }

    class DictionaryExpr : Expr
    {
        public List<Expr> Keys;
        public List<Expr> Values;
    }


    class BaseExpr : Expr
    {
        public Token Keyword;
        public Token Method;
    }

    class LambdaExpr : Expr
    {
        public List<Token> Params;
        public List<Stmt> Body;
    }
}


