using System;

namespace AATS.Desktop.Models
{
    public class ApiDocument
    {
        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public Guid Id { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("recordType")]
        public string RecordType { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("recordId")]
        public Guid RecordId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("fileName")]
        public string FileName { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("storageKey")]
        public string StorageKey { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("category")]
        public string? Category { get; set; }
    }
}
