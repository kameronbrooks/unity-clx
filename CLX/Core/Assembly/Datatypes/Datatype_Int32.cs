using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLX
{
    public class Datatype_Int32 : Datatype
    {

        public Datatype_Int32(Assembly asm) : base(asm)
        {
            _name = "int32";
            _id = Datatype.TYPEID_INT;
        }

        public override Datatype GetBinaryOpInstructions(Token.TokenType opType, Datatype rtype, ref InstructionBuffer buffer)
        {
            switch (opType)
            {
                case Token.TokenType.Add:
                    if(rtype == this)
                    {
                        buffer.Add(OpCode.Add_i32);
                        return this;
                    }
                    break;
                case Token.TokenType.Subtract:
                    if (rtype == this)
                    {
                        buffer.Add(OpCode.Sub_i32);
                        return this;
                    }
                    break;
                case Token.TokenType.Multiply:
                    if (rtype == this)
                    {
                        buffer.Add(OpCode.Mul_i32);
                        return this;
                    }
                    break;
                case Token.TokenType.Divide:
                    if (rtype == this)
                    {
                        buffer.Add(OpCode.Div_i32);
                       
                        return this;
                    }
                    break;
                case Token.TokenType.Modulo:
                    if (rtype == this)
                    {
                        buffer.Add(OpCode.Mod_i32);
                        return this;
                    }
                    break;
                case Token.TokenType.Equals:
                    if (rtype == this)
                    {
                        buffer.Add(OpCode.Eq_i32);
                        return this;
                    }
                    break;
                case Token.TokenType.NotEquals:
                    if (rtype == this)
                    {
                        buffer.Add(OpCode.NEq_i32);
                        return this;
                    }
                    break;
                case Token.TokenType.GreaterThan:
                    if (rtype == this)
                    {
                        buffer.Add(OpCode.GT_i32);

                        return this;
                    }
                    break;
                case Token.TokenType.LessThan:
                    if (rtype == this)
                    {
                        buffer.Add(OpCode.LT_i32);

                        return this;
                    }
                    break;
                case Token.TokenType.GreaterThanOrEqual:
                    if (rtype == this)
                    {
                        buffer.Add(OpCode.GTEq_i32);

                        return this;
                    }
                    break;
                case Token.TokenType.LessThanOrEqual:
                    if (rtype == this)
                    {
                        buffer.Add(OpCode.LTEq_i32);

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
            switch(target.id)
            {
                case Datatype.TYPEID_FLOAT:
                    buffer.Add(OpCode.Conv_i32_f32);
                    return target;
                default:
                    throw new System.Exception($"Cannot cast {this.fullyQualifiedName} to {target.fullyQualifiedName}");
            }
        }

        public override Datatype GetLoadInstructions(ref InstructionBuffer buffer, Compiler.State.Reference reference)
        {
            switch (reference.reftype)
            {
                case Compiler.State.Reference.RefType.Local:
                    buffer.Add(OpCode.LdLoc_i32, reference.offset);
                    break;
                case Compiler.State.Reference.RefType.Arg:
                    buffer.Add(OpCode.LdArg_i32, reference.offset);
                    break;
                case Compiler.State.Reference.RefType.Reg:
                    break;
                default:
                    throw new System.Exception($"The storage type is not supported by this data type({_name})");
            }
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
            switch(reference.reftype)
            {
                case Compiler.State.Reference.RefType.Local:
                    buffer.Add(OpCode.StLoc_i32, reference.offset);
                    break;
                case Compiler.State.Reference.RefType.Arg:
                    buffer.Add(OpCode.StArg_i32, reference.offset);
                    break;
                case Compiler.State.Reference.RefType.Reg:
                    break;
                default:
                    throw new System.Exception($"The storage type is not supported by this data type({_name})");
            }
            return this;
        }

        public override void ApplyTypePrecedence(Datatype rtype, InstructionBuffer.Node insertionNode)
        {
            if(rtype.id == Datatype.TYPEID_FLOAT)
            {
                insertionNode.InsertAfter(new InstructionBuffer.Node
                {
                    instruction = new Instruction(OpCode.Conv_i32_f32)
                });
            }
        }

        public override void GetPrintInstructions(ref InstructionBuffer buffer)
        {
            buffer.Add(OpCode.Prnt_i32);
        }
    }
}
