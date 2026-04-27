using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AATS.Desktop.Models
{
    public partial class AuditRecord : ObservableObject
    {
        [ObservableProperty]
        private bool _isSelected;

        [ObservableProperty]
        private int _currentStep; // 1 to 6
        
        [ObservableProperty] private string? _iD;
        
        [ObservableProperty] 
        [property: JsonPropertyName("recordDate")]
        private DateTime _date;
        
        [ObservableProperty] private string? _clientName;
        [ObservableProperty] private string? _company;
        [ObservableProperty] private string? _paymentStatus; // Paid, Unpaid, Partial
        [ObservableProperty] private string? _process; // BOOKKEEP, DRAFT, FINALIZE, etc.
        
        public bool HasProcess => !string.IsNullOrWhiteSpace(Process) && Process != "-";
        public bool HasNoProcess => !HasProcess;
        
        [ObservableProperty] private string? _paymentOption; // Online, Cash, etc.
        [ObservableProperty] private string? _assignment;
        
        [ObservableProperty] 
        [property: JsonPropertyName("branchName")]
        private string? _branch;
        
        [ObservableProperty] private int _noOfStaffs;
        [ObservableProperty] private string? _country;
        [ObservableProperty] private string? _notes;
        [ObservableProperty] private string? _period; // e.g. "2024 Year", "Jan 2024"
        [ObservableProperty] private string? _tIN;
        [ObservableProperty] private string? _directorID;
        [ObservableProperty] private string? _investmentValue;
        [ObservableProperty] private string? _countryAddress;
        
        [ObservableProperty] 
        [property: JsonPropertyName("recordCode")]
        private string? _code;
        
        // Secretarial Advisory Extensions
        [ObservableProperty] private string? _address;
        [ObservableProperty] private string? _email;
        [ObservableProperty] private string? _phoneNo;
        [ObservableProperty] private string? _objective;
        [ObservableProperty] private string? _description;

        [ObservableProperty] private List<CompanyCharacter>? _directorsList;
        [ObservableProperty] private List<CompanyCharacter>? _secretariesList;
        [ObservableProperty] private List<CompanyCharacter>? _shareholdersList;
        [ObservableProperty] private List<CompanyCharacter>? _othersList;
        [ObservableProperty] private List<AppDocument>? _registrationDocuments;

        [ObservableProperty] private List<SourceDocument>? _sourceDocuments;

        [ObservableProperty] private List<StaffMember>? _staffList;

        public bool IsPaid => PaymentStatus == "Paid";
        public bool IsUnpaid => PaymentStatus == "Unpaid";
        public bool IsPartial => PaymentStatus == "Partial";

        // Process Badge Helpers
        public bool IsCompleted => Process?.Equals("COMPLETED", StringComparison.OrdinalIgnoreCase) ?? false;
        public bool IsPendingProcess => Process?.Equals("PENDING", StringComparison.OrdinalIgnoreCase) ?? false;
        public bool IsInProgress => Process?.Equals("IN PROGRESS", StringComparison.OrdinalIgnoreCase) ?? false;
        public bool IsReview => Process?.Equals("REVIEW", StringComparison.OrdinalIgnoreCase) ?? false;
        public bool IsDraft => Process?.Equals("DRAFT", StringComparison.OrdinalIgnoreCase) ?? false;
        public bool IsSubmitted => Process?.Equals("SUBMITTED", StringComparison.OrdinalIgnoreCase) ?? false;
        public bool IsIssueRaised => Process?.Equals("ISSUE RAISED", StringComparison.OrdinalIgnoreCase) ?? false;
    }

    public partial class StaffMember : ObservableObject
    {
        [ObservableProperty] private bool _isSelected;
        [ObservableProperty] private string? _staffId;
        [ObservableProperty] private string? _staffName;
        [ObservableProperty] private string? _phone;
        [ObservableProperty] private string? _process;
        [ObservableProperty] private List<StaffHistory>? _history;

        public bool IsSubmit => Process?.Equals("SUBMIT", StringComparison.OrdinalIgnoreCase) ?? false;
        public bool IsComplete => Process?.Equals("COMPLETE", StringComparison.OrdinalIgnoreCase) ?? false;
    }

    public class StaffHistory
    {
        public DateTime Date { get; set; }
        public string? Description { get; set; }
        public decimal Amount { get; set; }
    }

    public class SourceDocument
    {
        public string? FileName { get; set; }
        public string? Description { get; set; }
    }
}
