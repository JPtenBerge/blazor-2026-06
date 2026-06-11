using Duende.IdentityServer.Models;

namespace IdentityProvidert
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope("scope1"),
                new ApiScope("scope2"),
            };

        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                // interactive client using code flow + pkce
                new Client
                {
                    ClientId = "blazorfrontend",
                    ClientSecrets = { new Secret("304F15BB-6C25-4BCC-A741-0BED7C0DCB10".Sha256()) },

                    AllowedGrantTypes = GrantTypes.Code,

                    RedirectUris = { "https://localhost:5005/signin-oidc" },
                    FrontChannelLogoutUri = "https://localhost:5005/signout-oidc",
                    PostLogoutRedirectUris = { "https://localhost:5005/signout-callback-oidc" },

                    AllowOfflineAccess = true,
                    AllowedScopes = { "openid", "profile", "scope2" }
                },
            };
    }
}
