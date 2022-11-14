using System.Globalization;

namespace Azure_Sync_Service
{
    public class AuthenticationConfig
    {
        public int HowOftenToSyncInHouers { get; set; }
        public string? AzureGroupObjectIdToSync { get; set; }
        public int DefaultDirectDepartment { get; set; }
        public string Instance { get; set; } = "https://login.microsoftonline.com/{0}";
        public string? Tenant { get; set; }
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }

        public string Authority => string.Format(CultureInfo.InvariantCulture, Instance, Tenant);
        public static AuthenticationConfig ReadFromJsonFile(string path)
        {
            var builder = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(path);

            var Configuration = builder.Build();
            return Configuration.Get<AuthenticationConfig>();
        }
    }
}

