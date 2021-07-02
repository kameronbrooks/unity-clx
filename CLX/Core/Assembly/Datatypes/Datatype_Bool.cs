using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLX
{
    public class Datatype_Bool : Datatype
    {
        public Datatype_Bool(Assembly asm) : base(asm)
        {
            _name = "bool";
            _id = Datatype.TYPEID_BOOL;
        }
        public override Datatype GetBinaryOpInstructions(Token.TokenType opType, Datatype rtype, ref InstructionBuffer buffer)
        {
            return this;
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
            return this;
        }

        public override Datatype GetStoreInstructions(ref InstructionBuffer buffer, Compiler.State.Reference reference)
        {
            return this;
        }
        public override void GetPrintInstructions(ref InstructionBuffer buffer)
        {
            buffer.Add(OpCode.Prnt_bool);
        }
    }
}