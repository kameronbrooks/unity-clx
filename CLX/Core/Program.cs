using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace CLX
{
    public class Program
    {
        public class Resource
        {
            public string name;
            System.Delegate[] func;
            public object[] args;
            public System.Type returnType;

            public Resource(System.Delegate f, int argCount)
            {
                args = new object[argCount];
                func = new System.Delegate[1];
                func[0] = f;
            }
            public Resource(System.Delegate get, System.Delegate set, int argCount)
            {
                args = new object[argCount];
                func = new System.Delegate[2];
                func[0] = get;
                func[1] = set;
            }
            public object GetField(object target)
            {
                return ((System.Func<object, object>)func[0])(target);
            }
            public void SetField(object target, object value)
            {
                ((System.Action<object, object>)func[1])(target, value);
            }
            public T Invoke<T> (object target)
            {
                args[0] = target;
                return (T)func[0].DynamicInvoke(args);
            }
            public T Invoke<T>(object target, object arg0)
            {
                args[0] = target;
                args[1] = arg0;
                return (T)func[0].DynamicInvoke(args);
            }
            public T Invoke<T>(object target, object arg0, object arg1)
            {
                args[0] = target;
                args[1] = arg0;
                args[2] = arg1;
                return (T)func[0].DynamicInvoke(args);
            }
            public T Invoke<T>(object target, object arg0, object arg1, object arg2)
            {
                args[0] = target;
                args[1] = arg0;
                args[2] = arg1;
                args[3] = arg2;
                return (T)func[0].DynamicInvoke(args);
            }
        }

        public List<Resource> resources;
        public object[] constObjects;
        public Instruction[] instructions;
        public int minStackSize;
    }

}