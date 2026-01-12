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
}

