using System;
using System.Collections.Generic;

namespace CSharpLox
{
    public partial interface IVisitor<T>
    {
        public T Visit(Assign expr);
        public T Visit(Binary expr);
        public T Visit(Call expr);
        public T Visit(Get expr);
        public T Visit(Grouping expr);
        public T Visit(Literal expr);
        public T Visit(Logical expr);
        public T Visit(SetExpr expr);
        public T Visit(This expr);
        public T Visit(Unary expr);
        public T Visit(Ternary expr);
        public T Visit(Variable expr);
    }

    public abstract class Expr
    {
        public abstract T AcceptVisitor<T>(IVisitor<T> visitor);
    }

    public class Assign : Expr
    {
        public Token Name;
        public Expr Value;

        public Assign(Token name, Expr value)
        {
            Name = name;
            Value = value;
        }

        public override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }

    public class Binary : Expr
    {
        public Expr Left;
        public Token Op;
        public Expr Right;

        public Binary(Expr left, Token op, Expr right)
        {
            Left = left;
            Op = op;
            Right = right;
        }

        public override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }

    public class Call : Expr
    {
        public Expr Callee;
        public Token Paren;
        public List<Expr> Args;

        public Call(Expr callee, Token paren, List<Expr> args)
        {
            Callee = callee;
            Paren = paren;
            Args = args;
        }

        public override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }

    public class Get : Expr
    {
        public Expr Obj;
        public Token Name;

        public Get(Expr obj, Token name)
        {
            Obj = obj;
            Name = name;
        }

        public override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }

    public class Grouping : Expr
    {
        public Expr Expression;

        public Grouping(Expr expression)
        {
            Expression = expression;
        }

        public override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }

    public class Literal : Expr
    {
        public object Value;

        public Literal(object value)
        {
            Value = value;
        }

        public override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }

    public class Logical : Expr
    {
        public Expr Left;
        public Token Op;
        public Expr Right;

        public Logical(Expr left, Token op, Expr right)
        {
            Left = left;
            Op = op;
            Right = right;
        }

        public override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }

    public class SetExpr : Expr
    {
        public Expr Obj;
        public Token Name;
        public Expr Value;

        public SetExpr(Expr obj, Token name, Expr value)
        {
            Obj = obj;
            Name = name;
            Value = value;
        }

        public override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }

    public class This : Expr
    {
        public Token Keyword;

        public This(Token keyword)
        {
            Keyword = keyword;
        }

        public override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }

    public class Unary : Expr
    {
        public Token Op;
        public Expr Right;

        public Unary(Token op, Expr right)
        {
            Op = op;
            Right = right;
        }

        public override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }

    public class Ternary : Expr
    {
        public Expr Left;
        public Expr Middle;
        public Expr Right;

        public Ternary(Expr left, Expr middle, Expr right)
        {
            Left = left;
            Middle = middle;
            Right = right;
        }

        public override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }

    public class Variable : Expr
    {
        public Token Name;

        public Variable(Token name)
        {
            Name = name;
        }

        public override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }
}
