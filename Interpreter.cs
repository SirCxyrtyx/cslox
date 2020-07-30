using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CSharpLox.TokenType;

namespace CSharpLox
{
    public class Interpreter : IVisitor<object>
    {
        readonly Environment Globals = new Environment();
        Environment Env;
        readonly Dictionary<Expr, int> locals = new Dictionary<Expr, int>();

        public Interpreter()
        {
            Env = Globals;
            Globals.Define("clock", new NativeFunc((interpreter, args) => (double)(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond),
                                                            () => 0));

        }

        public void Interpret(List<Stmnt> statements)
        {
            try
            {
                foreach (Stmnt statement in statements)
                {
                    Execute(statement);
                }
            }
            catch (RuntimeError error)
            {
                Lox.RuntimeError(error);
            }
        }

        public void Interpret(Expr expression)
        {
            try
            {
                Console.WriteLine(Stringify(Eval(expression)));
            }
            catch (RuntimeError error)
            {
                Lox.RuntimeError(error);
            }
        }

        static string Stringify(object obj) => obj?.ToString() ?? "nil";

        public object Visit(Variable expr) => LookupVariable(expr.Name, expr);

        private object LookupVariable(Token name, Expr expr)
        {
            if (locals.TryGetValue(expr, out int distance))
            {
                return Env.GetAt(distance, name.Lexeme);
            }

            return Globals.Get(name);
        }

        public object Visit(BlockStatement stmnt)
        {
            ExecuteBlock(stmnt.Statements, new Environment(Env));
            return null;
        }

        public object Visit(Function stmnt)
        {
            Env.Define(stmnt.Name.Lexeme, new LoxFunction(stmnt, Env));
            return null;
        }

        public object Visit(IfStatement stmnt)
        {
            if (IsTruthy(Eval(stmnt.Condition)))
            {
                Execute(stmnt.ThenBranch);
            }
            else if (stmnt.ElseBranch != null)
            {
                Execute(stmnt.ElseBranch);
            }

            return null;
        }

        public object Visit(WhileStatement stmnt)
        {
            while (IsTruthy(Eval(stmnt.Condition)))
            {
                Execute(stmnt.Body);
            }

            return null;
        }

        void Execute(Stmnt statement) => statement.AcceptVisitor(this);

        public void ExecuteBlock(List<Stmnt> statements, Environment environment)
        {
            Environment previous = Env;
            try
            {
                Env = environment;

                foreach (Stmnt statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                Env = previous;
            }
        }

        public object Visit(Assign expr)
        {
            object value = Eval(expr.Value);

            if (locals.TryGetValue(expr, out int distance))
            {
                Env.AssignAt(distance, expr.Name, value);
            }
            else
            {
                Globals.Assign(expr.Name, value);
            }
            return value;
        }

        public object Visit(ExpressionStatement stmnt)
        {
            Eval(stmnt.Expression);
            return null;
        }

        public object Visit(PrintStatement stmnt)
        {
            Console.WriteLine(Stringify(Eval(stmnt.Expression)));
            return null;
        }

        public object Visit(ReturnStatement stmnt) => throw new Return(stmnt.Value != null ? Eval(stmnt.Value) : null);

        public object Visit(VarStatement stmnt)
        {
            object value = null;
            if (stmnt.Initializer != null)
            {
                value = Eval(stmnt.Initializer);
            }
            Env.Define(stmnt.Name.Lexeme, value);
            return null;
        }

        public object Visit(Binary expr)
        {
            var left = Eval(expr.Left);
            var right = Eval(expr.Right);

            switch (expr.Op.Type)
            {
                case GREATER:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left > (double)right;
                case GREATER_EQUAL:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left >= (double)right;
                case LESS:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left < (double)right;
                case LESS_EQUAL:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left <= (double)right;
                case STAR:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left * (double)right;
                case SLASH:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left / (double)right;
                case MINUS:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left - (double)right;
                case PLUS:
                    return left switch
                    {
                        double ld when right is double rd => ld + rd,
                        string ls => ls + Stringify(right),
                        _ when right is string rs => Stringify(left) + rs,
                        _ => throw new RuntimeError(expr.Op, "Operands must either be two numbers, or one must be a string.")
                    };
                case BANG_EQUAL:
                    return !IsEqual(left, right);
                case EQUAL_EQUAL:
                    return IsEqual(left, right);
                default:
                    throw new Exception();
            }
        }

        public object Visit(Call expr)
        {
            object callee = Eval(expr.Callee);

            List<object> args = expr.Args.Select(Eval).ToList();

            if (callee is ICallable function)
            {
                if (args.Count != function.Arity())
                {
                    throw new RuntimeError(expr.Paren, $"Expected {function.Arity()} arguments but got {args.Count}!");
                }
                return function.Call(this, args);
            }
            throw new RuntimeError(expr.Paren, "Can only call functions and classes!");
        }

        public object Visit(Grouping expr) => Eval(expr.Expression);

        public object Visit(Literal expr) => expr.Value;
        public object Visit(Logical expr)
        {
            object left = Eval(expr.Left);

            return expr.Op.Type switch
            {
                OR when IsTruthy(left) => left,
                AND when !IsTruthy(left) => left,
                _ => Eval(expr.Right)
            };
        }

        public object Visit(Unary expr)
        {
            var right = Eval(expr.Right);

            switch (expr.Op.Type)
            {
                case BANG:
                    return !IsTruthy(right);
                case MINUS:
                    CheckNumberOperand(expr.Op, right);
                    return -(double)right;
                default:
                    return null;
            }
        }

        public object Visit(Ternary expr) => Eval(IsTruthy(Eval(expr.Left)) ? expr.Middle : expr.Right);

        private static bool IsEqual(object a, object b) => a?.Equals(b) ?? b is null;

        private static bool IsTruthy(object obj) =>
            obj switch
            {
                null => false,
                bool b => b,
                _ => true
            };

        object Eval(Expr expr) => expr.AcceptVisitor(this);

        static void CheckNumberOperand(Token op, object operand)
        {
            if (operand is double)
            {
                return;
            }
            throw new RuntimeError(op, "Operand must be a number!");
        }

        static void CheckNumberOperands(Token op, object leftOperand, object rightOperand)
        {
            if (leftOperand is double && rightOperand is double)
            {
                return;
            }
            throw new RuntimeError(op, "Operands must be a number!");
        }

        public void Resolve(Expr expr, int depth) => locals[expr] = depth;
    }
}
