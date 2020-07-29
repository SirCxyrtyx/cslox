using System.Collections.Generic;
using static CSharpLox.TokenType;

namespace CSharpLox
{
    public class Scanner
    {
        private static Dictionary<string, TokenType> Keywords = new Dictionary<string, TokenType>
        {
            {"and",    AND},
            {"class",  CLASS},
            {"else",   ELSE},
            {"false",  FALSE},
            {"for",    FOR},
            {"fun",    FUN},
            {"if",     IF},
            {"nil",    NIL},
            {"or",     OR},
            {"print",  PRINT},
            {"return", RETURN},
            {"super",  SUPER},
            {"this",   THIS},
            {"true",   TRUE},
            {"var",    VAR},
            {"while",  WHILE}
        };

        private readonly string Source;
        readonly List<Token> Tokens = new List<Token>();

        private int start = 0;
        private int current = 0;
        private int line = 1;

        public Scanner(string source)
        {
            Source = source;
        }

        public List<Token> ScanTokens()
        {
            while (!IsAtEnd())
            {
                start = current;
                ScanToken();
            }

            Tokens.Add(new Token(EOF, "", null, current));
            return Tokens;
        }

        private void ScanToken()
        {
            char c = Advance();
            switch (c)
            {
                case '(': AddToken(LEFT_PAREN); break;
                case ')': AddToken(RIGHT_PAREN); break;
                case '{': AddToken(LEFT_BRACE); break;
                case '}': AddToken(RIGHT_BRACE); break;
                case ',': AddToken(COMMA); break;
                case '.': AddToken(DOT); break;
                case '-': AddToken(MINUS); break;
                case '+': AddToken(PLUS); break;
                case ';': AddToken(SEMICOLON); break;
                case '*': AddToken(STAR); break;
                case ':': AddToken(COLON); break;
                case '?': AddToken(QUESTION_MARK); break;
                case '!': AddToken(Match('=') ? BANG_EQUAL : BANG); break;
                case '=': AddToken(Match('=') ? EQUAL_EQUAL : EQUAL); break;
                case '<': AddToken(Match('=') ? LESS_EQUAL : LESS); break;
                case '>': AddToken(Match('=') ? GREATER_EQUAL : GREATER); break;
                case '/':
                    if (Match('/'))
                    {
                        while (Peek() != '\n' && !IsAtEnd())
                        {
                            Advance();
                        }
                    }
                    else
                    {
                        AddToken(SLASH);
                    }
                    break;
                case ' ':
                case '\r':
                case '\t':
                    // Ignore whitespace.
                    break;
                case '\n':
                    line++;
                    break;
                case '"': LexString(); break;

                default:
                    if (char.IsDigit(c))
                    {
                        LexNumber();
                    }
                    else if (IsAlpha(c))
                    {
                        LexIdentifier();
                    }
                    else
                    {
                        Lox.Error(current, "Unexpected character");
                    }
                    break;
            }
        }

        void AddToken(TokenType type, object literal = null)
        {
            Tokens.Add(new Token(type, Source.Slice(start, current), literal, start));
        }

        bool Match(char expected)
        {
            if (IsAtEnd() || Source[current] != expected)
            {
                return false;
            }
            current++;
            return true;
        }

        private void LexString () {
            while (Peek() != '"' && !IsAtEnd()) {
                if (Peek() == '\n')
                {
                    line++;
                }
                Advance();
            }

            // Unterminated string.
            if (IsAtEnd()) {
                Lox.Error(current, "Unterminated string.");
                return;
            }

            // The closing ".
            Advance();

            // Trim the surrounding quotes.
            string value = Source.Slice(start + 1, current - 1);
            AddToken(STRING, value);
        }

        void LexNumber()
        {
            while (char.IsDigit(Peek()))
            {
                Advance();
            }

            if (Peek() == '.' && char.IsDigit(PeekNext()))
            {
                do
                {
                    Advance();
                } while (char.IsDigit(Peek()));
            }

            AddToken(NUMBER, double.Parse(Source.Slice(start, current)));
        }

        void LexIdentifier()
        {
            while (IsAlphaNumeric(Peek()))
            {
                Advance();
            }

            string text = Source.Slice(start, current);
            if (Keywords.TryGetValue(text, out TokenType type))
            {
                AddToken(type);
            }
            else
            {
                AddToken(IDENTIFIER);
            }
        }

        char Peek() => IsAtEnd() ? '\0' : Source[current];
        char PeekNext() => current + 1 >= Source.Length ? '\0' : Source[current + 1];

        private char Advance() => IsAtEnd() ? '\0' : Source[current++];

        private bool IsAtEnd()
        {
            return current >= Source.Length;
        }

        static bool IsAlpha(char c) => char.IsLetter(c) || c == '_';

        static bool IsAlphaNumeric(char c) => IsAlpha(c) || char.IsDigit(c);
    }
}