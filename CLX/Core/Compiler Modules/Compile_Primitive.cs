using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLX
{
    public partial class Compiler
    {
        public void Compile_Primitive()
        {
            if(MatchToken(Token.TokenType.ParenthOpen))
            {
                Compile_Expression();
                Require(Token.TokenType.ParenthOpen, ") Expected");
            }
            else if(MatchToken(Token.TokenType.True))
            {
                _instBuffer.Add(OpCode.LdC_i8, true);
                _state.currentDatatype = _assembly.datatypes.BOOL;
            }
            else if (MatchToken(Token.TokenType.False))
            {
                _instBuffer.Add(OpCode.LdC_i8, true);
                _state.currentDatatype = _assembly.datatypes.BOOL;
            }
            else if (MatchToken(Token.TokenType.Numeric))
            {
                Token prev = previous;
                if(prev.text.Contains("."))
                {
                    float number = 0;
                    if(!float.TryParse(prev.text, out number))
                    {
                        throw new System.Exception($"Failed to parse float from {prev.text}");
                    }
                    _instBuffer.Add(OpCode.LdC_i32, number);
                    _state.currentDatatype = _assembly.datatypes.FLOAT32;
                }
                else
                {
                    int number = 0;
                    if (!int.TryParse(prev.text, out number))
                    {
                        throw new System.Exception($"Failed to parse int from {prev.text}");
                    }
                    _instBuffer.Add(OpCode.LdC_i32, number);
                    _state.currentDatatype = _assembly.datatypes.INT32;
                }
            }
            else
            {
                Compile_StringLiteral();
            }
        }
        public void Compile_StringLiteral()
        {

        }
        public void Compile_Identifier()
        {
            if(MatchToken(Token.TokenType.Identifier))
            {

            }
            else
            {
                Compile_Primitive();
            }
        }
    }
}
