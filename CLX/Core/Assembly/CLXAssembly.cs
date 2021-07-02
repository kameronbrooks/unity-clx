using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLX
{
    public class Assembly
    {
        public struct Datatypes
        {
            public Datatype BOOL;
            public Datatype INT32;
            public Datatype FLOAT32;
            public Datatype STRING;
            public Datatype DICTIONARY;
        }

        public class Namespace : IAssemblyElement
        {
            private static int baseID = 0;

            public Namespace parent;
            int _id;
            string _name;
            string _fullyQualifiedName;
            Dictionary<string, IAssemblyElement> _map;

            public Namespace(string name)
            {
                _id = baseID++;
                _name = name;
                _map = new Dictionary<string, IAssemblyElement>();
            }

            public int id => _id;

            public AssemblyElementType elementType => AssemblyElementType.Namespace;

            string IAssemblyElement.name => _name;

            public string fullyQualifiedName
            {
                get
                {
                    return _fullyQualifiedName;
                }
            }

            public IAssemblyElement GetElement(string qualifiedName)
            {
                if(qualifiedName.Contains("."))
                {
                    int pivot = qualifiedName.IndexOf('.');
                    string subNamespaceName = qualifiedName.Substring(0, pivot);
                    IAssemblyElement subNamespace;
                    if(_map.TryGetValue(subNamespaceName, out subNamespace))
                    {
                        if (subNamespace.elementType != AssemblyElementType.Namespace)
                        {
                            throw new System.Exception($"Failed to resolve the datatype, {subNamespaceName} is not a namespace");
                        }
                        return ((Namespace)subNamespace).GetElement(qualifiedName.Substring(pivot + 1));
                    }
                    else
                    {
                        if (parent != null)
                        {
                            return parent.GetElement(qualifiedName);
                        }
                        else
                        {
                            throw new System.Exception($"{qualifiedName} was not found");
                        }
                        
                    }
                }
                else
                {
                    IAssemblyElement datatype;
                    if (_map.TryGetValue(qualifiedName, out datatype))
                    {
                        return datatype;
                    }
                    else
                    {
                        if (parent != null)
                        {
                            return parent.GetElement(qualifiedName);
                        }
                        else
                        {
                            throw new System.Exception($"{qualifiedName} was not found");
                        }
                    }
                }
            }

            public bool TryGetElement(string qualifiedName, out IAssemblyElement element)
            {
                element = null;
                if (qualifiedName.Contains("."))
                {
                    int pivot = qualifiedName.IndexOf('.');
                    string subNamespaceName = qualifiedName.Substring(0, pivot);
                    IAssemblyElement subNamespace;
                    // If found in the map then check the child
                    if (_map.TryGetValue(subNamespaceName, out subNamespace))
                    {
                        if (subNamespace.elementType != AssemblyElementType.Namespace)
                        {
                            return false;
                        }
                        return ((Namespace)subNamespace).TryGetElement(qualifiedName.Substring(pivot + 1), out element);
                    }
                    // If not found in the map check the parent if one exists
                    else
                    {
                        if(parent != null)
                        {
                            return parent.TryGetElement(qualifiedName, out element);
                        }
                        else
                        {
                            return false;
                        }
                        
                    }
                }
                else
                {
                    IAssemblyElement datatype;
                    if (_map.TryGetValue(qualifiedName, out datatype))
                    {
                        element = datatype;
                        return true;
                    }
                    else
                    {
                        if (parent != null)
                        {
                            return parent.TryGetElement(qualifiedName, out element);
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            public Namespace AddNamespace(string name)
            {
                Namespace ns = new Namespace(name);
                ns.parent = this;
                ns._fullyQualifiedName = $"{this._fullyQualifiedName}.{name}";
                _map.Add(name, ns);
                return ns;
            }
            public Datatype AddDatatype(Datatype dt)
            {
                _map.Add(dt.name, dt);
                dt.parentNamespace = this;
                return dt;
            }
            public Datatype AddDatatype(Datatype dt, string alias)
            {
                _map.Add(alias, dt);
                dt.parentNamespace = this;
                return dt;
            }

            public IAssemblyElement this[string name]
            {
                get
                {
                    IAssemblyElement elem;
                    if(TryGetElement(name, out elem))
                    {
                        return elem;
                    }
                    return null;
                }
            }
        }

        public Assembly()
        {
            _global = new Namespace("__global__");
            datatypes = new Datatypes();

            datatypes.BOOL = new Datatype_Bool(this);
            datatypes.INT32 = new Datatype_Int32(this);
            datatypes.FLOAT32 = new Datatype_Float32(this);
            //datatypes.STRING = new Datatype("string", Datatype.TYPEID_STRING);
            //datatypes.STRING = new Datatype("dict", Datatype.TYPEID_DICTIONARY);

            _global.AddDatatype(datatypes.BOOL);

            _global.AddDatatype(datatypes.INT32);
            _global.AddDatatype(datatypes.INT32, "int");

            _global.AddDatatype(datatypes.FLOAT32);
            _global.AddDatatype(datatypes.FLOAT32, "float");
            //_global.AddDatatype(datatypes.STRING);
            //_global.AddDatatype(datatypes.DICTIONARY);
        }
        Namespace _global;
        public Datatypes datatypes;
        

        public Namespace global
        {
            get
            {
                return _global;
            }
        }

        public bool TryGetElement(string name, out IAssemblyElement elem)
        {
            elem = null;
            if(_global.TryGetElement(name, out elem))
            {
                return true;
            }
            return false;
        }

        public Datatype GetDataType(string name)
        {
            IAssemblyElement output = _global.GetElement(name);
            if(output.elementType != AssemblyElementType.Datatype)
            {
                throw new System.Exception($"{name} is not a datatype");
            }
            return (Datatype)output;
        }

        public bool TryGetDatatype(string name, out Datatype dt)
        {
            dt = null;
            IAssemblyElement elem;
            if(TryGetElement(name, out elem))
            {
                if(elem.elementType == AssemblyElementType.Datatype)
                {
                    dt = (Datatype)elem;
                    return true;
                }
            }
            return false;
        }

        public Namespace GetNamespace(string name)
        {
            IAssemblyElement output = _global.GetElement(name);
            if (output.elementType != AssemblyElementType.Namespace)
            {
                throw new System.Exception($"{name} is not a namespace");
            }
            return (Namespace)output;
        }

        public bool TryGeNamespace(string name, out Namespace ns)
        {
            ns = null;
            IAssemblyElement elem;
            if (TryGetElement(name, out elem))
            {
                if (elem.elementType == AssemblyElementType.Namespace)
                {
                    ns = (Namespace)elem;
                    return true;
                }
            }
            return false;
        }

        public IAssemblyElement this[string name]
        {
            get
            {
                IAssemblyElement elem;
                if (TryGetElement(name, out elem))
                {
                    return elem;
                }
                return null;
            }
        }



        
    }
}