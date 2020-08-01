using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpLox
{
    internal static class Lox
    {
        static bool HadError;
        static bool HadRuntimeError;
        static string[] Lines;
        static int[] LineStartPositions;
        static bool supressErrors;
        private static bool replMode;

        static readonly Interpreter Interpreter = new Interpreter();

        static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: csharplox [script]");
                System.Environment.Exit(64);
            }
            else if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunPrompt();
            }
        }

        private static void RunPrompt()
        {
            replMode = true;
            LineStartPositions = new[] { 0 };
            Lines = new string[1];
            while (true)
            {
                Console.Write("> ");
                string line = Console.ReadLine();
                if (string.IsNullOrEmpty(line))
                {
                    break;
                }

                Lines[0] = line;
                Run(line);
                HadError = HadRuntimeError = false;
            }
        }

        static void RunFile(string path)
        {
            path = Path.GetFullPath(path);
            if (!File.Exists(path))
            {
                Console.WriteLine($"File not found at:\n{path}");
                System.Environment.Exit(1);
            }

            Lines = File.ReadAllLines(path);
            LineStartPositions = new int[Lines.Length];
            int total = 0;
            for (int i = 0; i < Lines.Length; i++)
            {
                string line = Lines[i];
                LineStartPositions[i] = total;
                total += line.Length + 1;
            }

            Run(string.Join("\n", Lines));
            if (HadError)
            {
                System.Environment.ExitCode = 65;
            }

            if (HadRuntimeError)
            {
                System.Environment.ExitCode = 70;
            }
        }

        static void Run(string source)
        {
            var scanner = new Scanner(source);
            List<Token> tokens = scanner.ScanTokens();
            var parser = new Parser(tokens);
            if (replMode)
            {
                try
                {
                    supressErrors = true;
                    if (parser.ParseAsExpr() is Expr expr && !HadError)
                    {
                        Interpreter.Interpret(expr);
                        return;
                    }
                }
                finally
                {
                    supressErrors = false;
                }
            }
            List<Stmnt> stmnts = parser.Parse();

            if (HadError)
            {
                return;
            }

            new Resolver(Interpreter).Resolve(stmnts);

            if (HadError)
            {
                return;
            }

            Interpreter.Interpret(stmnts);
        }

        public static void Error(int pos, string msg)
        {
            Report(pos, msg);
        }

        static void Report(int pos, string msg, bool runtime = false)
        {
            if (supressErrors)
            {
                return;
            }
            int line = 0;
            for (int i = 0; i < LineStartPositions.Length; i++)
            {
                if (LineStartPositions[i] > pos)
                {
                    line = i;
                }
            }

            if (line == 0)
            {
                line = Lines.Length;
            }

            int col = pos - LineStartPositions[line - 1];

            Console.WriteLine($"{(runtime ? "Runtime ": "")}Error: {msg}");
            if (!(runtime && replMode))
            {
                Console.WriteLine();
                string preLine = $"    {line} | ";
                Console.WriteLine($"{preLine}{Lines[line - 1]}");
                Console.WriteLine($"{new string(' ', preLine.Length + col)}^-- Here");
            }
            if (runtime)
            {
                HadRuntimeError = true;
            }
            else
            {
                HadError = true;
            }
        }

        public static void Error(Token token, string msg)
        {
            Report(token.Pos, msg);
        }

        public static void RuntimeError(RuntimeError error)
        {
            Report(error.Token.Pos, error.Message, true);
        }
    }
}
