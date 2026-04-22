using System;
using System.IO;
using System.Text.Json;

namespace AATS.Desktop.Services
{
    public class DesktopAppSettings
    {
        public BackendSettings Backend { get; set; } = new();
        public ClientsApiSettings ClientsApi { get; set; } = new();
        public SupabaseSettings Supabase { get; set; } = new();

        public static DesktopAppSettings Load()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (!File.Exists(path))
            {
                return new DesktopAppSettings();
            }

            try
            {
                var json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<DesktopAppSettings>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new DesktopAppSettings();
            }
            catch
            {
                return new DesktopAppSettings();
            }
        }
    }

    public class BackendSettings
    {
        public string BaseUrl { get; set; } = "http://localhost:5561";
        public int TimeoutSeconds { get; set; } = 15;
        public bool UseMockFallback { get; set; } = true;
    }

    public class SupabaseSettings
    {
        public string Url { get; set; } = string.Empty;
        public string AnonKey { get; set; } = string.Empty;
    }

    public class ClientsApiSettings
    {
        public string BaseUrl { get; set; } = "http://localhost:5000";
        public string AccessToken { get; set; } = string.Empty;
    }
}
