using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLX
{
    public class Datatype_Float32 : Datatype
    {
        public Datatype_Float32(Assembly asm) : base(asm)
        {
            _name = "float32";
            _id = Datatype.TYPEID_FLOAT;
        }
        public override Datatype GetBinaryOpInstructions(Token.TokenType opType, Datatype rtype, ref InstructionBuffer buffer)
        {
            switch (opType)
            {
                case Token.TokenType.Add:
                    if (rtype == this)
                    {
                        buffer.Add(OpCode.Add_f32);
                        return this;
                    }
                    break;
                case Token.TokenType.Subtract:
                    if (rtype == this)
                    {
                        buffer.Add(OpCode.Sub_f32);
                        return this;
                    }
                    break;
                case Token.TokenType.Multiply:
                    if (rtype == this)
                    {
                        buffer.Add(OpCode.Mul_f32);
                        return this;
                    }
                    break;
                case Token.TokenType.Divide:
                    if (rtype == this)
                    {
                        buffer.Add(OpCode.Div_f32);
                        return this;
                    }
                    break;
                case Token.TokenType.Modulo:
                    if (rtype == this)
                    {
                        buffer.Add(OpCode.Mod_f32);
                        return this;
                    }
                    break;
                case Token.TokenType.Power:
                    if (rtype == this)
                    {
                        buffer.Add(OpCode.Pow_f32);
                        return this;
                    }
                    break;
                default:
                    break;
            }
            throw new System.Exception($"The operation {opType.ToString()} is unsupported with {this.fullyQualifiedName} and {rtype.fullyQualifiedName}");
        }

        public override Datatype GetConvertInstructions(ref InstructionBuffer buffer, Datatype target)
        {
            return this;
        }

        public override Datatype GetLoadInstructions(ref InstructionBuffer buffer, Compiler.State.Reference reference)
        {
            return this;
        }

        public override Datatype GetPostUnaryInstructions(Token.TokenType opType, ref InstructionBuffer buffer)
        {
            return this;
        }

        public override Datatype GetPreUnaryInstructions(Token.TokenType opType, ref InstructionBuffer buffer)
        {
            switch (opType)
            {
                case Token.TokenType.Negate:
                    buffer.Add(OpCode.Neg_i32);
                    return this;


                default:
                    break;
            }
            throw new System.Exception($"The operation {opType.ToString()} is unsupported for {this.fullyQualifiedName} ");
        }

        public override Datatype GetStoreInstructions(ref InstructionBuffer buffer, Compiler.State.Reference reference)
        {
            return this;
        }

        public override void GetPrintInstructions(ref InstructionBuffer buffer)
        {
            buffer.Add(OpCode.Prnt_f32);
        }
    }
}
