using System;
using System.Collections.Generic;
using System.Linq;
using static CSharpLox.TokenType;

namespace CSharpLox
{
    public class Parser
    {
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
            if (IsAtEnd())
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

        private Stmnt VarDeclaration()
        {
            Token name = Consume(IDENTIFIER, "Expected variable name!");

            Expr initializer = Match(EQUAL) ? Expression() : null;

            Consume(SEMICOLON, "Expected ';' after variable declaration!");
            return new VarStatement(name, initializer);
        }

        Stmnt Statement()
        {
            if (Match(PRINT))
            {
                return PrintStatement();
            }

            if (Match(LEFT_BRACE))
            {
                return new BlockStatement(Block());
            }

            return ExpressionStatement();
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

                Error(equals, "Invalid assignment target!");
            }

            return expr;
        }

        Expr Ternary()
        {
            Expr expr = Equality();

            if (Match(QUESTION_MARK))
            {
                Expr middle = Expression();
                Consume(COLON, "Expected ':' in ternary expression");
                Expr right = Expression();
                expr = new Ternary(expr, middle, right);
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

            return Primary();
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
