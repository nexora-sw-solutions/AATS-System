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
            // Seed sample data from central MockData source
            foreach (var log in MockData.ActivityLogs)
            {
                Logs.Add(log);
            }
        }

        public void AddLog(string action, string module, string branch, string details)
        {
            Logs.Insert(0, new ActivityLogEntry
            {
                Action = action,
                Module = module,
                Branch = branch,
                Details = details,
                Timestamp = DateTime.Now
            });
        }
    }
}
