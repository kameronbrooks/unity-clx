using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

namespace CLX
{
    public class Token
    {
        public enum TokenType
        {
            None,
            Identifier,
            StringLiteral,
            Numeric,
            True,
            False,
            If,
            Else,
            For,
            Foreach,
            While,
            Null,
            Assign,
            Equals,
            NotEquals,
            GreaterThan,
            GreaterThanOrEqual,
            LessThan,
            LessThanOrEqual,
            Add,
            Subtract,
            Multiply,
            Divide,
            SquareBracketOpen,
            SquareBracketClose,
            ParenthOpen,
            ParenthClose,
            CurlyBracketOpen,
            CurlyBracketClose,
            Dot,
            Comma,
            Colon,
            ArrowRight,
            ArrowLeft,
            Negate,
            And,
            Or,
            Not,
            Modulo,
            BitwiseOr,
            BitwiseAnd,
            BitwiseXOr,
            Factorial,
            EOS,
            Interp,
            Power,
            At,
            HashTag,
            Dollar,
            HexLiteral,
            IsSubsetOf,
            IsNotSubsetOf
        }
        public TokenType type;
        public string text;
        public int lineNumber;

        public Token(string text, TokenType type)
        {
            this.text = text;
            this.type = type;
        }
    }
    public class Lexer
    {
        private int _index;
        private string _input;
        private int _lineNumber;

        /// <summary>
        /// Is lexer at end of text
        /// </summary>
        private bool isEOF
        {
            get
            {
                return _index >= _input.Length;
            }
        }

        private char previous
        {
            get
            {
                return _input[_index - 1];
            }
        }

        /// <summary>
        /// Get list of tokens from string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public Token[] Tokenize(string input)
        {
            List<Token> tokens = new List<Token>();
            _index = 0;
            _input = input;


            while (!isEOF)
            {
                char cur = Peek();
                // Literals
                if (StepIf('\"'))
                {
                    tokens.Add(GetStringLiteral('\"'));
                }
                else if (StepIf('\''))
                {
                    tokens.Add(GetStringLiteral('\''));
                }
                else if (Char.IsDigit(cur))
                {
                    bool isHex = false;
                    // Hex Values
                    if (cur == '0')
                    {
                        if (_input.Length > _index + 1)
                        {
                            if (_input[_index + 1] == 'x')
                            {
                                isHex = true;
                                tokens.Add(GetHex());
                            }
                        }
                    }

                    // Normal Numbers
                    if (!isHex)
                    {
                        tokens.Add(GetNumber());
                    }
                }
                else if (Char.IsLetterOrDigit(cur) || cur == '_')
                {
                    string identifier = GetString();
                    Token token = GetKeywordToken(identifier);
                    if (token != null)
                    {
                        tokens.Add(token);
                    }
                    else
                    {
                        tokens.Add(CreateToken(Token.TokenType.Identifier, identifier));
                    }
                }
                else if (cur == '\n')
                {
                    ++_lineNumber;
                    Step();
                }
                else
                {
                    // Try to find operator
                    Token op = GetOperator();
                    if (op != null)
                    {
                        tokens.Add(op);
                    }
                }
            }


            return tokens.ToArray();
        }

        /// <summary>
        /// Use next character if it equals the specified character
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private char Consume(char c)
        {
            if (!isEOF && Peek() == c) Step();
            return c;
        }

        private Token CreateToken(Token.TokenType type, string text)
        {
            Token token = new Token(text, type);
            token.lineNumber = _lineNumber;
            return token;
        }

        private Token CreateToken(Token.TokenType type, params char[] chars)
        {
            Token token = new Token(new string(chars), type);
            token.lineNumber = _lineNumber;
            return token;
        }

        /// <summary>
        /// Get a hex literal from the string
        /// </summary>
        /// <returns></returns>
        private Token GetHex()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(Step());
            builder.Append(Step());
            while (!isEOF && (Char.IsDigit(Peek()) || "ABCDEFabcdef".IndexOf(Peek()) >= 0))
            {
                builder.Append(Step());
            }

