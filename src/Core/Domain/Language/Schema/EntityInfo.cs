namespace Cleanic.Core
{
    using System;
    using System.Collections.Generic;

    public class EntityInfo : DomainObjectInfo
    {
        public IReadOnlyCollection<CommandInfo> Commands { get; internal set; }
        public IReadOnlyCollection<ViewInfo> Views { get; internal set; }

        public EntityInfo(Type entityType) : base(entityType)
        {
            EnsureTermTypeCorrect(entityType, typeof(Entity));
        }
    }
}