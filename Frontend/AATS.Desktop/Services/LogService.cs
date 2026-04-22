using System;
using System.Collections.ObjectModel;
using AATS.Desktop.Models;
using Avalonia.Threading;

namespace AATS.Desktop.Services
{
    public class LogService
    {
        private static LogService? _instance;
        public static LogService Instance => _instance ??= new LogService();

        public ObservableCollection<ActivityLogEntry> Logs { get; } = new();

        private LogService()
        {
            _ = RefreshFromBackendAsync();
        }

        public void AddLog(string action, string module, string branch, string details)
        {
            var entry = new ActivityLogEntry
            {
                Action = action,
                Module = module,
                Branch = branch,
                Details = details,
                Timestamp = DateTime.Now,
                User = DataService.Instance.CurrentUser.Username ?? "Admin User"
            };

            Logs.Insert(0, entry);
            _ = DataService.Instance.AddActivityLogAsync(entry);
        }

        private async System.Threading.Tasks.Task RefreshFromBackendAsync()
        {
            var logs = await DataService.Instance.GetActivityLogsAsync();
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Logs.Clear();
                foreach (var log in logs)
                {
                    Logs.Add(log);
                }
            });
        }
    }
}