            return new Token(builder.ToString(), Token.TokenType.HexLiteral);
        }
        /// <summary>
        /// Get a keyword from the string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private Token GetKeywordToken(string text)
        {

            switch (text)
            {
                case "true":
                    return CreateToken(Token.TokenType.True, text);
                case "false":
                    return CreateToken(Token.TokenType.False, text);
                case "null":
                    return CreateToken(Token.TokenType.Null, text);
                case "if":
                    return CreateToken(Token.TokenType.If, text);
                case "else":
                    return CreateToken(Token.TokenType.Else, text);
                case "for":
                    return CreateToken(Token.TokenType.For, text);
                case "while":
                    return CreateToken(Token.TokenType.While, text);
                default:
                    return null;
            }
        }
        /// <summary>
        /// Get a number from the string
        /// </summary>
        /// <returns></returns>
        private Token GetNumber()
        {
            StringBuilder builder = new StringBuilder();
            int startIndex = _index;
            int radixIndex = -1;
            while (!isEOF && (Char.IsDigit(Peek()) || Peek() == '.'))
            {
                char cur = Step();
                if (cur == '.')
                {
                    if (Peek() == '.' || (radixIndex > -1 && radixIndex < _index - 1))
                    {
                        StepBack();
                        break;
                    }
                    else
                    {
                        radixIndex = _index;
                    }
                }
            }

            return new Token(_input.Substring(startIndex, _index - startIndex), Token.TokenType.Numeric);
        }
        /// <summary>
        /// Get an operator from the string
        /// </summary>
        /// <returns></returns>
        private Token GetOperator()
        {
            char cur = Step();
            int tmpIndex = 0;
            switch (cur)
            {
                case '{':
                    return CreateToken(Token.TokenType.CurlyBracketOpen, cur);

                case '}':
                    return CreateToken(Token.TokenType.CurlyBracketClose, cur);

                case '[':
                    return CreateToken(Token.TokenType.SquareBracketOpen, cur);

                case ']':
                    return CreateToken(Token.TokenType.SquareBracketClose, cur);

                case '(':

                    return CreateToken(Token.TokenType.ParenthOpen, cur);

                case ')':
                    return CreateToken(Token.TokenType.ParenthClose, cur);

                case '.':
                    if (Peek() == '.')
                    {
                        return CreateToken(Token.TokenType.Interp, cur, Step());
                    }
                    return CreateToken(Token.TokenType.Dot, cur);

                case ',':
                    return CreateToken(Token.TokenType.Comma, cur);

                case '%':
                    return CreateToken(Token.TokenType.Modulo, cur);

                case ';':
                    return CreateToken(Token.TokenType.EOS, cur);

                case '-':
                    if (Peek() == '=')
                    {
                        //return CreateToken(Token.TokenType.AssignSubtract, cur, Step());
                        throw new Exception("This operation is not supported");
                    }
                    if (Peek() == '-')
                    {
                        //return CreateToken(Token.TokenType.Decrement, cur, Step());
                        throw new Exception("This operation is not supported");
                    }
                    if (Peek() == '>')
                    {
                        return CreateToken(Token.TokenType.ArrowRight, cur, Step());
                    }
                    else
                    {
                        return CreateToken(Token.TokenType.Subtract, cur);
                    }
                case '+':
                    if (Peek() == '=')
                    {
                        //return CreateToken(Token.TokenType.AssignAdd, cur, Step());
                        throw new Exception("This operation is not supported");
                    }
                    if (Peek() == '+')
                    {
                        throw new Exception("This operation is not supported");
                        //return CreateToken(Token.TokenType.Increment, cur, Step());
                    }
                    else
                    {
                        return CreateToken(Token.TokenType.Add, cur);
                    }
                case '*':
                    if (Peek() == '=')
                    {
                        //return CreateToken(Token.TokenType.AssignMult, cur, Step());
                        throw new Exception("This operation is not supported");
                    }
                    if (Peek() == '*')
                    {
                        return CreateToken(Token.TokenType.Power, cur, Step());
                    }
                    else
                    {
                        return CreateToken(Token.TokenType.Multiply, cur);
                    }
                case '/':
                    if (Peek() == '/')
                    {
                        SkipComment();
                        return null;
                    }
                    if (Peek() == '=')
                    {
                        //return CreateToken(Token.TokenType.AssignDiv, cur, Step());
                        throw new Exception("This operation is not supported");
                    }
                    else
                    {
                        return CreateToken(Token.TokenType.Divide, cur);
                    }
                case '>':
                    if (Peek() == '=')
                    {
                        return CreateToken(Token.TokenType.GreaterThanOrEqual, cur, Step());
                    }
                    else if (Peek() == '>')
                    {
                        throw new Exception("This operation is not supported");
                        //return CreateToken(Token.TokenType.BitwiseRightShift, cur, Step());
                    }
                    else
                    {
                        return CreateToken(Token.TokenType.GreaterThan, cur);
                    }
                case '<':
                    if (Peek() == '=')
                    {
                        return CreateToken(Token.TokenType.LessThanOrEqual, cur, Step());
                    }
                    else if (Peek() == '>')
                    {
                        throw new Exception("This operation is not supported");
                    }
                    else if (Peek() == '<')
                    {
                        throw new Exception("This operation is not supported");
                        //return CreateToken(Token.TokenType.BitwiseLeftShift, cur, Step());
                    }
                    if (Peek() == '-')
                    {
                        return CreateToken(Token.TokenType.ArrowLeft, cur, Step());
                    }
                    else
                    {
                        return CreateToken(Token.TokenType.LessThan, cur);
                    }
                case '^':
                    return CreateToken(Token.TokenType.BitwiseXOr, cur);

                case ':':
                    if (Peek() == ':')
                    {
                        return CreateToken(Token.TokenType.IsSubsetOf, cur, Step());
                    }
                    if (Peek() == '=')
                    {
                        return CreateToken(Token.TokenType.Assign, cur, Step());
                    }
                    return CreateToken(Token.TokenType.Colon);

                case '|':
                    if (Peek() == '|')
                    {
                        return CreateToken(Token.TokenType.Or, cur, Step());
                    }
                    else
                    {
                        return CreateToken(Token.TokenType.BitwiseOr, cur);
                    }
                case '&':
                    if (Peek() == '&')
                    {
                        return CreateToken(Token.TokenType.And, cur, Step());
                    }
                    else
                    {
                        return CreateToken(Token.TokenType.BitwiseAnd, cur);
                    }
                case '!':
                    if (Peek() == '=')
                    {
                        return CreateToken(Token.TokenType.NotEquals, cur, Step());
                    }
                    else if (Peek() == '!')
                    {
                        return CreateToken(Token.TokenType.Factorial, cur, Step());
                    }
                    else if (StepIf(':'))
                    {
                        if (StepIf(':'))
                        {
                            return CreateToken(Token.TokenType.IsNotSubsetOf, "!::");
                        }
                    }
                    return CreateToken(Token.TokenType.Not, cur);

                case '=':
                    if (Peek() == '=')
                    {
                        return CreateToken(Token.TokenType.Equals, cur, Step());
                    }
                    else
                    {
                        return CreateToken(Token.TokenType.Equals, cur);
                    }
                case '?':
                    if (Peek() == '.')
                    {
                        throw new Exception("This operation is not supported");
                        //return CreateToken(Token.TokenType.IfDot, cur, Step());
                    }
                    else
                    {
                        throw new Exception("This operation is not supported");
                        //return CreateToken(Token.TokenType.Help, cur, cur);
                    }
                default:
                    return null;
            }
        }

