using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AATS.Desktop.Models;
using AATS.Desktop.Services;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using System.IO;
using System.Diagnostics;

namespace AATS.Desktop.ViewModels.Reports;

public partial class ReportViewModel : ObservableObject
{
    [ObservableProperty] private string _title = "INVOICE";
    [ObservableProperty] private string _clientName = string.Empty;
    [ObservableProperty] private string _phoneNumber = string.Empty;
    [ObservableProperty] private string _address = string.Empty;
    [ObservableProperty] private string _email = "N/A";
    [ObservableProperty] private string _company = "N/A";
    [ObservableProperty] private string _taxPeriod = "N/A";
    [ObservableProperty] private string _status = "N/A";
    [ObservableProperty] private string _objective = "N/A";
    [ObservableProperty] private string _assignment = "N/A";
    [ObservableProperty] private string _dateOfBirth = "N/A";
    [ObservableProperty] private string _bankName = "Borcelle Bank";
    [ObservableProperty] private string _accountNo = "+123-456-7890";
    
    [ObservableProperty] private List<ReportItem> _items = new();
    
    [ObservableProperty] private decimal _subtotal;
    [ObservableProperty] private decimal _discount;
    [ObservableProperty] private decimal _total;

    public string SubtotalFormatted => $"Rs. {Subtotal:N2}";
    public string DiscountFormatted => $"Rs. {Discount:N2}";
    public string TotalFormatted => $"Rs. {Total:N2}";

    public ReportViewModel() { }

    public ReportViewModel(AuditRecord record, string moduleTitle)
    {
        Title = (string.IsNullOrWhiteSpace(moduleTitle) || moduleTitle == "N/A" ? "AUDIT" : moduleTitle).ToUpper() + " REPORT";
        ClientName = record.ClientName ?? "N/A";
        PhoneNumber = record.PhoneNo ?? "N/A";
        Address = record.Address ?? "N/A";
        Email = record.Email ?? "N/A";
        Company = record.Company ?? "N/A";
        Objective = record.Objective ?? "N/A";
        Assignment = record.Assignment ?? "N/A";
        Status = record.PaymentStatus ?? "N/A";
        
        Items = new List<ReportItem>
        {
            new ReportItem 
            { 
                Date = record.Date.ToString("dd/MM/yyyy"),
                Description = record.Assignment ?? moduleTitle,
                Price = record.SubTotal,
                Quantity = 1,
                Amount = record.SubTotal
            }
        };
        
        Subtotal = record.SubTotal;
        Discount = record.Discount;
        Total = record.TotalPayment;
    }

    public ReportViewModel(NexoraRequest record)
    {
        Title = "NEXORA SERVICE REPORT";
        ClientName = record.ClientFullName;
        PhoneNumber = record.Phone ?? "N/A";
        Address = "N/A"; // Nexora request doesn't have a specific address field
        Company = record.CompanyName ?? "N/A";
        Assignment = record.Service ?? "N/A";
        Status = record.Status ?? "Pending";
        Email = "N/A";
        
        Items = new List<ReportItem>
        {
            new ReportItem 
            { 
                Date = record.Date.ToString("dd/MM/yyyy"),
                Description = record.Service,
                Price = 0,
                Quantity = 1,
                Amount = 0
            }
        };
        
        Subtotal = 0;
        Discount = 0;
        Total = 0;
    }

    public ReportViewModel(TaxRecord record, string moduleTitle)
    {
        Title = (string.IsNullOrWhiteSpace(moduleTitle) || moduleTitle == "N/A" ? "TAX" : moduleTitle).ToUpper() + " REPORT";
        ClientName = record.ClientName ?? "N/A";
        PhoneNumber = "N/A";
        Address = "N/A";
        Company = record.Branch ?? "N/A";
        TaxPeriod = record.TaxPeriod ?? "N/A";
        Status = record.Status ?? "N/A";
        Email = "N/A";
        
        Items = new List<ReportItem>
        {
            new ReportItem 
            { 
                Date = record.Date.ToString("dd/MM/yyyy"),
                Description = $"{moduleTitle} - {record.TaxPeriod}",
                Price = 0,
                Quantity = 1,
                Amount = 0
            }
        };
        
        Subtotal = 0;
        Discount = 0;
        Total = 0;
    }
    
    [RelayCommand]
    public async Task Print()
    {
        try
        {
            var document = new ReportDocument(this);
            var downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            var fileName = $"AATS_Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            var filePath = Path.Combine(downloadsPath, fileName);

            await Task.Run(() => document.GeneratePdf(filePath));

            LogService.Instance.AddLog("Print", "Report", "Central", $"Generated PDF report for {ClientName}");
            NotificationService.Instance.AddNotification("Report", "PDF generated and opened");

            // Open the file
            Process.Start(new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            NotificationService.Instance.AddNotification("Error", $"Failed to generate PDF: {ex.Message}");
        }
    }
}

public class ReportItem
{
    public string Date { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal Amount { get; set; }
    
    public string PriceFormatted => $"Rs. {Price:N2}";
    public string AmountFormatted => $"Rs. {Amount:N2}";
}
