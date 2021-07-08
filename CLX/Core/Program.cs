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
            public System.Delegate[] func;
            public object[] args0;
            public object[] args1;
            public System.Type returnType;
            public int id;
            public MemberTypes memberType;

            public bool isWritable
            {
                get
                {
                    if(memberType == MemberTypes.Field)
                    {
                        return true;
                    }
                    else if(memberType == MemberTypes.Property && func[1] != null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            public Resource(System.Delegate f, int argCount)
            {
                args0 = new object[argCount];
                func = new System.Delegate[1];
                func[0] = f;
            }
            public Resource(System.Delegate get, System.Delegate set, int argCount0, int argCount1)
            {
                args0 = new object[argCount0];
                args1 = new object[argCount1];
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
                args0[0] = target;
                return (T)func[0].DynamicInvoke(args0);
            }
            public T Invoke<T>(object target, object arg0)
            {
                args0[0] = target;
                args0[1] = arg0;
                return (T)func[0].DynamicInvoke(args0);
            }
            public T Invoke<T>(object target, object arg0, object arg1)
            {
                args0[0] = target;
                args0[1] = arg0;
                args0[2] = arg1;
                return (T)func[0].DynamicInvoke(args0);
            }
            public T Invoke<T>(object target, object arg0, object arg1, object arg2)
            {
                args0[0] = target;
                args0[1] = arg0;
                args0[2] = arg1;
                args0[3] = arg2;
                return (T)func[0].DynamicInvoke(args0);
            }

            public Datatype GetOpCode(ref InstructionBuffer buffer, Assembly asm, bool isAPITarget = false)
            {
                Datatype dt = asm.FromCSharpType(returnType);
                InstructionBuffer.Node node = null;
                if(isAPITarget)
                {
                    switch(dt.id)
                    {
                        case Datatype.TYPEID_INT:
                            node = buffer.Add(OpCode.Call_API_i32, 0);
                            break;
                        default:
                            node = buffer.Add(OpCode.Call_API_obj, 0);
                            break;
                    }
                }
                else
                {
                    switch (dt.id)
                    {
                        case Datatype.TYPEID_INT:
                            node = buffer.Add(OpCode.Call_Ext_i32, 0);
                            break;
                        default:
                            node = buffer.Add(OpCode.Call_Ext_obj, 0);
                            break;
                    }
                }

                node.reference = this;

                return dt;
                
            }
        }

        public List<Resource> resources;
        public object[] constObjects;
        public Instruction[] instructions;
        public int minStackSize;

        public Program()
        {
            resources = new List<Resource>();
            minStackSize = 512;
        }
    }

}