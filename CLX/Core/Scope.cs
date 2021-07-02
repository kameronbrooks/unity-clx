using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLX { 
    public class Scope
    {
        Scope _parent;
        Scope _child;
        public class ScopeElement
        {
            public CLX.Compiler.State.Reference reference;
            public string name;
        }
        Dictionary<string, ScopeElement> _elements;

        public Scope()
        {
            _elements = new Dictionary<string, ScopeElement>();
        }
        public Scope PushChild()
        {
            _child = new Scope();
            _child._parent = this;
            return _child;
        }
        public Scope PopChild()
        {
            if(_child != null)
            {
                _child._parent = null;
                _child = null;
            }
            return this;
        }
        public Scope Pop()
        {
            if(_parent != null)
            {
                _parent._child = null;
                return _parent;
            }
            else
            {
                return this;
            }
            
        }

        public void AddElement(string name, CLX.Compiler.State.Reference reference)
        {
            _elements.Add(name, new ScopeElement { name = name, reference = reference });
            Debug.Log(this);
        }
        public bool TryGetElement(string name, out ScopeElement elem)
        {
            if(_elements.TryGetValue(name, out elem))
            {
                return true;
            }
            else if(_parent == null)
            {
                return false;
            }
            else
            {
                return _parent.TryGetElement(name, out elem);
            }
        }
        public ScopeElement GetElement(string name)
        {
            ScopeElement output = null;
            if(_elements.TryGetValue(name, out output ))
            {
                return output;
            }
            else if(_parent == null)
            {
                throw new System.IndexOutOfRangeException($"The variable {name} was not found in scope");
            }
            else
            {
                return _parent.GetElement(name);
            }
        }

        public ScopeElement this[string name]
        {
            get
            {
                ScopeElement output = null;
                if(TryGetElement(name, out output))
                {
                    return output;
                }
                return null;
            }
            set
            {
                _elements[name] = value;
            }
        }

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (var kvp in _elements)
            {
                sb.AppendLine($"{kvp.Key} ({kvp.Value.reference.datatype.name}) @ +{kvp.Value.reference.offset}");
            }
            return sb.ToString();
        }
    }
}
