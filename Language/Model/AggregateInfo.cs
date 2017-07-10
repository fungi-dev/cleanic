using System;
using System.Linq;
using System.Reflection;

namespace FrogsTalks
{
    public class AggregateInfo : DomainObjectInfo
    {
        public CommandInfo[] Commands { get; }

        public override string DomainType => "Aggregate";

        protected virtual Func<TypeInfo[]> CommandsFinder
        {
            get
            {
                return () =>
                {
                    var methods = Type.DeclaredMethods;
                    var parameters = methods.SelectMany(p => p.GetParameters()).Select(p => p.ParameterType.GetTypeInfo());
                    var commands = parameters.Where(t => t.ImplementedInterfaces.Contains(typeof(ICommand)));
                    return commands.Distinct().ToArray();
                };
            }
        }

        public AggregateInfo(TypeInfo type, string context = "") : base(type, context)
        {
            Commands = CommandsFinder().Select(x => new CommandInfo(x, this)).ToArray();
        }
    }
}