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
            Delegate del = MethodToOpenDelegate(target, method);
            ParameterInfo[] argInfo = method.GetParameters();
            Program.Resource output = new Program.Resource(del, argInfo.Length+1);
            output.returnType = method.ReturnType;
            output.name = target.FullName + "." + method.Name;
            output.memberType = MemberTypes.Method;
            return output;
        }

        public static Program.Resource GenerateResource(Type target, PropertyInfo prop)
        {
            Delegate getDel = prop.CanRead ? MethodToOpenDelegate(target, prop.GetGetMethod()) : null;
            Delegate setDel = prop.CanWrite ? MethodToOpenDelegate(target, prop.GetSetMethod()) : null;
            Program.Resource output = new Program.Resource(getDel, setDel, 2,3);
            output.returnType = prop.PropertyType;
            output.name = target.FullName + "." + prop.Name;
            output.memberType = MemberTypes.Property;
            return output;
        }

        public static Program.Resource GenerateResource(Type target, FieldInfo field)
        {

            MethodInfo getMethod = field.GetType().GetMethod("GetValue");
            Delegate getDel = MethodToInstanceDelegate(field, getMethod);

            MethodInfo setMethod = field.GetType().GetMethod("SetValue");
            Delegate setDel = MethodToInstanceDelegate(field, setMethod);

            Program.Resource output = new Program.Resource(getDel, setDel, 2,3);
            output.returnType = field.FieldType;
            output.name = target.FullName + "." + field.Name;
            output.memberType = MemberTypes.Field;

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

        public static Program.Resource[] GenerateResource(Type target, string name)
        {
            MemberInfo[] members = GetMemberInfo(target, name);
            Program.Resource[] output = new Program.Resource[members.Length];
            for(int i = 0; i < members.Length; ++i)
            {
                output[i] = GenerateResource(target, members[i]);
            }
            return output;
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