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
            Client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            Client.DefaultRequestHeaders.Add("Pragma", "no-cache");
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
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            var json = JsonSerializer.Serialize(data, options);
            Console.WriteLine($"[DEBUG] POST {uri} - Body: {json}");
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(uri, content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[DEBUG] POST {uri} failed ({(int)response.StatusCode}): {errorBody}");
                throw new Exception($"Server error ({(int)response.StatusCode}): {errorBody}");
            }
            Console.WriteLine($"[DEBUG] POST {uri} successful.");
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string uri, TRequest data)
        {
            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
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
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
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

            Console.WriteLine($"[DEBUG] PUT {uri} successful. Response: {responseBody}");
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
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
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

        /// <summary>
        /// Uploads a client logo to the backend which stores it in Cloudflare R2.
        /// Returns the public URL of the uploaded image.
        /// </summary>
        public async Task<string> UploadLogoAsync(string localFilePath, string clientId)
        {
            if (!System.IO.File.Exists(localFilePath))
                throw new System.IO.FileNotFoundException("Logo file not found", localFilePath);

            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(clientId), "clientId");

            var bytes = await System.IO.File.ReadAllBytesAsync(localFilePath);
            var fileContent = new ByteArrayContent(bytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(GetMimeType(localFilePath));
            form.Add(fileContent, "file", System.IO.Path.GetFileName(localFilePath));

            var response = await Client.PostAsync("/api/upload/logo", form);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[DEBUG] UploadLogo failed ({(int)response.StatusCode}): {body}");
                throw new Exception($"Logo upload failed ({(int)response.StatusCode}): {body}");
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<Dictionary<string, string>>(body, options);
            if (result != null && result.TryGetValue("url", out var url))
            {
                return url;
            }
            throw new Exception("Invalid response from logo upload endpoint");
        }

        /// <summary>
        /// Uploads one or more local files to the backend which stores them in Cloudflare R2.
        /// Returns a list of SourceDocument with the remote Url populated.
        /// </summary>
        public async Task<List<AATS.Desktop.Models.SourceDocument>> UploadDocumentsAsync(
            List<string> localFilePaths,
            string recordType,
            string recordId)
        {
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(recordType), "recordType");
            form.Add(new StringContent(recordId), "recordId");

            foreach (var path in localFilePaths)
            {
                if (!System.IO.File.Exists(path)) continue;
                var bytes = await System.IO.File.ReadAllBytesAsync(path);
                var fileContent = new ByteArrayContent(bytes);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(GetMimeType(path));
                form.Add(fileContent, "files", System.IO.Path.GetFileName(path));
            }

            var response = await Client.PostAsync("/api/upload/documents", form);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[DEBUG] UploadDocuments failed ({(int)response.StatusCode}): {body}");
                throw new Exception($"Upload failed ({(int)response.StatusCode}): {body}");
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<List<AATS.Desktop.Models.SourceDocument>>(body, options)
                   ?? new List<AATS.Desktop.Models.SourceDocument>();
        }

        /// <summary>
        /// Downloads a file from a remote URL and saves it to the user's active Downloads folder.
        /// </summary>
        public async Task DownloadDocumentAsync(string fileUrl, string fileName)
        {
            var bytes = await Client.GetByteArrayAsync(fileUrl);
            
            bool dialogSaved = false;
            try
            {
                if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop &&
                    desktop.MainWindow != null)
                {
                    var topLevel = Avalonia.Controls.TopLevel.GetTopLevel(desktop.MainWindow);
                    if (topLevel != null)
                    {
                        var ext = System.IO.Path.GetExtension(fileName);
                        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new Avalonia.Platform.Storage.FilePickerSaveOptions
                        {
                            Title = "Save Document",
                            SuggestedFileName = fileName,
                            DefaultExtension = ext?.TrimStart('.'),
                            ShowOverwritePrompt = true
                        });
                        
                        if (file != null)
                        {
                            await using var stream = await file.OpenWriteAsync();
                            await stream.WriteAsync(bytes, 0, bytes.Length);
                            dialogSaved = true;
                            
                            var localPath = file.Path.LocalPath;
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(localPath) { UseShellExecute = true });
                        }
                        else
                        {
                            // User cancelled the save dialog. Do not save/open.
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Save file dialog failed, falling back: {ex.Message}");
            }

            if (!dialogSaved)
            {
                string downloadsPath;
                try
                {
                    if (OperatingSystem.IsWindows())
                    {
                        // Query User Shell Folders for Downloads path (handles redirections/OneDrive)
                        downloadsPath = Microsoft.Win32.Registry.GetValue(
                            @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders",
                            "{7D1C3B02-7D38-4E90-937D-B05463014C47}",
                            null) as string 
                            ?? Microsoft.Win32.Registry.GetValue(
                                @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders",
                                "{374DE290-123F-4565-9164-39C4925E467B}",
                                null) as string
                            ?? System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                        
                        downloadsPath = Environment.ExpandEnvironmentVariables(downloadsPath);
                    }
                    else
                    {
                        downloadsPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                    }
                }
                catch
                {
                    downloadsPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                }

                System.IO.Directory.CreateDirectory(downloadsPath);
                var savePath = System.IO.Path.Combine(downloadsPath, fileName);
                await System.IO.File.WriteAllBytesAsync(savePath, bytes);
                // Open the file after download
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(savePath) { UseShellExecute = true });
            }
        }

        private static string GetMimeType(string path)
        {
            return System.IO.Path.GetExtension(path).ToLowerInvariant() switch
            {
                ".pdf"  => "application/pdf",
                ".doc"  => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls"  => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png"  => "image/png",
                ".zip"  => "application/zip",
                _       => "application/octet-stream"
            };
        }
    }
}
