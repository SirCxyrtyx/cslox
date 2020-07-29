namespace CSharpLox
{
    public class Token
    {
        public TokenType Type;
        public string Lexeme;
        public object Literal;
        public int Pos;

        public Token(TokenType type, string lexeme, object literal, int pos)
        {
            Type = type;
            Lexeme = lexeme;
            Literal = literal;
            Pos = pos;
        }

        public override string ToString()
        {
            return Type + " " + Lexeme + " " + Literal;
        }
    }
}