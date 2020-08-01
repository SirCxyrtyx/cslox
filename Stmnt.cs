using System;
using System.Collections.Generic;

namespace CSharpLox
{
    public partial interface IVisitor<T>
    {
        public T Visit(ExpressionStatement stmnt);
        public T Visit(PrintStatement stmnt);
        public T Visit(ReturnStatement stmnt);
        public T Visit(VarStatement stmnt);
        public T Visit(ClassDeclaration stmnt);
        public T Visit(BlockStatement stmnt);
        public T Visit(Function stmnt);
        public T Visit(IfStatement stmnt);
        public T Visit(WhileStatement stmnt);
    }

    public abstract class Stmnt
    {
        public abstract T AcceptVisitor<T>(IVisitor<T> visitor);
    }

    public class ExpressionStatement : Stmnt
    {
        public Expr Expression;

        public ExpressionStatement(Expr expression)
        {
            Expression = expression;
        }

        public override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }

    public class PrintStatement : Stmnt
    {
        public Expr Expression;

        public PrintStatement(Expr expression)
        {
            Expression = expression;
        }

        public override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }

    public class ReturnStatement : Stmnt
    {
        public Token Keyword;
        public Expr Value;

        public ReturnStatement(Token keyword, Expr value)
        {
            Keyword = keyword;
            Value = value;
        }

        public override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }

    public class VarStatement : Stmnt
    {
        public Token Name;
        public Expr Initializer;

        public VarStatement(Token name, Expr initializer)
        {
            Name = name;
            Initializer = initializer;
        }

        public override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }

    public class ClassDeclaration : Stmnt
    {
        public Token Name;
        public Variable Superclass;
        public List<Function> Methods;

        public ClassDeclaration(Token name, Variable superclass, List<Function> methods)
        {
            Name = name;
            Superclass = superclass;
            Methods = methods;
        }

        public override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }

    public class BlockStatement : Stmnt
    {
        public List<Stmnt> Statements;

        public BlockStatement(List<Stmnt> statements)
        {
            Statements = statements;
        }

        public override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }

    public class Function : Stmnt
    {
        public Token Name;
        public List<Token> Parameters;
        public List<Stmnt> Body;

        public Function(Token name, List<Token> parameters, List<Stmnt> body)
        {
            Name = name;
            Parameters = parameters;
            Body = body;
        }

        public override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }

    public class IfStatement : Stmnt
    {
        public Expr Condition;
        public Stmnt ThenBranch;
        public Stmnt ElseBranch;

        public IfStatement(Expr condition, Stmnt thenbranch, Stmnt elsebranch)
        {
            Condition = condition;
            ThenBranch = thenbranch;
            ElseBranch = elsebranch;
        }

        public override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }

    public class WhileStatement : Stmnt
    {
        public Expr Condition;
        public Stmnt Body;

        public WhileStatement(Expr condition, Stmnt body)
        {
            Condition = condition;
            Body = body;
        }

        public override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }
}
