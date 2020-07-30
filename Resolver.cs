using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpLox
{
    public class Resolver : IVisitor<object>
    {
        enum FunctionType
        {
            NONE,
            FUNCTION
        }

        private readonly Interpreter interpreter;
        private readonly Stack<Dictionary<string, bool>> Scopes = new Stack<Dictionary<string, bool>>();
        private FunctionType currentFunction = FunctionType.NONE;

        public Resolver(Interpreter interpreter)
        {
            this.interpreter = interpreter;
        }

        void BeginScope() => Scopes.Push(new Dictionary<string, bool>());

        void EndScope() => Scopes.Pop();

        void Declare(Token name)
        {
            if (Scopes.Any())
            {
                Dictionary<string, bool> scope = Scopes.Peek();
                if (scope.ContainsKey(name.Lexeme))
                {
                    Lox.Error(name, "Variable with this name already declared in this scope!");
                }
                scope[name.Lexeme] = false;
            }
        }

        void Define(Token name)
        {
            if (Scopes.Any())
            {
                Scopes.Peek()[name.Lexeme] = true;
            }
        }

        void Resolve(Stmnt stmnt) => stmnt.AcceptVisitor(this);

        void Resolve(Expr expr) => expr.AcceptVisitor(this);

        public void Resolve(IEnumerable<Stmnt> stmnts)
        {
            foreach (Stmnt stmnt in stmnts)
            {
                Resolve(stmnt);
            }
        }

        public object Visit(ExpressionStatement stmnt)
        {
            Resolve(stmnt.Expression);
            return null;
        }

        public object Visit(PrintStatement stmnt)
        {
            Resolve(stmnt.Expression);
            return null;
        }

        public object Visit(ReturnStatement stmnt)
        {
            if (currentFunction == FunctionType.NONE)
            {
                Lox.Error(stmnt.Keyword, "Cannot return from top-level code!");
            }
            if (stmnt.Value != null)
            {
                Resolve(stmnt.Value);
            }
            return null;
        }

        public object Visit(VarStatement stmnt)
        {
            Declare(stmnt.Name);
            if (stmnt.Initializer != null)
            {
                Resolve(stmnt.Initializer);
            }
            Define(stmnt.Name);
            return null;
        }

        public object Visit(BlockStatement stmnt)
        {
            BeginScope();
            Resolve(stmnt.Statements);
            EndScope();
            return null;
        }

        public object Visit(Function stmnt)
        {
            Declare(stmnt.Name);
            Define(stmnt.Name);

            ResolveFunction(stmnt, FunctionType.FUNCTION);
            return null;
        }

        private void ResolveFunction(Function function, FunctionType funcType)
        {
            FunctionType enclosing = currentFunction;
            currentFunction = funcType;
            BeginScope();
            foreach (Token param in function.Parameters)
            {
                Declare(param);
                Define(param);
            }
            Resolve(function.Body);
            EndScope();
            currentFunction = enclosing;
        }

        public object Visit(IfStatement stmnt)
        {
            Resolve(stmnt.Condition);
            Resolve(stmnt.ThenBranch);
            if (stmnt.ElseBranch != null)
            {
                Resolve(stmnt.ElseBranch);
            }

            return null;
        }

        public object Visit(WhileStatement stmnt)
        {
            Resolve(stmnt.Condition);
            Resolve(stmnt.Body);
            return null;
        }

        public object Visit(Assign expr)
        {
            Resolve(expr.Value);
            ResolveLocal(expr, expr.Name);
            return null;
        }

        public object Visit(Binary expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);
            return null;
        }

        public object Visit(Call expr)
        {
            Resolve(expr.Callee);
            foreach (Expr arg in expr.Args)
            {
                Resolve(arg);
            }
            return null;
        }

        public object Visit(Grouping expr)
        {
            Resolve(expr.Expression);
            return null;
        }

        public object Visit(Literal expr)
        {
            return null;
        }

        public object Visit(Logical expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);
            return null;
        }

        public object Visit(Unary expr)
        {
            Resolve(expr.Right);
            return null;
        }

        public object Visit(Ternary expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Middle);
            Resolve(expr.Right);
            return null;
        }

        public object Visit(Variable expr)
        {
            if (Scopes.Any() && Scopes.Peek().TryGetValue(expr.Name.Lexeme, out bool isDefined) && !isDefined)
            {
                Lox.Error(expr.Name, "Cannot read local variable in its own initializer.");
            }

            ResolveLocal(expr, expr.Name);
            return null;
        }

        void ResolveLocal(Expr expr, Token name)
        {
            for (int i = Scopes.Count - 1; i >= 0; i--)
            {
                if (Scopes.ElementAt(i).ContainsKey(name.Lexeme))
                {
                    interpreter.Resolve(expr, Scopes.Count - 1 - i);
                    return;
                }
            }
        }
    }
}
