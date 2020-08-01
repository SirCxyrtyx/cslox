using System;
using System.Collections.Generic;
using System.Linq;
using static CSharpLox.TokenType;

namespace CSharpLox
{
    public class Parser
    {
        private const int MAX_FUNCTION_PARAMS = 255;

        private class ParseError : Exception{}

        private readonly List<Token> Tokens;
        private int current;

        public Parser(List<Token> tokens)
        {
            Tokens = tokens;
        }

        public List<Stmnt> Parse()
        {
            Reset();
            var statements = new List<Stmnt>();

            while (!IsAtEnd())
            {
                var statement = Declaration();
                if (statement != null)
                {
                    statements.Add(statement);
                }
            }

            return statements;
        }

        public Expr ParseAsExpr()
        {
            Reset();
            Synchronize();
            if (IsAtEnd() && (Tokens.Count < 2 || Tokens[Tokens.Count - 2].Type != SEMICOLON))
            {
                Reset();
                try
                {
                    return Expression();
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }

        private void Reset() => current = 0;

        Stmnt Declaration()
        {
            try
            {
                if (Match(CLASS))
                {
                    return ClassDeclaration();
                }
                if (Match(FUN))
                {
                    return Function("function");
                }
                if (Match(VAR))
                {
                    return VarDeclaration();
                }

                return Statement();
            }
            catch (ParseError)
            {
                Synchronize();
                return null;
            }
        }

        Stmnt ClassDeclaration()
        {
            Token name = Consume(IDENTIFIER, "Expected class name!");
            Consume(LEFT_BRACE, "Expected '{' before class body!");

            var methods = new List<Function>();
            while (!Check(RIGHT_BRACE) && !IsAtEnd())
            {
                methods.Add(Function("method"));
            }

            Consume(RIGHT_BRACE, "Expected '}' after class body!");
            return new ClassDeclaration(name, methods);
        }

        Function Function(string kind)
        {
            Token name = Consume(IDENTIFIER, $"Expected {kind} name!");
            Consume(LEFT_PAREN, $"Expected '(' after {kind} name!");
            var parameters = new List<Token>();
            if (!Check(RIGHT_PAREN))
            {
                do
                {
                    if (parameters.Count >= MAX_FUNCTION_PARAMS)
                    {
                        Error(Peek(), $"Cannot have more than {MAX_FUNCTION_PARAMS} parameters!");
                    }
                    parameters.Add(Consume(IDENTIFIER, "Expected parameter name!"));
                } while (Match(COMMA));
            }
            Consume(RIGHT_PAREN, "Expected ')' after parameters!");

            Consume(LEFT_BRACE, $"Expected '{{' before {kind} body!");
            return new Function(name, parameters, Block());
        }

        private Stmnt VarDeclaration()
        {
            Token name = Consume(IDENTIFIER, "Expected variable name!");

            Expr initializer = Match(EQUAL) ? Expression() : null;

            Consume(SEMICOLON, "Expected ';' after variable declaration!");
            return new VarStatement(name, initializer);
        }

        Stmnt Statement()
        {
            if (Match(FOR))
            {
                return ForStatement();
            }
            if (Match(IF))
            {
                return IfStatement();
            }
            if (Match(PRINT))
            {
                return PrintStatement();
            }

            if (Match(RETURN))
            {
                return ReturnStatement();
            }
            if (Match(WHILE))
            {
                return WhileStatement();
            }
            if (Match(LEFT_BRACE))
            {
                return new BlockStatement(Block());
            }

            return ExpressionStatement();
        }

        Stmnt ReturnStatement()
        {
            Token keyword = Prev();
            Expr value = null;
            if (!Check(SEMICOLON))
            {
                value = Expression();
            }

            Consume(SEMICOLON, "Expected ';' after return value!");
            return new ReturnStatement(keyword, value);
        }

        private Stmnt ForStatement()
        {
            Consume(LEFT_PAREN, "Expected '(' after 'for'!");

            Stmnt init;
            if (Match(SEMICOLON))
            {
                init = null;
            }
            else if (Match(VAR))
            {
                init = VarDeclaration();
            }
            else
            {
                init = ExpressionStatement();
            }

            Expr condition = Check(SEMICOLON) ? new Literal(true) : Expression();
            Consume(SEMICOLON, "Expected ';' after loop condition!");

            Expr increment = Check(RIGHT_PAREN) ? null : Expression();
            Consume(RIGHT_PAREN, "Expected ')' after for clauses!");

            Stmnt body = Statement();

            if (increment != null)
            {
                body = new BlockStatement(new List<Stmnt> { body, new ExpressionStatement(increment) });
            }
            body = new WhileStatement(condition, body);
            if (init != null)
            {
                body = new BlockStatement(new List<Stmnt> { init, body });
            }

            return body;
        }

        Stmnt WhileStatement()
        {
            Consume(LEFT_PAREN, "Expected '(' after 'while'!");
            Expr condition = Expression();
            Consume(RIGHT_PAREN, "Expected ')' after while condition!");
            Stmnt body = Statement();
            return new WhileStatement(condition, body);
        }

        Stmnt IfStatement()
        {
            Consume(LEFT_PAREN, "Expected '(' after 'if'!");
            Expr condition = Expression();
            Consume(RIGHT_PAREN, "Expected ')' after if condition!");

            Stmnt thenBranch = Statement();
            Stmnt elseBranch = null;
            if (Match(ELSE))
            {
                elseBranch = Statement();
            }
            return new IfStatement(condition, thenBranch, elseBranch);
        }

        private List<Stmnt> Block()
        {
            var statements = new List<Stmnt>();

            while (!Check(RIGHT_BRACE) && !IsAtEnd())
            {
                statements.Add(Declaration());
            }

            Consume(RIGHT_BRACE, "Expected '}' after block!");
            return statements;
        }

        Stmnt PrintStatement()
        {
            Expr value = Expression();
            Consume(SEMICOLON, "Expected ';' after value!");
            return new PrintStatement(value);
        }

        Stmnt ExpressionStatement()
        {
            Expr expr = Expression();
            Consume(SEMICOLON, "Expected ';' after expression!");
            return new ExpressionStatement(expr);
        }

        Expr Expression() => Assignment();

        Expr Assignment()
        {
            Expr expr = Ternary();

            if (Match(EQUAL))
            {
                Token equals = Prev();
                Expr value = Expression();

                if (expr is Variable variable)
                {
                    return new Assign(variable.Name, value);
                }
                else if (expr is Get get)
                {
                    return new SetExpr(get.Obj, get.Name, value);
                }

                Error(equals, "Invalid assignment target!");
            }

            return expr;
        }

        Expr Ternary()
        {
            Expr expr = Or();

            if (Match(QUESTION_MARK))
            {
                Expr middle = Or();
                Consume(COLON, "Expected ':' in ternary expression");
                Expr right = Or();
                expr = new Ternary(expr, middle, right);
            }

            return expr;
        }

        Expr Or()
        {
            Expr expr = And();

            while (Match(OR))
            {
                Token op = Prev();
                Expr right = And();
                return new Logical(expr, op, right);
            }

            return expr;
        }

        Expr And()
        {
            Expr expr = Equality();

            while (Match(AND))
            {
                Token op = Prev();
                Expr right = Equality();
                return new Logical(expr, op, right);
            }

            return expr;
        }

        Expr Equality() => BinaryOperator(Comparison, BANG_EQUAL, EQUAL_EQUAL);
        Expr Comparison() => BinaryOperator(Addition, GREATER, GREATER_EQUAL, LESS, LESS_EQUAL);
        Expr Addition() => BinaryOperator(Multiplication, MINUS, PLUS);
        Expr Multiplication() => BinaryOperator(Unary, SLASH, STAR);

        Expr Unary()
        {
            if (Match(BANG, MINUS))
            {
                Token op = Prev();
                Expr right = Unary();
                return new Unary(op, right);
            }

            return Call();
        }

        Expr Call()
        {
            Expr expr = Primary();

            while (true)
            {
                if (Match(LEFT_PAREN))
                {
                    expr = FinishCall(expr);
                }
                else if (Match(DOT))
                {
                    expr = new Get(expr, Consume(IDENTIFIER, "Expected property name after '.'!"));
                }
                else
                {
                    break;
                }
            }

            return expr;
        }

        private Expr FinishCall(Expr callee)
        {
            var args = new List<Expr>();
            if (!Check(RIGHT_PAREN))
            {
                do
                {
                    if (args.Count >= MAX_FUNCTION_PARAMS)
                    {
                        Error(Peek(), $"Cannot have more than {MAX_FUNCTION_PARAMS} arguments!");
                    }
                    args.Add(Expression());
                } while (Match(COMMA));
            }

            Token paren = Consume(RIGHT_PAREN, "Expected ')' after arguments!");

            return new Call(callee, paren, args);
        }

        private Expr Primary()
        {
            if (Match(FALSE))
            {
                return new Literal(false);
            }
            if (Match(TRUE))
            {
                return new Literal(true);
            }
            if (Match(NIL))
            {
                return new Literal(null);
            }
            if (Match(NUMBER, STRING))
            {
                return new Literal(Prev().Literal);
            }

            if (Match(IDENTIFIER))
            {
                return new This(Prev());
            }
            if (Match(IDENTIFIER))
            {
                return new Variable(Prev());
            }
            if (Match(LEFT_PAREN))
            {
                var expr = Expression();
                Consume(RIGHT_PAREN, "Expected ')' after expression.");
                return new Grouping(expr);
            }
            throw Error(Peek(), "Expected expression");
        }

        private Token Consume(TokenType type, string msg)
        {
            if (Check(type))
            {
                return Advance();
            }

            throw Error(Peek(), msg);
        }

        private static ParseError Error(Token token, string msg)
        {
            Lox.Error(token, msg);
            return new ParseError();
        }
        private void Synchronize()
        {
            Advance();

            while (!IsAtEnd())
            {
                if (Prev().Type == SEMICOLON) return;

                switch (Peek().Type)
                {
                    case CLASS:
                    case FUN:
                    case VAR:
                    case FOR:
                    case IF:
                    case WHILE:
                    case PRINT:
                    case RETURN:
                        return;
                }

                Advance();
            }
        }

        Expr BinaryOperator(Func<Expr> next, params TokenType[] types)
        {
            Expr expr = next();

            while (Match(types))
            {
                Token op = Prev();
                Expr right = next();
                expr = new Binary(expr, op, right);
            }

            return expr;
        }

        bool Match(params TokenType[] types)
        {
            if (types.Any(Check))
            {
                Advance();
                return true;
            }

            return false;
        }

        bool Check(TokenType type) => !IsAtEnd() && Peek().Type == type;

        Token Peek() => Tokens[current];

        Token Prev() => Tokens[current - 1];

        bool IsAtEnd() => Peek().Type == EOF;

        Token Advance()
        {
            if (!IsAtEnd())
            {
                current++;
            }

            return Prev();
        }
    }
}
