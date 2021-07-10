using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;

namespace CLX
{
    public static class ReflectionUtility
    {
        public static MemberInfo[] GetMemberInfo(Type type, string name)
        {
            return type.GetMember(name);
        }
        public static bool HasMember(Type type, string name)
        {
            return type.GetMember(name).Length > 0;
        }

        /// <summary>
        /// Turn a method into an instance delegate
        /// </summary>
        /// <param name="target"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static Delegate MethodToInstanceDelegate(object target, MethodInfo method)
        {
            ParameterInfo[] methodParams = method.GetParameters();
            Type[] methodParamTypes = new Type[methodParams.Length + 1];
            for (int j = 0; j < methodParams.Length; j += 1)
            {
                methodParamTypes[j] = methodParams[j].ParameterType;
            }
            methodParamTypes[methodParamTypes.Length - 1] = method.ReturnType;

            Type delegateType = System.Linq.Expressions.Expression.GetDelegateType(methodParamTypes);
            if ((target as Type) != null)
            {
                return Delegate.CreateDelegate(delegateType, null, method);
            }
            return Delegate.CreateDelegate(delegateType, target, method);
        }
        /// <summary>
        /// Turn a lambda into a delegate
        /// </summary>
        /// <param name="lambda"></param>
        /// <returns></returns>
        public static Delegate LambdaToDelegate(object lambda)
        {
            Type t = lambda.GetType();
            MethodInfo method = t.GetMethod(("Invoke"));
            return MethodToInstanceDelegate(lambda, method);
        }
        /// <summary>
        /// Create an open delegate that you can pass arguments to with the object as the first argument
        /// </summary>
        /// <param name="type"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static Delegate MethodToOpenDelegate(Type type, MethodInfo method)
        {
            ParameterInfo[] methodParams = method.GetParameters();
            Type[] methodParamTypes = new Type[methodParams.Length + 2];

            methodParamTypes[0] = type;
            for (int j = 0; j < methodParams.Length; j += 1)
            {
                methodParamTypes[j + 1] = methodParams[j].ParameterType;
            }
            methodParamTypes[methodParamTypes.Length - 1] = method.ReturnType;

            Type delegateType = System.Linq.Expressions.Expression.GetDelegateType(methodParamTypes);

            return Delegate.CreateDelegate(delegateType, null, method);
        }

        public static Delegate StaticMethodToDelegate(MethodInfo method)
        {
            ParameterInfo[] methodParams = method.GetParameters();
            Type[] methodParamTypes = new Type[methodParams.Length + 1];
            for (int j = 0; j < methodParams.Length; j += 1)
            {
                methodParamTypes[j] = methodParams[j].ParameterType;
            }
            methodParamTypes[methodParamTypes.Length - 1] = method.ReturnType;

            Type delegateType = System.Linq.Expressions.Expression.GetDelegateType(methodParamTypes);

            return Delegate.CreateDelegate(delegateType, null, method);
        }

        /// <summary>
        /// Create a program resource from a method
        /// </summary>
        /// <param name="target"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static Program.Resource GenerateResource(Type target, MethodInfo method)
        {
            CSharpAPI api = target.FindAPI();
            ParameterInfo[] parameters = method.GetParameters();
            Type[] args = new Type[parameters.Length];
            for(int i = 0; i < parameters.Length; ++i)
            {
                args[i] = parameters[i].ParameterType;
            }
            if (api != null)
            {
                Program.Resource output = new Program.Resource()
                {
                    func = api.Method(method.Name, args),
                    name = method.Name,
                    memberType = MemberTypes.Method
                };
            }
            return null;
        }

        public static Program.Resource[] GenerateResource(Type target, PropertyInfo prop)
        {
            Program.Resource[] output = new Program.Resource[2];
            CSharpAPI api = target.FindAPI();
            if (api != null)
            {
                output[0] = new Program.Resource()
                {
                    func = api.GetField(prop.Name),
                    name = prop.Name + "_get",
                    memberType = MemberTypes.Property
                };
                output[1] = new Program.Resource()
                {
                    func = api.SetField(prop.Name),
                    name = prop.Name + "_set",
                    memberType = MemberTypes.Property
                };

            }
            return output;
        }

        public static Program.Resource[] GenerateResource(Type target, FieldInfo field)
        {
            Program.Resource[] output = new Program.Resource[2];
            CSharpAPI api = target.FindAPI();
            if(api != null)
            {
                output[0] = new Program.Resource()
                {
                    func = api.GetField(field.Name),
                    name = field.Name + "_get",
                    memberType = MemberTypes.Field
                };
                output[1] = new Program.Resource()
                {
                    func = api.SetField(field.Name),
                    name = field.Name + "_set",
                    memberType = MemberTypes.Field
                };

            }
            return output;
        }

        public static Program.Resource[] GenerateResource(Type target, MemberInfo member)
        {
            if(member.MemberType == MemberTypes.Method)
            {
                return new Program.Resource[] {GenerateResource(target, (MethodInfo)member) };
            }
            else if (member.MemberType == MemberTypes.Property)
            {
                return GenerateResource(target, (PropertyInfo)member);
            }
            else if (member.MemberType == MemberTypes.Field)
            {
                return GenerateResource(target, (FieldInfo)member);
            }
            else
            {
                throw new Exception("unsupported type of resource");
            }
        }

        public static Program.Resource[] GenerateResource(Type target, string name)
        {
            List<Program.Resource> output = new List<Program.Resource>();
            MemberInfo[] members = GetMemberInfo(target, name);
            for(int i = 0; i < members.Length; ++i)
            {
                output.AddRange(GenerateResource(target, members[i]));
            }
            return output.ToArray();
        }

        public static string GetUniqueName(MethodInfo method)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(method.DeclaringType.FullName);
            sb.Append(".");
            sb.Append(method.Name);
            sb.Append("(");
            foreach(var arg in method.GetParameters())
            {
                if (sb[sb.Length - 1] != '(') sb.Append(", ");
                sb.Append(arg.ParameterType.FullName);
            }
            sb.Append(")");

            return sb.ToString();
        }
        public static string GetUniqueName(PropertyInfo prop)
        {
            return prop.DeclaringType + "." + prop.Name;
        }
        public static string GetUniqueName(FieldInfo field)
        {
            return field.DeclaringType + "." + field.Name;
        }


    }

}