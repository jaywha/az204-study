using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace az204_auth
{
    public class Program
    {
        private const string _clientId = "f5c3d77d-f9db-46a7-b28d-3dd1ffac7c6d";
        private const string _tenantId = "27882565-f13a-4eda-9153-5786e2237502";

        public static async Task Main(string[] args)
        {
            var app = PublicClientApplicationBuilder
                .Create(_clientId)
                .WithAuthority(AzureCloudInstance.AzurePublic, _tenantId)
                .WithRedirectUri("http://localhost")
                .Build();

            string[] scopes = ["user.read"];

            AuthenticationResult result = await app.AcquireTokenInteractive(scopes).ExecuteAsync();

            Console.WriteLine($"Token:\t{result.AccessToken}");
            Console.WriteLine($"Args: {args}");
        }
    }
}