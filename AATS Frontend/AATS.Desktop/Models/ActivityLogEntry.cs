using System;

namespace AATS.Desktop.Models
{
    public class ActivityLogEntry
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string User { get; set; } = "Admin User"; // Mocked for now
        private string _action = string.Empty;
        public string Action
        {
            get => _action;
            set => _action = value?.ToUpper() switch
            {
                "CREATE" => "Create",
                "UPDATE" => "Update",
                "DELETE" => "Delete",
                "LOGIN" => "Login",
                "AUTH" => "Login",
                "PRINT" => "Print",
                "EXPORT" => "Export",
                _ => value ?? string.Empty
            };
        }
        public string Module { get; set; } = string.Empty; // Team, Audit & Assurance, CIT, etc.
        public string Branch { get; set; } = "Central";
        public string Details { get; set; } = string.Empty;

        // Formatted Helpers for the UI
        public string TimestampFormatted => Timestamp.ToString("MMM dd, yyyy HH:mm");
        
        public string ActionIcon => Action switch
        {
            "Create" => "fa-solid fa-plus-circle",
            "Update" => "fa-solid fa-pen-to-square",
            "Delete" => "fa-solid fa-trash-can",
            "Login" => "fa-solid fa-right-to-bracket",
            "Print" => "fa-solid fa-print",
            "Export" => "fa-solid fa-file-export",
            _ => "fa-solid fa-circle-info"
        };

        public string ActionColor => Action switch
        {
            "Create" => "#10B981", // Emerald
            "Update" => "#3B82F6", // Blue
            "Delete" => "#EF4444", // Red
            "Login" => "#8B5CF6",  // Violet
            "Print" => "#F59E0B",  // Amber
            "Export" => "#06B6D4", // Cyan
            _ => "#94A3B8"
        };

        public string ModuleIcon => Module switch
        {
            "Audit & Assurance" => "fa-solid fa-file-invoice-dollar",
            "CIT" => "fa-solid fa-receipt",
            "Registration" => "fa-solid fa-briefcase",
            "Team" => "fa-solid fa-user-tie",
            "Auth" => "fa-solid fa-shield-halved",
            _ => "fa-solid fa-layer-group"
        };

        public bool IsCreate => Action == "Create";
        public bool IsUpdate => Action == "Update";
        public bool IsDelete => Action == "Delete";
        public bool IsLogin => Action == "Login" || Action == "Auth";
        public bool IsPrint => Action == "Print";
        public bool IsExport => Action == "Export";
    }
}
