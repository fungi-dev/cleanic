namespace Cleanic.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    public abstract class Saga : DomainObject
    {
        public async Task<Command[]> Handle(AggregateEvent @event)
        {
            var methods = GetType().GetTypeInfo().DeclaredMethods
                .Where(x => !x.IsStatic)
                .Where(x => x.GetParameters().Length == 1)
                .Where(x => x.GetParameters()[0].ParameterType == @event.GetType());

            var commands = new List<Command>();
            foreach (var method in methods)
            {
                var cmds = await (Task<Command[]>)method.Invoke(this, new[] { @event });
                commands.AddRange(cmds);
            }

            return commands.ToArray();
        }

        protected override IEnumerable<Object> GetIdentityComponents()
        {
            yield return GetType();
        }
    }
}