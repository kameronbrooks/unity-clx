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
                        compiler._state.CollapseCurrentRef(ref compiler._ibuffer, true);;
                    }
                    // Save this node incase we need to convert the lvalue
                    InstructionBuffer.Node savedNode = compiler._ibuffer.tail;
                    // Continue to the right side of the operation
                    next.Execute();

                    if (compiler._state.HasLRValueRef())
                    {
                        compiler._state.CollapseCurrentRef(ref compiler._ibuffer, true);;
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
                Debug.Log("Identifier:" + compiler.Peek());
                if (compiler.MatchToken(Token.TokenType.Identifier))
                {
                    Token prev = this.compiler.previous;
                    // Check to see if this identifier matches a type name
                    // if it does match a type name then this is a declaration
                    Datatype datatype = null;
                    if(compiler._assembly.TryGetDatatype(prev.text, out datatype))
                    {
                        // Update the datatype field of the Declaration Step before calling execute
                        compiler.Step_Declaration.datatype = datatype;
                        compiler.Step_Declaration.Execute();
                        return true;
                    }
                    // Check current scope for this element
                    Scope.ScopeElement scopeElement = null;
                    if(compiler._state.currentScope.TryGetElement(prev.text, out scopeElement))
                    {
                        // If found then set the current LR reference to the one found in this scope
                        compiler._state.currentLRReference = scopeElement.reference;
                        compiler._state.currentDatatype = scopeElement.reference.datatype;
                        Debug.Log("LR Ref set " + compiler._state.currentLRReference);
                    }
                    // Check to see if we are referencing an api member
                    else if (compiler._state.externalTarget != null || ReflectionUtility.HasMember(compiler._state.externalTarget, prev.text))
                    {
                        
                        string name = compiler._state.externalTarget.FullName + "." + prev.text;
                        Program.Resource resource = null;
                        // Check if resource already exists
                        if(!compiler._resourceTable.TryGetValue(name, out resource))
                        {
                            // Add generate the resource and specify its index
                            resource = ReflectionUtility.GenerateResource(compiler._state.externalTarget, prev.text);
                            resource.id = compiler._resourceTable.Count;
                            // Add to the resource table
                            compiler._resourceTable.Add(name, resource);
                            
                        }

                        compiler._state.currentLRReference = new State.Reference
                        {
                            reftype = State.Reference.RefType.API,
                            index = resource.id,
                            datatype = compiler._assembly.FromCSharpType(resource.returnType)
                        };
                        compiler._state.currentDatatype = compiler._state.currentLRReference.datatype;



                    }
                    // We dont know what this thing is
                    else
                    {
                        throw new System.Exception($"The identifier {prev.text} was not found in the current scope");
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
                        compiler._state.CollapseCurrentRef(ref compiler._ibuffer, true);;
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
                    compiler.Step_Expression.Execute();
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
                next.Execute();

                Debug.Log("Assign:" + compiler.Peek());
                if (compiler.MatchToken(Token.TokenType.Assign))
                {
                    if(!compiler._state.HasLRValueRef())
                    {
                        throw new System.Exception("Can only assign to an l-value");
                    }
                    State.Reference lref = compiler._state.CollapseCurrentRef(ref compiler._ibuffer, false);

                    compiler.Step_Expression.Execute();

                    if (compiler._state.HasLRValueRef())
                    {
                        compiler._state.CollapseCurrentRef(ref compiler._ibuffer, true);
                    }

                    lref.GetStoreInstructions(ref compiler._ibuffer);
                    compiler._state.currentDatatype = lref.datatype;
                    

                }
                return true;
            }
        }

        protected class Compile_Declaration : CompilationStep
        {
            public Compile_Declaration(Compiler comp, CompilationStep next) : base(comp, next) { }
            public Datatype datatype;
            public override bool Execute()
            {
                Debug.Log("Declaration:" + compiler.Peek());
                if (compiler.MatchToken(Token.TokenType.Identifier))
                {
                    string varname = compiler.previous.text;
                    // See if the variable is already defined
                    Scope.ScopeElement element = null;
                    if(compiler._state.currentScope.TryGetElement(varname, out element))
                    {
                        throw new System.Exception($"The variable {varname} is already defined in this scope");
                    }
                    // TODO this is where the function declaration hook will be
                    // Add the variable to the current scope
                    unsafe
                    {
                        compiler._state.currentScope.AddElement(varname, new State.Reference
                        {
                            datatype = datatype,
                            reftype = State.Reference.RefType.Local,
                            offset = (sizeof(byte**) + sizeof(Instruction*)) + compiler._state.localVariableBytes
                        });
                        compiler._state.localVariableBytes += datatype.byteSize;
                    }
                    // If immediately assigning
                    if(compiler.LookAhead(Token.TokenType.Assign))
                    {
                        compiler.BackStep();
                        compiler.Step_Assignment.Execute();
                    }

                }
                else
                {
                    // For some reason we have a type name but this is not a declaration
                    //next.Execute();
                }
                return true;
            }

        }


        protected class Compile_Expression : CompilationStep
        {
            public Compile_Expression(Compiler comp, CompilationStep next) : base(comp, next) { }
            public override bool Execute()
            {
                return next.Execute();
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
        Compile_Declaration Step_Declaration;

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
            Step_Assignment = new Compile_Assignment        (this, Step_AndOr);
            Step_Expression = new Compile_Expression        (this, Step_Assignment);

            Step_Declaration = new Compile_Declaration      (this, null);

            
        }


        public bool Compile_If()
        {
            if(MatchToken(Token.TokenType.If))
            {
                // Conditional statement
                Step_Expression.Execute();
                
                InstructionBuffer.Node branchNode = _ibuffer.Add(OpCode.BrchFalse, 0);
                // body
                CompileStatement();
                // look for else clause
                if(!IsEOF() && MatchToken(Token.TokenType.Else))
                {
                    InstructionBuffer.Node jumpNode = _ibuffer.Add(OpCode.Jump);
                    _ibuffer.PushForwardBranchTarget(branchNode);
                    CompileStatement();
                    _ibuffer.PushForwardBranchTarget(jumpNode);
                }
                else
                {
                    _ibuffer.PushForwardBranchTarget(branchNode);
                }
                return true;

            }
            return false;
        }
        public bool Compile_For()
        {
            if (MatchToken(Token.TokenType.For))
            {
                bool parenthEnclosed = false;
                // Intialization statement
                if(!MatchToken(Token.TokenType.EOS))
                {
                    parenthEnclosed = MatchToken(Token.TokenType.ParenthOpen);
                    
                    Step_Expression.Execute();
                    Require(Token.TokenType.EOS, "; Required");
                }
                // Condition statement
                InstructionBuffer.Node conditionNode = _ibuffer.tail;
                Step_Equality.Execute();
                conditionNode = conditionNode.next;

                if (_state.HasLRValueRef())
                {
                    _state.CollapseCurrentRef(ref _ibuffer, true);
                }
                InstructionBuffer.Node branchNode = _ibuffer.Add(OpCode.BrchFalse, 0);
                Require(Token.TokenType.EOS, "; Required");
                // Increment statement
                InstructionBuffer tempBuffer = new InstructionBuffer();
                InstructionBuffer oldBuffer = _ibuffer;
                _ibuffer = tempBuffer;
                Step_Expression.Execute();
                _ibuffer = oldBuffer;

                if(parenthEnclosed)
                {
                    Debug.Log("Peek: " + Peek().text);
                    Require(Token.TokenType.ParenthClose, ") Expected");
                }
                // Body
                CompileStatement();
                _ibuffer.Append(tempBuffer);
                InstructionBuffer.Node jumpNode = _ibuffer.Add(OpCode.Jump);
                jumpNode.reference = conditionNode;
                _ibuffer.PushForwardBranchTarget(branchNode);

                return true;
            }
            return false;
        }
        public bool Compile_While()
        {
            if (MatchToken(Token.TokenType.While))
            {
                InstructionBuffer.Node conditionNode = _ibuffer.tail;
                Step_Equality.Execute();
                conditionNode = conditionNode.next;

                if(_state.HasLRValueRef())
                {
                    _state.CollapseCurrentRef(ref _ibuffer, true);
                }
                InstructionBuffer.Node branchNode = _ibuffer.Add(OpCode.BrchFalse, 0);
                CompileStatement();
                InstructionBuffer.Node jumpNode = _ibuffer.Add(OpCode.Jump);
                jumpNode.reference = conditionNode;
                _ibuffer.PushForwardBranchTarget(branchNode);

                return true;
            }
            return false;
        }
        public bool Compile_Foreach()
        {
            return false;
        }
        public bool Compile_Block(bool pushScope = true)
        {
            if(MatchToken(Token.TokenType.CurlyBracketOpen))
            {
                if(pushScope)
                {
                    _state.PushScope();
                }
                while (!IsEOF() && Peek().type != Token.TokenType.CurlyBracketClose)
                {
                    CompileStatement();
                }
                Require(Token.TokenType.CurlyBracketClose, "} Expected");
                if (pushScope)
                {
                    _state.PopScope();
                }
                return true;
            }
            return false;
        }

        public bool Compile_Print()
        {
            if(Peek().text == "print")
            {
                Step();
                CompileStatement();
                if (_state.HasLRValueRef())
                {
                    _state.CollapseCurrentRef(ref _ibuffer, true);
                }
                _state.currentDatatype.GetPrintInstructions(ref _ibuffer);
                return true;
            }
            return false;
        }


    }
}
