using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLX
{
    public class Datatype_Array : Datatype
    {
        public Datatype_Array(Assembly asm) : base(asm)
        {
            _name = "array";
            _id = 10;
        }

        public override Datatype GetBinaryOpInstructions(Token.TokenType opType, Datatype rtype, ref InstructionBuffer buffer)
        {
            throw new System.NotImplementedException();
        }

        public override Datatype GetConvertInstructions(ref InstructionBuffer buffer, Datatype target)
        {
            throw new System.NotImplementedException();
        }

        public override Datatype GetLoadInstructions(ref InstructionBuffer buffer, Compiler.State.Reference reference)
        {
            throw new System.NotImplementedException();
        }

        public override Datatype GetPostUnaryInstructions(Token.TokenType opType, ref InstructionBuffer buffer)
        {
            throw new System.NotImplementedException();
        }

        public override Datatype GetPreUnaryInstructions(Token.TokenType opType, ref InstructionBuffer buffer)
        {
            throw new System.NotImplementedException();
        }

        public override void GetPrintInstructions(ref InstructionBuffer buffer)
        {
            throw new System.NotImplementedException();
        }

        public override Datatype GetStoreInstructions(ref InstructionBuffer buffer, Compiler.State.Reference reference)
        {
            throw new System.NotImplementedException();
        }
    }
}
