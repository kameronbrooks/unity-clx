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
        /// <summary>
        /// Returns the API_CALL with the matching name that is a member of this API Class
        /// Uses reflection to look at the members of this class and find the matching one with the attribute APIMethod
        /// The signature for the method must be void API_CALL(byte** sp, Stack<object> os)
        /// Method must me marked as unsafe
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected API_CALL GetMemberAPI_CALL(string name)
        {
            MethodInfo method = this.GetType().GetMethod(name);
            if (method.GetCustomAttribute<APIMethod>() != null)
            {
                return (API_CALL)method.CreateDelegate(typeof(API_CALL), this);
            }
            return null;
        }

        public virtual API_CALL[] Field(string name)
        {
            return new API_CALL[2]
            {
                GetMemberAPI_CALL(name + "_get"),
                GetMemberAPI_CALL(name + "_set")
            };
        }
        public virtual API_CALL[] Property(string name)
        {
            return new API_CALL[2]
            {
                GetMemberAPI_CALL(name + "_get"),
                GetMemberAPI_CALL(name + "_set")
            };
        }
        public virtual API_CALL[] Method(string name)
        {
            return new API_CALL[]
            {
                GetMemberAPI_CALL(name)
            };
        }

        public API_CALL[] GetMember(string name)
        {
            List<API_CALL> apicalls = new List<API_CALL>();
            MemberInfo[] memberInfo = targetType.GetMember(name);
            foreach(var member in memberInfo)
            {
                if(member.MemberType == MemberTypes.Field)
                {
                    apicalls.AddRange(Field(name));
                }
                else if (member.MemberType == MemberTypes.Property)
                {
                    apicalls.AddRange(Property(name));
                }
                else if (member.MemberType == MemberTypes.Method)
                {
                    apicalls.AddRange(Method(name));
                }
                else
                {
                    throw new System.Exception($"The member {name} was not found in the class {targetType.FullName}");
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
