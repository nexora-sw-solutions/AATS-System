using System;
using System.Collections.ObjectModel;
using AATS.Desktop.Models;
using AATS.Desktop.Data;

namespace AATS.Desktop.Services
{
    public class LogService
    {
        private static LogService? _instance;
        public static LogService Instance => _instance ??= new LogService();

        public ObservableCollection<ActivityLogEntry> Logs { get; } = new();

        private LogService()
        {
            _ = LoadLogsFromBackendAsync();
        }

        public void AddLog(string action, string module, string branch, string details)
        {
            var entry = new ActivityLogEntry
            {
                Action = action,
                Module = module,
                Branch = branch,
                Details = details,
                Timestamp = DateTime.Now
            };

            Logs.Insert(0, entry);

            _ = DataService.Instance.AddActivityLogAsync(action, module, branch, details);
        }

        public async System.Threading.Tasks.Task LoadLogsFromBackendAsync()
        {
            try
            {
                var backendLogs = await DataService.Instance.GetActivityLogsAsync();
                if (backendLogs != null && backendLogs.Count > 0)
                {
                    Logs.Clear();
                    foreach (var log in backendLogs)
                    {
                        Logs.Add(log);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error synchronizing backend logs in LogService: {ex.Message}");
            }
        }
    }
}
