using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace CLX
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class CLXAPIClass : System.Attribute
    {
        public System.Type type;
        public CLXAPIClass(System.Type type)
        {
            this.type = type;
        }
    }


    public unsafe abstract class CSharpAPI
    {
        public delegate void API_CALL(byte** sp, Stack<object> os);

        public System.Type targetType
        {
            get
            {
                return GetType().GetCustomAttribute<CLXAPIClass>().type;
            }
        }



        public abstract API_CALL GetField(string name);
        public abstract API_CALL SetField(string name);
        public abstract API_CALL GetProperty(string name);
        public abstract API_CALL SetProperty(string name);
        public abstract API_CALL Method(string name, params System.Type[] signature);

        public API_CALL[] GetMember(string name)
        {
            List<API_CALL> apicalls = new List<API_CALL>();
            MemberInfo[] memberInfo = targetType.GetMember(name);
            foreach(var member in memberInfo)
            {
                if(member.MemberType == MemberTypes.Field)
                {
                    apicalls.Add(GetField(name));
                    apicalls.Add(SetField(name));
                }
                else if (member.MemberType == MemberTypes.Property)
                {
                    apicalls.Add(GetProperty(name));
                    apicalls.Add(SetProperty(name));
                }
                else if (member.MemberType == MemberTypes.Method)
                {
                    apicalls.Add(Method(name));
                }
                else
                {

                }

            }
            return apicalls.ToArray();
        }

    }

    public static class APIExtentions
    {
        /// <summary>
        /// Extention class to get the type api from the class
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static CSharpAPI FindAPI(this System.Type type)
        {
            foreach (var t in type.Assembly.GetTypes())
            {
                var apiclass = t.GetCustomAttribute<CLXAPIClass>();
                if (apiclass != null)
                {
                    if (apiclass.type == type)
                    {
                        return (CSharpAPI)System.Activator.CreateInstance(t);
                    }
                }
            }
            return null;
        }
    }


    
}
