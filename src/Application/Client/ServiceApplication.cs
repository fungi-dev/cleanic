namespace Cleanic.Application
{
    using System;
    using System.Threading.Tasks;

    public abstract class ServiceApplication : ClientApplication
    {
        public ServiceApplication(IServerSdk server, IIdentityProvider identityProvider) : base(server, identityProvider) { }

        public async Task Authenticate()
        {
            var authGrant = new ClientCredentialsAuthorizationGrant
            {
                ClientId = GetClientId(),
                ClientSecret = GetClientSecret()
            };
            AccessToken = await IdentityProvider.ObtainToken(authGrant);
        }

        protected abstract String GetClientId();
        protected abstract String GetClientSecret();
    }
}