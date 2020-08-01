using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenTool
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: codegen <output directory>");
                Environment.Exit(64);
            }

            string outputDir = Path.GetFullPath(args[0]);
            if (!Directory.Exists(outputDir))
            {
                Console.WriteLine("Output directory does not exist!:");
                Console.WriteLine($"\t{outputDir}");
                Environment.Exit(1);
            }

            const string Expr = "Expr";
            const string Token = "Token";
            const string Stmnt = "Stmnt";
            const string Function = "Function";
            DefineAST(outputDir, Expr, new Dictionary<string, IEnumerable<(string, string)>>
            {
                ["Assign"] = new[] { (Token, "Name"), (Expr, "Value") },
                ["Binary"] = new[] { (Expr, "Left"), (Token, "Op"), (Expr, "Right")},
                ["Call"] = new[] { (Expr, "Callee"), (Token, "Paren"), ($"List<{Expr}>", "Args") },
                ["Get"] = new[] { (Expr, "Obj"), (Token, "Name") },
                ["Grouping"] = new[] { (Expr, "Expression") },
                ["Literal"] = new[] { ("object", "Value") },
                ["Logical"] = new[] { (Expr, "Left"), (Token, "Op"), (Expr, "Right") },
                ["SetExpr"] = new[] { (Expr, "Obj"), (Token, "Name"), (Expr, "Value") },
                ["This"] = new[] { (Token, "Keyword") },
                ["Unary"] = new[] { (Token, "Op"), (Expr, "Right") },
                ["Ternary"] = new[] { (Expr, "Left"), (Expr, "Middle"), (Expr, "Right") },
                ["Variable"] = new[] { (Token, "Name") },
            });
            DefineAST(outputDir, Stmnt, new Dictionary<string, IEnumerable<(string, string)>>
            {
                ["ExpressionStatement"] = new[] { (Expr, "Expression") },
                ["PrintStatement"] = new[] { (Expr, "Expression") },
                ["ReturnStatement"] = new[] { (Token, "Keyword"), (Expr, "Value") },
                ["VarStatement"] = new []{ (Token, "Name"), (Expr, "Initializer")},
                ["ClassDeclaration"] = new[] { (Token, "Name"), ($"List<{Function}>", "Methods") },
                ["BlockStatement"] = new[] { ($"List<{Stmnt}>", "Statements") },
                [Function] = new[] { (Token, "Name"), ($"List<{Token}>", "Parameters"), ($"List<{Stmnt}>", "Body") },
                ["IfStatement"] = new[] { (Expr, "Condition"), (Stmnt, "ThenBranch"), (Stmnt, "ElseBranch") },
                ["WhileStatement"] = new[] { (Expr, "Condition"), (Stmnt, "Body") },
            });
        }

        static void DefineAST(string outputDir, string baseName, Dictionary<string, IEnumerable<(string, string)>> types)
        {
            string path = Path.Combine(outputDir, $"{baseName}.cs");
            using var writer = new CodeWriter(new FileStream(path, FileMode.Create));
            
            writer.WriteLine("using System;");
            writer.WriteLine("using System.Collections.Generic;");
            writer.WriteLine();
            writer.WriteBlock("namespace CSharpLox", () =>
            {
                writer.WriteBlock("public partial interface IVisitor<T>", () =>
                {
                    foreach (string className in types.Keys)
                    {
                        writer.WriteLine($"public T Visit({className} {baseName.ToLower()});");
                    }
                });
                writer.WriteLine();

                //ASTNodes
                writer.WriteBlock($"public abstract class {baseName}", () =>
                {
                    writer.WriteLine("public abstract T AcceptVisitor<T>(IVisitor<T> visitor);");
                });

                foreach ((string typeName, IEnumerable<(string, string)> fields) in types)
                {
                    writer.WriteLine();
                    writer.WriteBlock($"public class {typeName} : {baseName}", () =>
                    {
                        foreach ((string type, string name) in fields)
                        {
                            writer.WriteLine($"public {type} {name};");
                        }
                        writer.WriteLine();
                        writer.WriteBlock($"public {typeName}({string.Join(", ", fields.Select(t => $"{t.Item1} {t.Item2.ToLower()}"))})", () =>
                        {
                            foreach ((_, string name) in fields)
                            {
                                writer.WriteLine($"{name} = {name.ToLower()};");
                            }
                        });
                        writer.WriteLine();
                        writer.WriteLine("public override T AcceptVisitor<T>(IVisitor<T> visitor) => visitor.Visit(this);");
                    });
                }
            });
        }
    }
}
