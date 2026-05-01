using System;
using System.Collections.Generic;
using AATS.Desktop.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AATS.Desktop.Services;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory;

public partial class TradeLicenseDetailViewModel : DetailViewModelBase
{
    public override string GuideTitle => "Guide: Trade License Details";
    public override string GuideDescription => "Track and manage the trade license renewal process including assessment and registration.";
    public override string GuideProTip => "Ensure all source documents like NIC and BR are verified before advancing to the 'Finalize' step.";

    public TradeLicenseDetailViewModel(AuditRecord record) : base(record)
    {
        InitializeSteps();
        
        // Ensure some default source documents for demonstration as per mockup
        if (Record?.SourceDocuments == null || Record.SourceDocuments.Count == 0)
        {
            Record!.SourceDocuments = new List<SourceDocument>
            {
                new() { FileName = "NIC.pdf", Description = "National Identity Card" }
            };
        }
    }

    protected override void InitializeSteps()
    {
        var stepDefinitions = new List<(string Name, string? Icon)>
        {
            ("Bookkeep", null),
            ("Draft Account", null),
            ("Finalize", null),
            ("Handover", null),
            ("Return", "fa-solid fa-circle-info"),
            ("Submit", "fa-solid fa-check")
        };

        SetupSteps(stepDefinitions);
    }

    public override void OnDeleteRecord()
    {
        ConfirmDialogTitle = "Delete Record?";
        ConfirmDialogMessage = $"Are you sure you want to delete the Trade License record for '{CompanyName}'? This action cannot be undone.";
        ConfirmActionDelegate = async () =>
        {
            if (Record != null)
            {
                await DataService.Instance.DeleteAuditRecordsAsync("Trade License", new[] { Record });
                NavigateBack?.Invoke();
            }
        };
        IsConfirmDialogVisible = true;
    }
}