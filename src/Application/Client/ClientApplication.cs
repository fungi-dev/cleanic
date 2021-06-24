namespace Cleanic.Application
{
    using Cleanic.Core;
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;

    public abstract class ClientApplication
    {
        public ClientApplication(IServerSdk server, IIdentityProvider identityProvider)
        {
            Server = server ?? throw new ArgumentNullException(nameof(server));
            Language = server.Language;

            IdentityProvider = identityProvider ?? throw new ArgumentNullException(nameof(identityProvider));

            _contextMenuItems = new List<MenuItem>();
        }

        public LanguageSchema Language { get; }
        public IReadOnlyCollection<MenuItem> ContextMenu => _contextMenuItems.ToImmutableHashSet();
        public Message Output { get; protected set; }
        public Message Input { get; protected set; }

        public async Task SelectItemInView(Expression<Func<AggregateView, Object>> viewItemPointer)
        {
            var dataItem = viewItemPointer.Compile().Invoke(Output as AggregateView);
            var query = BuildQueryFromViewItem(dataItem);
            await SendMessageToServer(query);
        }

        public async Task SelectItemFromContextMenu<T>() where T : Message, new()
        {
            if (!_contextMenuItems.Any(x => x.AssociatedMessage.Type == typeof(T))) throw new ArgumentOutOfRangeException(nameof(T));

            Input = new T { AggregateId = Output.AggregateId };
            Output = null;

            await OnCommandIntent(Input as Command);
        }

        public Task Submit() => SendMessageToServer(Input);

        protected readonly IServerSdk Server;
        protected readonly IIdentityProvider IdentityProvider;
        protected String AccessToken;

        protected virtual Task OnCommandIntent(Command command) => Task.CompletedTask;
        protected virtual Task OnMessageSending(Message message) => Task.CompletedTask;
        protected virtual Task OnStateChanged(Message message) => Task.CompletedTask;

        protected async Task SendMessageToServer(Message message)
        {
            await OnMessageSending(message);
            Input = null;
            Output = null;

            switch (message)
            {
                case Command cmd:
                    await Server.Do(cmd, AccessToken);
                    break;

                case Query qr:
                    Output = await Server.Get(qr, AccessToken);
                    break;
            }

            _contextMenuItems.Clear();
            await OnStateChanged(message);
        }

        protected void AddContextMenuItem<T>() where T : Message
        {
            var messageInfo = Language.GetMessage(typeof(T));
            if (_contextMenuItems.Any(x => x.AssociatedMessage == messageInfo)) throw new ArgumentException($"'{messageInfo.Name}' already presented in context menu");
            _contextMenuItems.Add(new MenuItem(messageInfo));
        }

        private readonly List<MenuItem> _contextMenuItems;

        private Query BuildQueryFromViewItem(Object viewDataItem)
        {
            var methods = GetType().GetRuntimeMethods().Where(x => x.GetParameters().Length == 1 && x.ReturnType.IsSubclassOf(typeof(Query)));
            var queryCreator = methods.SingleOrDefault(x => x.GetParameters()[0].ParameterType == viewDataItem.GetType());
            if (queryCreator == null)
            {
                var er = $"'{GetType().FullName}' doesn't contain method for create and fill query when user selects '{viewDataItem.GetType().FullName}'";
                throw new NotImplementedException(er);
            }
            return (Query)queryCreator.Invoke(this, new Object[] { viewDataItem });
        }
    }

    public class MenuItem
    {
        public MessageInfo AssociatedMessage { get; }

        public MenuItem(MessageInfo associatedMessage)
        {
            AssociatedMessage = associatedMessage;
        }
    }
}