using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace AATS.Domain.Entities
{
    public class StaffHistoryItem
    {
        public DateTime Date { get; set; }
        public string? Description { get; set; }
        public decimal Amount { get; set; }
    }

    public class StaffMember : BaseEntity
    {
        public Guid RecordId { get; set; }
        public AuditRecord? Record { get; set; }

        public string? StaffCode { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string ProcessStatus { get; set; } = "PROCESSING";

        public string? HistoryJson { get; set; } = "[]";

        [NotMapped]
        public List<StaffHistoryItem> History
        {
            get => string.IsNullOrEmpty(HistoryJson) ? new List<StaffHistoryItem>() : JsonSerializer.Deserialize<List<StaffHistoryItem>>(HistoryJson) ?? new List<StaffHistoryItem>();
            set => HistoryJson = JsonSerializer.Serialize(value ?? new List<StaffHistoryItem>());
        }
    }
}
