namespace Cleanic.Application
{
    using Cleanic.Core;
    using System;
    using System.Threading.Tasks;

    public interface IServerSdk
    {
        LanguageSchema Language { get; }
        Task<View> Get(Query query, String accessToken);
        Task Do(Command command, String accessToken);
        void ListenViewUpdates(ViewInfo viewInfo, Func<View, Task> listener);
    }
}