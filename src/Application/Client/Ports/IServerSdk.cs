namespace Cleanic.Application
{
    using Cleanic.Core;
    using System;
    using System.Threading.Tasks;

    public interface IServerSdk
    {
        LanguageSchema Language { get; }
        Task<AggregateView> Get(Query query, String accessToken);
        Task Do(Command command, String accessToken);
        void ListenViewUpdates(AggregateViewInfo aggregateViewInfo, Func<AggregateView, Task> listener);
    }
}