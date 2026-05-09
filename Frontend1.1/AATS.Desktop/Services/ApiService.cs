using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;
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
            Client.BaseAddress = new Uri("http://localhost:5152");
        }

        public void SetToken(string token)
        {
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<T?> GetAsync<T>(string uri)
        {
            var response = await Client.GetAsync(uri);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[DEBUG] GET {uri} failed ({(int)response.StatusCode}): {errorBody}");
                throw new Exception($"Server error ({(int)response.StatusCode}): {errorBody}");
            }
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task PostAsync<TRequest>(string uri, TRequest data)
        {
            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            var json = JsonSerializer.Serialize(data, options);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(uri, content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"Server error ({(int)response.StatusCode}): {errorBody}");
            }
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string uri, TRequest data)
        {
            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var json = JsonSerializer.Serialize(data, options);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(uri, content);
            
            var responseBody = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[DEBUG] POST {uri} failed ({(int)response.StatusCode}): {responseBody}");
                throw new Exception($"Server error ({(int)response.StatusCode}): {responseBody}");
            }

            try 
            {
                if (!string.IsNullOrWhiteSpace(responseBody))
                {
                    return JsonSerializer.Deserialize<TResponse>(responseBody, options);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Error deserializing POST response from {uri}: {ex.Message}");
            }

            return default;
        }


        public async Task<TResponse?> PutAsync<TRequest, TResponse>(string uri, TRequest data)
        {
            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var json = JsonSerializer.Serialize(data, options);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await Client.PutAsync(uri, content);
            
            var responseBody = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                // Try to parse structured error
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseBody, options);
                    if (errorResponse?.Error != null)
                    {
                        throw new Exception(errorResponse.Error.Message);
                    }
                }
                catch (Exception ex) when (ex.Message != null && !ex.Message.Contains("Server error")) 
                { 
                    throw; 
                }
                
                throw new Exception($"Server error ({(int)response.StatusCode}): {responseBody}");
            }

            try 
            {
                if (!string.IsNullOrWhiteSpace(responseBody))
                {
                    return JsonSerializer.Deserialize<TResponse>(responseBody, options);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Error deserializing PUT response from {uri}: {ex.Message}");
            }

            return default;
        }

    public async Task PutAsync<TRequest>(string uri, TRequest data)
    {
        var options = new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        var json = JsonSerializer.Serialize(data, options);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await Client.PutAsync(uri, content);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new Exception($"Server error ({(int)response.StatusCode}): {errorBody}");
        }
    }

    public async Task DeleteAsync(string uri)
        {
            var response = await Client.DeleteAsync(uri);
            response.EnsureSuccessStatusCode();
        }
    }
}
