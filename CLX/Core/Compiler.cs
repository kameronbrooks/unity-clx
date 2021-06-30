using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

namespace CLX
{
    public partial class Compiler
    {
        public class State
        {
            public class Reference
            {
                public enum RefType { Local, Arg, Reg, Elem }
                public RefType reftype;
                public Datatype datatype;
                public int offset;
                public int index;

                public bool Close(ref InstructionBuffer buffer)
                {
                    return true;
                }
            }
            public class Substate
            {
                public Datatype currentDatatype;
                public Reference lvalueReference;

                public Substate Copy()
                {
                    return new Substate
                    {
                        currentDatatype = currentDatatype
                    };
                }
            }
            Stack<Substate> _frames;

            public Datatype currentDatatype
            {
                get
                {
                    return _frames.Peek().currentDatatype;
                }
                set
                {
                    _frames.Peek().currentDatatype = value;
                }
            }

            /// <summary>
            /// Does the state have an open reference that can become either l/r?
            /// </summary>
            /// <returns></returns>
            public bool HasLRValueRef()
            {
                return _frames.Peek().lvalueReference != null;
            }
            /// <summary>
            /// Turn the saved l/r value into just an r value
            /// </summary>
            public void CloseLRValueRef()
            {
                _frames.Peek().lvalueReference = null;
            }

            public State()
            {
                _frames = new Stack<Substate>();
                _frames.Push(new Substate());
            }

            public void Push()
            {
                _frames.Push(_frames.Peek().Copy());
            }
            public void Pop()
            {
                if (_frames.Count > 1)
                {
                    _frames.Pop();
                }
            }
        }
        Token[] _tokens;
        int _index;
        int _tokenCount;
        Program _program;
        Type _targetType;
        InstructionBuffer _ibuffer;
        Assembly _assembly;
        State _state;

        public Program Compile(string script, Type targetType)
        {
            _program = new Program();
            _targetType = targetType;
            Lexer lexer = new Lexer();
            _ibuffer = new InstructionBuffer();
            _assembly = new Assembly();
            _state = new State();
            try
            {
                _tokens = lexer.Tokenize(script);
                _tokenCount = _tokens.Length;
            }
            catch (Exception e)
            {
                Debug.LogError("CLX Failed to parse: " + e.Message);
            }
            try
            {
                while (!IsEOF())
                {
                    CompileStatement();
                }
            }
            catch(Exception e)
            {
                Debug.LogError("CLX Failed to compile: " + e.Message);
                return null;
            }

            _ibuffer.Add(new Instruction(OpCode.Term));
            // add the instructions to the program
            _program.instructions = _ibuffer.BakeAndExport();
            Debug.Log(_ibuffer);

            return _program;
        }
        /// <summary>
        /// Is end of file
        /// </summary>
        /// <returns></returns>
        private bool IsEOF()
        {
            return (_index >= _tokenCount);
        }

        /// <summary>
        /// Look at next token without incrementing index
        /// </summary>
        private Token Peek()
        {
            return _tokens[_index];
        }

        /// <summary>
        /// current token
        /// </summary>
        private Token previous
        {
            get
            {
                return _tokens[_index - 1];
            }
        }

        /// <summary>
        /// Look ahead to see if the next token matches one of the provided types
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        private bool LookAhead(params Token.TokenType[] types)
        {
            for (int i = 0; i < types.Length; i += 1)
            {
                if (isAtEnd) return false;
                if (_tokens[_index + i].type != types[i]) return false;
            }
            return true;
        }

        /// <summary>
        /// return current token and increment index
        /// </summary>
        private Token Step()
        {
            if (!isAtEnd) _index += 1;
            return previous;
        }

        /// <summary>
        /// decrement index
        /// </summary>
        private void BackStep()
        {
            if (_index > 0) _index -= 1;
        }

        /// <summary>
        /// Requires current token to be of a certain type and increments index, or throws exception
        /// </summary>
        private Token Require(Token.TokenType type, string errorMessage)
        {
            if (!isAtEnd)
            {
                if (Peek().type == type)
                {
                    _index += 1;
                    return previous;
                }
            }
            throw new Exception("Parsing Error [line " + _tokens[_index].lineNumber + "]: " + errorMessage);
        }

        /// <summary>
        /// Are we out of tokens?
        /// </summary>
        private bool isAtEnd
        {
            get
            {
                return _index >= _tokenCount;
            }
        }

        /// <summary>
        /// Checks to see if the current token type is the spefified type
        /// </summary>
        private bool CheckType(Token.TokenType type)
        {
            if (isAtEnd) return false;
            return Peek().type == type;
        }

        /// <summary>
        /// Checks to see if the current token text matches
        /// </summary>
        private bool CheckToken(string str)
        {
            if (isAtEnd) return false;
            return Peek().text == str;
        }

        /// <summary>
        /// Checks to see if the current token matches one of several strings
        /// If so, returns true and increments index
        /// </summary>
        private bool MatchToken(params string[] values)
        {
            foreach (string value in values)
            {
                if (CheckToken(value))
                {
                    Step();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks to see if the current token matches one of several types
        /// If so, returns true and increments index
        /// </summary>
        private bool MatchToken(params Token.TokenType[] types)
        {
            foreach (Token.TokenType type in types)
            {
                if (CheckType(type))
                {
                    Step();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks to see if the current token matches one of several types
        /// If so, returns token and increments index
        /// </summary>
        private Token MatchTokenAndReturn(params Token.TokenType[] types)
        {
            foreach (Token.TokenType type in types)
            {
                if (CheckType(type))
                {
                    return Step();
                }
            }
            return null;
        }

        /// <summary>
        /// Compile Statement
        /// Root of the recursive compilation tree
        /// </summary>
        private void CompileStatement()
        {
            if      (Compile_If())      { }
            else if (Compile_While())   { }
            else if (Compile_For())     { }
            else if (Compile_Foreach()) { }
            else if (Compile_Block())   { }
            else
            {
                Compile_Expression();
                Require(Token.TokenType.EOS, "; Required");
            }
        }
    }
}