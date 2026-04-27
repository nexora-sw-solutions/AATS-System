using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace AATS.Desktop.Services
{
    public class ApiError
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public ApiError? Error { get; set; }
    }

    public class PaginatedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
    }

    public class ApiService
    {
        private static ApiService? _instance;
        public static ApiService Instance => _instance ??= new ApiService();

        public HttpClient Client { get; }

        private ApiService()
        {
            Client = new HttpClient();
            // Reverted to the actual port the user is running the backend on (5000)
            Client.BaseAddress = new Uri("http://localhost:5000");
        }

        public void SetToken(string token)
        {
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<T?> GetAsync<T>(string uri)
        {
            var response = await Client.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[DEBUG] GET {uri} returned: {json}");
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string uri, TRequest data)
        {
            var response = await Client.PostAsJsonAsync(uri, data);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            
            try 
            {
                var json = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(json))
                {
                    return JsonSerializer.Deserialize<TResponse>(json, options);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Error deserializing POST response from {uri}: {ex.Message}");
            }

            if (!response.IsSuccessStatusCode)
            {
                response.EnsureSuccessStatusCode();
            }

            return default;
        }

        public async Task PostAsync<TRequest>(string uri, TRequest data)
        {
            var response = await Client.PostAsJsonAsync(uri, data);
            response.EnsureSuccessStatusCode();
        }

        public async Task PutAsync<TRequest>(string uri, TRequest data)
        {
            var response = await Client.PutAsJsonAsync(uri, data);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(string uri)
        {
            var response = await Client.DeleteAsync(uri);
            response.EnsureSuccessStatusCode();
        }
    }
}
