using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz.Parsing;

namespace Quartz.AST
{
    abstract class Stmt { }

    class VarDeclStmt : Stmt
    {
        public string Name;
        public Expr Initializer;
    }

    class ExpressionStmt : Stmt
    {
        public Expr Expression;
    }

    class BlockStmt : Stmt
    {
        public List<Stmt> Statements;
    }

    class IfStmt : Stmt
    {
        public Expr Condition;
        public Stmt ThenBranch;
        public Stmt ElseBranch;
    }

    class WhileStmt : Stmt
    {
        public Expr Condition;
        public Stmt Body;
    }

    class FunctionStmt : Stmt
    {
        public Token Name;
        public List<Token> Params;
        public List<Stmt> Body;
    }

    class ReturnStmt : Stmt
    {
        public Token Keyword;
        public Expr Value;
    }

    class ClassStmt : Stmt
    {
        public Token Name;
        public List<FunctionStmt> Methods;
    }
}

