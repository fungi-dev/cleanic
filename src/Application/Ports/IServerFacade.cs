namespace Cleanic.Application
{
    using Cleanic.Core;
    using System;
    using System.Threading.Tasks;

    public interface IServerFacade
    {
        Task<AggregateView> Get(Query query);
        Task Do(Command command);
        void ListenViewUpdates(AggregateViewInfo aggregateViewInfo, Func<AggregateView, Task> listener);
    }
}