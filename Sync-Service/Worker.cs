using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Azure_Sync_Service;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private AuthenticationConfig config;
    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
   
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var azureUserList = new List<User>();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            config = AuthenticationConfig.ReadFromJsonFile("appsettings.json");
            var app = ConfidentialClientApplicationBuilder.Create(config.ClientId)
                .WithClientSecret(config.ClientSecret)
                .WithAuthority(new Uri(config.Authority))
                .Build();
            
           
            var scopes = new[] { "https://graph.microsoft.com/.default" }/* { "User.Read", "User.ReadBasic.All" }*/;

            AuthenticationResult? result = null;
            try
            {
                result = await app.AcquireTokenForClient(scopes).ExecuteAsync();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Token acquired");
                Console.ResetColor();
            }
            catch (MsalServiceException ex)
            {
                //TODO: Handle exception | logg?
                Console.ForegroundColor = ConsoleColor.Red;  
                Console.WriteLine("Error Acquiring Token:");
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
           
            if (result != null)
            {
                var httpClient = new HttpClient();
                var apiCaller = new ProtectedApiCallHelper(httpClient);
                var userResult = await apiCaller.CallWebApiAndProcessResultASync($"https://graph.microsoft.com/v1.0/groups/{config.AzureGroupObjectIdToSync}/members", result.AccessToken);

                if (userResult != null)
                {
                    azureUserList = GetUserList(userResult);
                }

                if (azureUserList != null)
                    foreach (var user in azureUserList)
                    {

                    }
            }

            _logger.LogInformation("Worker running at: {Time}", DateTimeOffset.Now);

            var syncIntervalInMS = 1000 * 60 * 60 * config.HowOftenToSyncInHouers;
            await Task.Delay(syncIntervalInMS, stoppingToken);
        }
    }

    private List<User>? GetUserList(JObject result)
    {
        foreach (var child in result.Properties().Where(p => !p.Name.StartsWith("@")))
        {
            return JsonConvert.DeserializeObject<List<User>>(child.Value.ToString());
        }
        return null;
    }

    private string getdummyData()
    {
        var res = new Random();
        const string str = "abcdefghijklmnopqrstuvwxyzøæå123456789";
        const int size = 20;
        var randomString = "";
  
        for (var i = 0; i < size; i++)
        {
            var x = res.Next(37);
  
            randomString += str[x];
        }

        return randomString;
    } }