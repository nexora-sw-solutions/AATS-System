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
        [property: JsonPropertyName("currentStep")]
        private int _currentStep; // 1 to 6
        
        [ObservableProperty] 
        [property: JsonPropertyName("id")]
        private string? _iD;
        
        [ObservableProperty]
        [property: JsonPropertyName("clientId")]
        private Guid? _clientId;

        [ObservableProperty]
        [property: JsonPropertyName("branchId")]
        private Guid _branchId;
        
        [ObservableProperty]
        [property: JsonPropertyName("clientCode")]
        private string? _clientCode;
        
        [ObservableProperty] 
        [property: JsonPropertyName("date")]
        private DateTime _date;
        
        [ObservableProperty] 
        [property: JsonPropertyName("clientName")]
        private string? _clientName;

        [ObservableProperty] 
        [property: JsonPropertyName("companyName")]
        private string? _company;

        [ObservableProperty] 
        [property: JsonPropertyName("paymentStatus")]
        private string? _paymentStatus; // Paid, Unpaid, Partial

        [ObservableProperty]
        [property: JsonPropertyName("status")]
        private string? _status; // ACTIVE, COMPLETED, etc.

        [ObservableProperty]
        [property: JsonPropertyName("process")]
        private string? _process; // BOOKKEEP, DRAFT, FINALIZE, etc.
        
        [ObservableProperty] 
        [property: JsonPropertyName("subTotal")]
        private decimal _subTotal;

        [ObservableProperty] 
        [property: JsonPropertyName("discount")]
        private decimal _discount;

        [ObservableProperty] 
        [property: JsonPropertyName("totalPayment")]
        private decimal _totalPayment;

        [ObservableProperty] 
        [property: JsonPropertyName("partialAmount")]
        private decimal _partialAmount;

        [ObservableProperty] 
        [property: JsonPropertyName("companyType")]
        private string? _type;
        
        public bool HasProcess => !string.IsNullOrWhiteSpace(Process) && Process != "-";
        public bool HasNoProcess => !HasProcess;
        
        [ObservableProperty] 
        [property: JsonPropertyName("paymentOption")]
        private string? _paymentOption; // Online, Cash, etc.
        
        [ObservableProperty] 
        [property: JsonPropertyName("assignment")]
        private string? _assignment;
        
        [ObservableProperty] 
        [property: JsonPropertyName("branchName")]
        private string? _branch;
        
        [ObservableProperty] 
        [property: JsonPropertyName("noOfStaffs")]
        private int _noOfStaffs;
        [ObservableProperty] private string? _country;
        [ObservableProperty] private string? _notes;
        [ObservableProperty] private string? _period; // e.g. "2024 Year", "Jan 2024"
        [ObservableProperty] private string? _tIN;
        [ObservableProperty] private string? _directorID;
        [ObservableProperty] private string? _investmentValue;
        [ObservableProperty] private string? _countryAddress;
        [ObservableProperty] private string? _periodNumber;
        [ObservableProperty] private string? _periodType;
        
        [ObservableProperty] 
        [property: JsonPropertyName("recordCode")]
        private string? _code;
        
        // Secretarial Advisory Extensions
        [ObservableProperty] 
        [property: JsonPropertyName("address")]
        private string? _address;

        [ObservableProperty] 
        [property: JsonPropertyName("email")]
        private string? _email;

        [ObservableProperty] 
        [property: JsonPropertyName("phone")]
        private string? _phoneNo;

        [ObservableProperty] 
        [property: JsonPropertyName("objective")]
        private string? _objective;

        [ObservableProperty] 
        [property: JsonPropertyName("description")]
        private string? _description;

        [ObservableProperty] 
        [property: JsonPropertyName("directors")]
        private List<CompanyCharacter>? _directorsList;

        [ObservableProperty] 
        [property: JsonPropertyName("secretaries")]
        private List<CompanyCharacter>? _secretariesList;

        [ObservableProperty] 
        [property: JsonPropertyName("shareholders")]
        private List<CompanyCharacter>? _shareholdersList;

        [ObservableProperty] 
        [property: JsonPropertyName("others")]
        private List<CompanyCharacter>? _othersList;
        [ObservableProperty] private List<AppDocument>? _registrationDocuments;

        [ObservableProperty] private List<SourceDocument>? _sourceDocuments;

        [ObservableProperty] 
        [property: JsonPropertyName("officers")]
        private List<CompanyOfficer>? _officers;

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

    public partial class CompanyOfficer : ObservableObject
    {
        [ObservableProperty]
        [property: JsonPropertyName("name")]
        private string? _name;

        [ObservableProperty]
        [property: JsonPropertyName("position")]
        private string? _position;

        [ObservableProperty]
        [property: JsonPropertyName("nicNumber")]
        private string? _nicNumber;
    }
}
