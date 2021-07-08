using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLX
{
    public class Datatype_Object : Datatype
    {
        System.Type _csType;
        public Datatype_Object(Assembly asm, System.Type cstype) : base(asm)
        {
            _csType = cstype;
            _id = 100;
            _name = $"object_{cstype.FullName}";
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

        public override void GetPrintInstructions(ref InstructionBuffer buffer)
        {
            
        }

        public override Datatype GetStoreInstructions(ref InstructionBuffer buffer, Compiler.State.Reference reference)
        {
            return this;
        }
    }
}
