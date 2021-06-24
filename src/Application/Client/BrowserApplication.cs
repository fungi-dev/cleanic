namespace Cleanic.Application
{
    using System;
    using System.Threading.Tasks;

    public class BrowserApplication : ClientApplication
    {
        public User User { get; private set; }

        public BrowserApplication(IServerSdk server, IIdentityProvider identityProvider) : base(server, identityProvider) { }

        public async Task Login(String userName, String password)
        {
            var authGrant = new ResourceOwnerPasswordCredentialsAuthorizationGrant { UserName = userName, Password = password };
            AccessToken = await IdentityProvider.ObtainToken(authGrant);

            User = await IdentityProvider.GetUser(AccessToken);

            await UpdateStateAfterLogin();
        }

        protected virtual Task UpdateStateAfterLogin() => Task.CompletedTask;
    }
}