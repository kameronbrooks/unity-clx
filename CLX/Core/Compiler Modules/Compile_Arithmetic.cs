using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLX
{
    public partial class Compiler
    {
        public void Compile_AddSub()
        {
            Debug.Log("AddSub:" + Peek());
            Compile_MulDiv();
            Datatype ltype = _state.currentDatatype;
            while (!IsEOF() && MatchToken(Token.TokenType.Add, Token.TokenType.Subtract))
            {
                Token op = previous;
                if(_state.HasLRValueRef())
                {
                    _state.CloseLRValueRef();
                }
                // Save this node incase we need to convert the lvalue
                InstructionBuffer.Node savedNode = _ibuffer.tail;
                // Continue to the right side of the operation
                Compile_MulDiv();

                if (_state.HasLRValueRef())
                {
                    _state.CloseLRValueRef();
                }
                Datatype rtype = _state.currentDatatype;

                // Add the appropriate instructions to the buffer based on the op and data types
                // The exact details are inside of the class for the left datatype class
                ltype.GetBinaryOpInstructions(op.type, rtype, ref _ibuffer);
                Debug.Log("Add op added");
            }
        }
        public void Compile_MulDiv()
        {
            Debug.Log("MulDiv:" + Peek());
            Compile_PowRoot();
            Datatype ltype = _state.currentDatatype;
            while (!IsEOF() && MatchToken(Token.TokenType.Multiply, Token.TokenType.Divide))
            {
                Token op = previous;
                if (_state.HasLRValueRef())
                {
                    _state.CloseLRValueRef();
                }
                // Save this node incase we need to convert the lvalue
                InstructionBuffer.Node savedNode = _ibuffer.tail;
                // Continue to the right side of the operation
                Compile_PowRoot();

                if (_state.HasLRValueRef())
                {
                    _state.CloseLRValueRef();
                }
                Datatype rtype = _state.currentDatatype;

                // Add the appropriate instructions to the buffer based on the op and data types
                // The exact details are inside of the class for the left datatype class
                ltype.GetBinaryOpInstructions(op.type, rtype, ref _ibuffer);

            }
        }
        public void Compile_PowRoot()
        {
            Debug.Log("PowRoot:" + Peek());
            Compile_PostUnary();
            Datatype ltype = _state.currentDatatype;
            while (!IsEOF() && MatchToken(Token.TokenType.Power))
            {

            }
        }
        public void Compile_PostUnary()
        {
            Debug.Log("PostUnary:" + Peek());
            Compile_PreUnary();
        }

        public void Compile_PreUnary()
        {
            Debug.Log("PreUnary:" + Peek());
            bool isPreunary = false;
            if (!IsEOF() && MatchToken(Token.TokenType.Subtract))
            {
                isPreunary = true;
                Compile_PreUnary();
                Token op = previous;
                if (_state.HasLRValueRef())
                {
                    _state.CloseLRValueRef();
                }
                _state.currentDatatype.GetPreUnaryInstructions(op.type, ref _ibuffer);
                
            }
            if (!isPreunary) { Compile_Identifier(); }
        }

       
    }
}
