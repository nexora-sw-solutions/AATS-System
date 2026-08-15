using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AATS.Desktop.Models
{
    public partial class OutstandingBalanceRecord : ObservableObject
    {
        [ObservableProperty] private string? _clientId;
        [ObservableProperty] private string? _clientName;
        [ObservableProperty] private string? _serviceModule;
        [ObservableProperty] private string? _invoiceNumber;
        [ObservableProperty] private decimal _totalAmount;
        [ObservableProperty] private decimal _amountPaid;
        [ObservableProperty] private decimal _outstandingAmount;
        [ObservableProperty] private string? _paymentType;
        [ObservableProperty] private string? _chequeNumber;
        [ObservableProperty] private DateTime _dueDate;
        [ObservableProperty] private string? _paymentStatus; // Paid, Partial, Unpaid, Pending Cheque, Bounced Cheque
        [ObservableProperty] private int _daysOverdue;
        [ObservableProperty] private DateTime? _lastPaymentDate;
        [ObservableProperty] private string? _notes;
        [ObservableProperty] private bool _isSelected;
        [ObservableProperty] private List<PaymentHistoryEntry>? _paymentHistory;
        [ObservableProperty] private List<ChequeDetail>? _chequeDetails;

        public bool HasChequeDetails => ChequeDetails != null && ChequeDetails.Count > 0;
        public bool HasPaymentHistory => PaymentHistory != null && PaymentHistory.Count > 0;
    }

    public class PaymentHistoryEntry
    {
        public DateTime Date { get; set; }
        public string? Description { get; set; }
        public decimal Amount { get; set; }
        public string? PaymentMethod { get; set; }
        public string? Reference { get; set; }
    }

    public class ChequeDetail
    {
        public string? ChequeNumber { get; set; }
        public string? Bank { get; set; }
        public decimal Amount { get; set; }
        public DateTime ChequeDate { get; set; }
        public string? Status { get; set; } // Pending, Realized, Bounced
    }
}
