﻿using System;

namespace Cleanic.DomainInfo
{
    public class AggregateInfo
    {
        public AggregateInfo(Type aggregateType)
        {
            Type = aggregateType;
        }

        public Type Type { get; }
    }
}