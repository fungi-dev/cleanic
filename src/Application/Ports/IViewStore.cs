namespace Cleanic.Application
{
    using Cleanic.Core;
    using System;
    using System.Threading.Tasks;

    public interface IViewStore
    {
        Task<AggregateView> Load(AggregateViewInfo aggregateViewInfo, String aggregateId);
        Task Save(AggregateView aggregateView);
    }
}