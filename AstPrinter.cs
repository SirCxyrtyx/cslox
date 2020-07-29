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

        public string Visit(BlockStatement stmnt)
        {
            return $"\n{{\n{string.Join("", stmnt.Statements.Select(st => st.AcceptVisitor(this)))}}}\n";
        }

        public string Visit(Assign expr)
        {
            return Parenthesize($"= {expr.Name.Lexeme}", expr.Value);
        }

        public string Visit(ExpressionStatement stmnt)
        {
            return $"{stmnt.AcceptVisitor(this)};\n";
        }

        public string Visit(PrintStatement stmnt)
        {
            return $"print {stmnt.AcceptVisitor(this)};\n";
        }

        public string Visit(VarStatement stmnt)
        {
            return $"var {stmnt.Name.Lexeme}{(stmnt.Initializer != null ? $" = {stmnt.Initializer}" : "")};\n";
        }

        public string Visit(Binary expr)
        {
            return Parenthesize(expr.Op.Lexeme, expr.Left, expr.Right);
        }

        public string Visit(Grouping expr)
        {
            return Parenthesize("grouping", expr.Expression);
        }

        public string Visit(Literal expr)
        {
            return expr.Value is null ? "nil" : expr.Value.ToString();
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
