using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Cleanic.Core
{
    public static class QueryTypeExtensions
    {
        public static Boolean IsQuery(this Type type) => type.GetTypeInfo().IsQuery();
        public static Boolean IsQuery(this TypeInfo type) => type.AsType() == typeof(IQuery) || type.ImplementedInterfaces.Contains(typeof(IQuery));
        public static Boolean IsQueryCollection(this Type type) => type.GetTypeInfo().IsQueryCollection();
        public static Boolean IsQueryCollection(this TypeInfo type)
        {
            if (type.IsSubclassOf(typeof(Task))) type = type.GenericTypeArguments[0].GetTypeInfo();
            if (!type.IsArray) return false;
            return type.GetElementType().IsQuery();
        }
    }
}