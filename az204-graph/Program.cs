using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Core;
using Microsoft.Graph.Models;

internal class Program
{
    private static async global::System.Threading.Tasks.Task Main(string[] args)
    {
        var scopes = new[] { "User.Read" };

        // Multi-tenant apps can use "common",
        // single-tenant apps must use the tenant ID from the Azure portal
        var tenantId = "27882565-f13a-4eda-9153-5786e2237502";

        // Value from app registration
        var clientId = "8ab07d57-7ebe-496a-b2b3-788763d8b953";

        // using Azure.Identity;
        var options = new TokenCredentialOptions
        {
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
        };

        // Callback function that receives the user prompt
        // Prompt contains the generated device code that you must
        // enter during the auth process in the browser
        Func<DeviceCodeInfo, CancellationToken, Task> callback = (code, cancellation) =>
        {
            Console.WriteLine(code.Message);
            return Task.FromResult(0);
        };

        // /dotnet/api/azure.identity.devicecodecredential
        var deviceCodeCredential = new DeviceCodeCredential(
            callback, tenantId, clientId, options);

        var graphClient = new GraphServiceClient(deviceCodeCredential, scopes);

        // GET https://graph.microsoft.com/v1.0/me

        var user = await graphClient.Me
            .GetAsync();

        // GET https://graph.microsoft.com/v1.0/me/messages?$select=subject,sender&$filter=<some condition>&orderBy=receivedDateTime

        var messages = await graphClient.Me.Messages.GetAsync() ?? new MessageCollectionResponse() { OdataCount = -1, Value = null };

        if (messages.Value == null || messages.OdataCount == -1) throw new Exception();
        
        foreach(var message in messages.Value)
        {            
            Console.WriteLine(message.ToString());
        }

        // DELETE https://graph.microsoft.com/v1.0/me/messages/{message-id}

        /*string messageId = "";
        var message = await graphClient.Me.Messages[messageId]
            .Request()
            .DeleteAsync();*/

        // POST https://graph.microsoft.com/v1.0/me/calendars

        var calendar = new Calendar
        {
            Name = "Volunteer"
        };

        var newCalendar = await graphClient.Me.Calendars.PostAsync(calendar);
    }
}