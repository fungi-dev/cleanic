using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Cleanic.Core
{
    public static class DomainTypesReflection
    {
        public static Boolean Is<T>(this ParameterInfo parameterInfo) => parameterInfo.ParameterType.Is<T>();

        public static Boolean Is<T>(this Type type) => type.GetTypeInfo().Is<T>();
        public static Boolean Is<T>(this TypeInfo type) => typeof(T).GetTypeInfo().IsAssignableFrom(type);
        public static Boolean IsCollection<T>(this Type type) => type.GetTypeInfo().IsCollection<T>();
        public static Boolean IsCollection<T>(this TypeInfo type) => type.IsArray && type.GetElementType().Is<T>();

        public static Boolean Returns<T>(this MethodInfo methodInfo)
        {
            var t = methodInfo.ReturnType;
            if (t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition() == typeof(Task<>)) t = t.GenericTypeArguments[0];
            return t.Is<T>() || t.IsCollection<T>();
        }

        public static Type GetReturnDomainType(this MethodInfo methodInfo)
        {
            var t = methodInfo.ReturnType;
            if (t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition() == typeof(Task<>)) t = t.GenericTypeArguments[0];
            return t.IsArray ? t.GetElementType() : t;
        }
    }
}