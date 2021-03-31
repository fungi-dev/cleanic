namespace Cleanic.Core
{
    using System;

    public class TermInfo
    {
        public Type Type { get; }
        public String Name { get; }
        public String FullName { get; }

        public TermInfo(Type termType)
        {
            Type = termType ?? throw new ArgumentNullException(nameof(termType));
            Name = termType.Name;
            FullName = termType.FullName.Replace("+", ".");
        }

        public override String ToString() => Name;
    }
}