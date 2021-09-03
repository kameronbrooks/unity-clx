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
            Debug.Log($"Generating Method Resource: {target.FullName}::{method.Name}");
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
                    func = api.Method(method.Name),
                    name = method.Name,
                    memberType = MemberTypes.Method,
                    args = args,
                    returnType = method.ReturnType
                };

                return output;
            }
            return null;
        }

        public static Program.Resource GenerateResource(Type target, PropertyInfo prop)
        {
            Debug.Log($"Generating Property Resource: {target.FullName}::{prop.Name}");
            Program.Resource output = new Program.Resource()
            {
                name = prop.Name,
                func = new CSharpAPI.API_CALL[2],
                memberType = MemberTypes.Property,
                returnType = prop.PropertyType
            };

            CSharpAPI api = target.FindAPI();
            if (api != null)
            {
                output.func = api.Property(prop.Name);
            }
            return output;
        }

        public static Program.Resource GenerateResource(Type target, FieldInfo field)
        {
            Program.Resource output = new Program.Resource()
            {
                name = field.Name,
                func = new CSharpAPI.API_CALL[2],
                memberType = MemberTypes.Property,
                returnType = field.FieldType
                
            };

            CSharpAPI api = target.FindAPI();
            if (api != null)
            {
                output.func = api.Field(field.Name);
            }
            return output;
        }

        public static Program.Resource GenerateResource(Type target, MemberInfo member)
        {
            if(member.MemberType == MemberTypes.Method)
            {
                return GenerateResource(target, (MethodInfo)member);
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

        public static Program.Resource GenerateResource(Type target, string name)
        {
            /*
            Debug.Log($"Generating resource {target.FullName}::{name}");
            List<Program.Resource> output = new List<Program.Resource>();
            MemberInfo[] members = GetMemberInfo(target, name);
            Debug.Log(members.Length);
            if(members.Length == 1)
            {
                return GenerateResource(target, members[0]);
            }
            */
            List<Program.Resource> output = new List<Program.Resource>();
            CSharpAPI api = target.FindAPI();

            if(api == null)
            {
                Debug.Log($"No API found for type:{target.FullName}");
                return null;
            }

            return api.GenerateResource(name);
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

        public static System.Type CreateFunctionType(params Type[] args)
        {
            return typeof(System.Func<>).MakeGenericType(args);
        }
        public static System.Type CreateFunctionType(Type returnType, params Type[] args)
        {
            Type[] totalParams = new Type[args.Length + 1];
            totalParams[0] = returnType;
            for(int i = 0; i < args.Length; ++i)
            {
                totalParams[i + 1] = args[i];
            }
            return typeof(System.Func<>).MakeGenericType(totalParams);
        }


    }

}