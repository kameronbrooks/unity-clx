using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLX
{
    public class Datatype : IAssemblyElement
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
        string _name;
        /// <summary>
        /// Unique id for the type
        /// </summary>
        int _id;
        /// <summary>
        /// How large is this on the stack
        /// </summary>
        public readonly int byteSize;
        /// <summary>
        /// Fully qualified name
        /// </summary>
        string _fullyQualifiedName;

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
    }
}