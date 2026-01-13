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
        public Expr Superclass; 
        public List<FunctionStmt> Methods;
    }

    class StructField
    {
        public Token Type;
        public Token Name;
    }

    class StructStmt : Stmt
    {
        public Token Name;
        public List<StructField> Fields;
        public List<FunctionStmt> Methods;
    }

    class ForeachStmt : Stmt
    {
        public Token VariableName;
        public Expr Collection;
        public Stmt Body;
    }

    class TryStmt : Stmt
    {
        public Stmt TryBlock;
        public Stmt CatchBlock;
        public Token ErrorVariable;
    }

    class SwitchCase
    {
        public Expr Value; 
        
        
        
        public List<Stmt> Body;
    }

    class SwitchStmt : Stmt
    {
        public Expr Expression;
        public List<SwitchCase> Cases;
        public List<Stmt> DefaultBranch;
    }

    class BreakStmt : Stmt
    {
        public Token Keyword;
    }



    class EnumStmt : Stmt
    {
        public Token Name;
        public List<Token> Members;
    }
}


