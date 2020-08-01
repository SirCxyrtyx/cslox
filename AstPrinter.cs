using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpLox
{
    public class AstPrinter : IVisitor<string>
    {
        public static string Print(Expr expr) => expr.AcceptVisitor(new AstPrinter());

        private string Parenthesize(string name, params Expr[] exprs)
        {
            var sb = new StringBuilder();
            sb.Append('(').Append(name);
            foreach (Expr expr in exprs)
            {
                sb.Append(' ');
                sb.Append(expr.AcceptVisitor(this));
            }
            sb.Append(')');
            return sb.ToString();
        }

        public string Visit(Variable expr)
        {
            return expr.Name.Lexeme;
        }

        public string Visit(ClassDeclaration stmnt)
        {
            return $"class {stmnt.Name}\n{{\n{string.Join("\n\n", stmnt.Methods.Select(meth => meth.AcceptVisitor(this)))}\n}}\n";
        }

        public string Visit(BlockStatement stmnt)
        {
            return $"\n{{\n{string.Join("", stmnt.Statements.Select(st => st.AcceptVisitor(this)))}}}\n";
        }

        public string Visit(Function stmnt)
        {
            return $"fun {stmnt.Name.Lexeme} ({string.Join(",", stmnt.Parameters.Select(p => p.Lexeme))})\n{{\n{string.Join("", stmnt.Body.Select(st => st.AcceptVisitor(this)))}}}\n";
        }

        public string Visit(IfStatement stmnt)
        {
            return $"if ({stmnt.Condition.AcceptVisitor(this)}) {stmnt.ThenBranch}{(stmnt.ElseBranch != null ? $" else {stmnt.ElseBranch}" : "")}";
        }

        public string Visit(WhileStatement stmnt)
        {
            return $"While ({stmnt.Condition.AcceptVisitor(this)}) {stmnt.Body}";
        }

        public string Visit(Assign expr)
        {
            return Parenthesize($"= {expr.Name.Lexeme}", expr.Value);
        }

        public string Visit(ExpressionStatement stmnt)
        {
            return $"{stmnt.Expression.AcceptVisitor(this)};\n";
        }

        public string Visit(PrintStatement stmnt)
        {
            return $"print {stmnt.Expression.AcceptVisitor(this)};\n";
        }

        public string Visit(ReturnStatement stmnt)
        {
            return $"{stmnt.Keyword.Lexeme} {stmnt.Value.AcceptVisitor(this)};\n";
        }

        public string Visit(VarStatement stmnt)
        {
            return $"var {stmnt.Name.Lexeme}{(stmnt.Initializer != null ? $" = {stmnt.Initializer}" : "")};\n";
        }

        public string Visit(Binary expr)
        {
            return Parenthesize(expr.Op.Lexeme, expr.Left, expr.Right);
        }

        public string Visit(Call expr)
        {
            return $"{expr.Callee.AcceptVisitor(this)}({string.Join(",", expr.Args.Select(arg => arg.AcceptVisitor(this)))})";
        }

        public string Visit(Get expr)
        {
            return $"(. {expr.Obj.AcceptVisitor(this)} {expr.Name.Lexeme})";
        }

        public string Visit(Grouping expr)
        {
            return Parenthesize("grouping", expr.Expression);
        }

        public string Visit(Literal expr)
        {
            return expr.Value is null ? "nil" : expr.Value.ToString();
        }

        public string Visit(Logical expr)
        {
            return Parenthesize(expr.Op.Lexeme, expr.Left, expr.Right);
        }

        public string Visit(SetExpr expr)
        {
            throw new NotImplementedException();
        }

        public string Visit(This expr)
        {
            return expr.Keyword.Lexeme;
        }

        public string Visit(Unary expr)
        {
            return Parenthesize(expr.Op.Lexeme, expr.Right);
        }

        public string Visit(Ternary expr)
        {
            return Parenthesize("?:", expr.Left, expr.Middle, expr.Right);
        }
    }
}
