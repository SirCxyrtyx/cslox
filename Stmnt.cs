using System;
using System.Collections.Generic;

namespace CSharpLox
{
    public partial interface IVisitor<T>
    {
        public T Visit(ExpressionStatement stmnt);
        public T Visit(PrintStatement stmnt);
        public T Visit(VarStatement stmnt);
        public T Visit(BlockStatement stmnt);
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

    public class BlockStatement : Stmnt
    {
        public List<Stmnt> Statements;

        public BlockStatement(List<Stmnt> statements)
        {
            Statements = statements;
        }

        public override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }
}
