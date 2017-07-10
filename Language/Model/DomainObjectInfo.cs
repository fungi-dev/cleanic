using System;
using System.Reflection;

namespace FrogsTalks
{
    public abstract class DomainObjectInfo
    {
        public abstract string DomainType { get; }

        public string Context { get; }

        public string Name { get; }

        public Guid Id { get; }

        public TypeInfo Type { get; }

        public DomainObjectInfo(TypeInfo type, string context = "")
        {
            Id = type.GUID;
            Type = type;
            Context = context;
            Name = NameBuilder();
        }

        protected virtual Func<string> NameBuilder => () => Type.Name.Replace(DomainType, null);

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Context)) return $"{DomainType} {Name}";
            return $"{DomainType} {Context}.{Name}";
        }
    }
}