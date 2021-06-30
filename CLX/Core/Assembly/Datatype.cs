using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLX
{
    public abstract class Datatype : IAssemblyElement
    {
        public const int TYPEID_BOOL = 1;
        public const int TYPEID_INT = 4;
        public const int TYPEID_FLOAT = 5;
        public const int TYPEID_STRING = 6;
        public const int TYPEID_DICTIONARY = 7;

        IAssemblyElement _parentNamespace;

        /// <summary>
        /// Name of the type
        /// </summary>
        protected string _name;
        /// <summary>
        /// Unique id for the type
        /// </summary>
        protected int _id;
        /// <summary>
        /// How large is this on the stack
        /// </summary>
        public readonly int byteSize;
        /// <summary>
        /// Fully qualified name
        /// </summary>
        protected string _fullyQualifiedName;

        public readonly Assembly assembly;

        public Datatype(Assembly asm)
        {
            assembly = asm;
            _name = "";
            _id = -1;
        }
        public Datatype(string name, int id)
        {
            _name = name;
            _id = id;
        }

        public AssemblyElementType elementType => AssemblyElementType.Datatype;

        public int id => _id;

        public string name => _name;

        public string fullyQualifiedName
        {
            get
            {
                return _fullyQualifiedName;
            }

        }

        public IAssemblyElement parentNamespace
        {
            get
            {
                return _parentNamespace;
            }
            set
            {
                _parentNamespace = value;
                _fullyQualifiedName = $"{_parentNamespace.fullyQualifiedName}.{_name}";
            }
        }


        public abstract Datatype GetPreUnaryInstructions(Token.TokenType opType, ref InstructionBuffer buffer);
        public abstract Datatype GetPostUnaryInstructions(Token.TokenType opType, ref InstructionBuffer buffer);
        public abstract Datatype GetBinaryOpInstructions(Token.TokenType opType, Datatype rtype, ref InstructionBuffer buffer);
        public abstract Datatype GetStoreInstructions(ref InstructionBuffer buffer, Compiler.State.Reference.RefType refType);
        public abstract Datatype GetLoadInstructions(ref InstructionBuffer buffer, Compiler.State.Reference.RefType refType);
        public abstract Datatype GetConvertInstructions(ref InstructionBuffer buffer, Datatype target);

        public virtual void ApplyTypePrecedence(Datatype rtype, InstructionBuffer.Node insertionNode)
        {

        }

    }
}