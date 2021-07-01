using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLX
{
    public partial class Compiler
    {
        /// <summary>
        /// The base class for all compilation steps
        /// </summary>
        protected abstract class CompilationStep
        {
            public Compiler compiler;
            public CompilationStep next;

            public CompilationStep(Compiler comp, CompilationStep next)
            {
                compiler = comp;
                this.next = next;
            }
            public abstract bool Execute();
        }
        /// <summary>
        /// The generic binary op compilation step
        /// </summary>
        protected class BinaryOpCompilationStep : CompilationStep
        {
            Token.TokenType[] matchingTokenTypes;
            string name;
            public BinaryOpCompilationStep(Compiler compiler, CompilationStep next, string name, params Token.TokenType[] tokenTypes) : base(compiler, next)
            {
                matchingTokenTypes = tokenTypes;
                this.name = name;
            }
            public override bool Execute()
            {
                Debug.Log(name + ":" + compiler.Peek());
                next.Execute();
                Datatype ltype = compiler._state.currentDatatype;
                while (!compiler.IsEOF() && compiler.MatchToken(matchingTokenTypes))
                {
                    Token op = compiler.previous;
                    if (compiler._state.HasLRValueRef())
                    {
                        compiler._state.CloseLRValueRef();
                    }
                    // Save this node incase we need to convert the lvalue
                    InstructionBuffer.Node savedNode = compiler._ibuffer.tail;
                    // Continue to the right side of the operation
                    next.Execute();

                    if (compiler._state.HasLRValueRef())
                    {
                        compiler._state.CloseLRValueRef();
                    }
                    Datatype rtype = compiler._state.currentDatatype;

                    // Add the appropriate instructions to the buffer based on the op and data types
                    // The exact details are inside of the class for the left datatype class
                    ltype.GetBinaryOpInstructions(op.type, rtype, ref compiler._ibuffer);

                }
                return true;
            }
        }
        /// <summary>
        /// Compile Identifer compilation step
        /// </summary>
        protected class Compile_Identifier : CompilationStep
        {
            public Compile_Identifier(Compiler comp, CompilationStep next) : base(comp, next) { }

            public override bool Execute()
            {
                if (compiler.MatchToken(Token.TokenType.Identifier))
                {

                }
                else
                {
                    next.Execute();
                }
                return true;
            }
        }
        /// <summary>
        /// Compile pre unary compilation step
        /// </summary>
        protected class Compile_PreUnary: CompilationStep
        {
            public Compile_PreUnary(Compiler comp, CompilationStep next) : base(comp, next) { }

            public override bool Execute()
            {
                Debug.Log("PreUnary:" + compiler.Peek());
                bool isPreunary = false;
                if (!compiler.IsEOF() && compiler.MatchToken(Token.TokenType.Subtract))
                {
                    isPreunary = true;
                    this.Execute();
                    Token op = compiler.previous;
                    if (compiler._state.HasLRValueRef())
                    {
                        compiler._state.CloseLRValueRef();
                    }
                    compiler._state.currentDatatype.GetPreUnaryInstructions(op.type, ref compiler._ibuffer);

                }
                if (!isPreunary) { next.Execute(); }
                return isPreunary;
            }
        }
        /// <summary>
        /// Compile post unary compilation step
        /// </summary>
        protected class Compile_PostUnary : CompilationStep
        {
            public Compile_PostUnary(Compiler comp, CompilationStep next) : base(comp, next) { }

            public override bool Execute()
            {
                Debug.Log("PostUnary:" + compiler.Peek());
                next.Execute();
                return true;
            }
        }
        /// <summary>
        /// Compile primitve compilation step
        /// </summary>
        protected class Compile_Primitive : CompilationStep
        {
            public Compile_Primitive(Compiler comp, CompilationStep next) : base(comp, next) { }

            public override bool Execute()
            {
                Debug.Log("Primitive:" + compiler.Peek());
                if (compiler.MatchToken(Token.TokenType.ParenthOpen))
                {
                    compiler.Compile_Expression();
                    compiler.Require(Token.TokenType.ParenthClose, ") Expected");
                }
                else if (compiler.MatchToken(Token.TokenType.True))
                {
                    compiler._ibuffer.Add(OpCode.LdC_i8, true);
                    compiler._state.currentDatatype = compiler._assembly.datatypes.BOOL;
                }
                else if (compiler.MatchToken(Token.TokenType.False))
                {
                    compiler._ibuffer.Add(OpCode.LdC_i8, true);
                    compiler._state.currentDatatype = compiler._assembly.datatypes.BOOL;
                }
                else if (compiler.MatchToken(Token.TokenType.Numeric))
                {
                    Debug.Log("Token Matched Numeric");
                    Token prev = compiler.previous;
                    if (prev.text.Contains("."))
                    {
                        float number = 0;
                        if (!float.TryParse(prev.text, out number))
                        {
                            throw new System.Exception($"Failed to parse float from {prev.text}");
                        }
                        compiler._ibuffer.Add(OpCode.LdC_i32, number);
                        compiler._state.currentDatatype = compiler._assembly.datatypes.FLOAT32;
                    }
                    else
                    {
                        int number = 0;
                        if (!int.TryParse(prev.text, out number))
                        {
                            throw new System.Exception($"Failed to parse int from {prev.text}");
                        }
                        compiler._ibuffer.Add(OpCode.LdC_i32, number);
                        compiler._state.currentDatatype = compiler._assembly.datatypes.INT32;
                    }
                }
                else
                {
                    next.Execute();
                }
                return true;
            }
        }
        /// <summary>
        /// Compile string literal compilation step
        /// </summary>
        protected class Compile_StringLiteral : CompilationStep
        {
            public Compile_StringLiteral(Compiler comp, CompilationStep next) : base(comp, next) { }

            public override bool Execute()
            {
                Debug.Log("String Literal:" + compiler.Peek());
                if (compiler.MatchToken(Token.TokenType.Identifier))
                {

                }
                else
                {
                    //next.Execute();
                }
                return true;
            }
        }

        protected class Compile_Assignment: CompilationStep
        {
            public Compile_Assignment(Compiler comp, CompilationStep next) : base(comp, next) { }

            public override bool Execute()
            {
                Debug.Log("Assign:" + compiler.Peek());
                next.Execute();
                if (compiler.MatchToken(Token.TokenType.Assign))
                {
                    if(!compiler._state.HasLRValueRef())
                    {
                        throw new System.Exception("Can only assign to an l-value");
                    }
                    State.Reference lref = compiler._state.currentLRReference;

                    compiler.Step_Expression.Execute();
                    if (compiler._state.HasLRValueRef())
                    {
                        compiler._state.CloseLRValueRef();
                    }

                    lref.datatype.GetStoreInstructions(ref compiler._ibuffer, lref);
                    compiler._state.currentDatatype = lref.datatype;
                    compiler._state.currentDatatype = null;

                }
                return true;
            }
        }

        CompilationStep Step_StringLiteral;
        CompilationStep Step_Primitive;
        CompilationStep Step_Identifier;
        CompilationStep Step_PreUnary;
        CompilationStep Step_PostUnary;
        CompilationStep Step_PowRoot;
        CompilationStep Step_MulDiv;
        CompilationStep Step_AddSub;
        CompilationStep Step_Comparison;
        CompilationStep Step_Equality;
        CompilationStep Step_AndOr;
        CompilationStep Step_Assignment;
        CompilationStep Step_Declaration;

        CompilationStep Step_Expression;

        public void BuildCompilationChain()
        {
            Step_StringLiteral = new Compile_StringLiteral  (this, null);
            Step_Primitive = new Compile_Primitive          (this, Step_StringLiteral);
            Step_Identifier = new Compile_Identifier        (this, Step_Primitive);
            Step_PreUnary = new Compile_PreUnary            (this, Step_Identifier);
            Step_PostUnary = new Compile_PostUnary          (this, Step_PreUnary);
            Step_PowRoot = new BinaryOpCompilationStep      (this, Step_PostUnary,  "PowRoot", Token.TokenType.Power);
            Step_MulDiv = new BinaryOpCompilationStep       (this, Step_PowRoot,    "MulDiv", Token.TokenType.Multiply, Token.TokenType.Divide);
            Step_AddSub = new BinaryOpCompilationStep       (this, Step_MulDiv,     "AddSub", Token.TokenType.Add, Token.TokenType.Subtract);
            Step_Comparison = new BinaryOpCompilationStep   (this, Step_AddSub,     "Compare", Token.TokenType.LessThan, Token.TokenType.GreaterThan, Token.TokenType.LessThanOrEqual, Token.TokenType.GreaterThanOrEqual);
            Step_Equality = new BinaryOpCompilationStep     (this, Step_Comparison, "Equality", Token.TokenType.Equals, Token.TokenType.NotEquals);
            Step_AndOr = new BinaryOpCompilationStep        (this, Step_Equality,   "AndOr", Token.TokenType.And, Token.TokenType.Or);
        }


        public bool Compile_If()
        {
            return false;
        }
        public bool Compile_For()
        {
            return false;
        }
        public bool Compile_While()
        {
            return false;
        }
        public bool Compile_Foreach()
        {
            return false;
        }
        public bool Compile_Block()
        {
            return false;
        }
        public void Compile_Expression()
        {
            Debug.Log("**Expression:" + Peek());
            Step_Equality.Execute();
        }

    }
}