        private string GetString()
        {
            StringBuilder builder = new StringBuilder();
            // If characters are alphanumeric
            while (!isEOF && (Char.IsLetterOrDigit(Peek()) || Peek() == '_'))
            {
                builder.Append(Step());
            }
            return builder.ToString();
        }

        private Token GetStringLiteral(char delim)
        {
            StringBuilder builder = new StringBuilder();
            while (!isEOF && Peek() != delim)
            {
                builder.Append(Step());
            }
            Consume(delim);
            return new Token(builder.ToString(), Token.TokenType.StringLiteral);
        }

        private bool LookAhead(params char[] chars)
        {
            int start = _index;
            for (int i = 0; i < chars.Length; i++)
            {
                if (start + i >= _input.Length) return false;
                if (_input[start + i] != chars[i]) return false;
            }
            return true;
        }

        private bool LookAhead(string str)
        {
            int start = _index;
            for (int i = 0; i < str.Length; i++)
            {
                if (start + i >= _input.Length) return false;
                if (_input[start + i] != str[i]) return false;
            }
            return true;
        }
        /// <summary>
        /// Look at the current char
        /// </summary>
        /// <returns></returns>
        private char Peek()
        {
            return _input[_index];
        }

        private void RestoreIndex(int ind)
        {
            _index = ind;
        }

        private int SaveIndex()
        {
            return _index;
        }
        /// <summary>
        /// Ignore the comment and skip ahead
        /// </summary>
        private void SkipComment()
        {
            while (!isEOF && Step() != '\n')
            {
            }
            ++_lineNumber;
            return;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private char Step()
        {
            char output = _input[_index++];
            return output;
        }

        private string Step(int count)
        {
            string output = "";
            for (int i = 0; i < count; i++)
            {
                output += _input[_index++];
            }
            return output;
        }

        private void StepBack()
        {
            if (_index > 0)
            {
                --_index;
            }
        }
        /// <summary>
        /// Step to next char if current char matches arg
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool StepIf(char c)
        {
            if (!isEOF && Peek() == c)
            {
                Step();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}