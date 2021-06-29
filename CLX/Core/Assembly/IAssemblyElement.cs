using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLX
{
    public enum AssemblyElementType
    {
        Namespace,
        Datatype
    }
    public interface IAssemblyElement
    {
        int id { get; }
        string name { get; }
        string fullyQualifiedName { get; }
        AssemblyElementType elementType { get; }
    }

}