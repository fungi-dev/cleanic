namespace Cleanic.Application
{
    using System;
    using System.Threading.Tasks;

    public interface IIdentityProvider
    {
        Task<String> ObtainToken(AuthorizationGrant authorizationGrant);
        Task<User> GetUser(String accessToken);
    }

    public abstract class AuthorizationGrant { }

    public class ClientCredentialsAuthorizationGrant : AuthorizationGrant
    {
        public String ClientId { get; set; }
        public String ClientSecret { get; set; }
    }

    public class ResourceOwnerPasswordCredentialsAuthorizationGrant : AuthorizationGrant
    {
        public String UserName { get; set; }
        public String Password { get; set; }
    }
}