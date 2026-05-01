using System;
using System.Collections.Generic;
using AATS.Desktop.Models;
using AATS.Desktop.Services;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory;

public partial class BOIDetailViewModel : DetailViewModelBase
{
    public override string GuideTitle => "Guide: BOI Registration";
    public override string GuideDescription => "Manage Board of Investment (BOI) approvals, compliance, and reporting.";
    public override string GuideProTip => "Track periodic compliance requirements and status updates for BOI-registered companies.";

    public BOIDetailViewModel(AuditRecord record) : base(record)
    {
        InitializeSteps();
    }

    protected override void InitializeSteps()
    {
        var stepDefinitions = new List<(string Name, string? Icon)>
        {
            ("Application", null),
            ("Documentation", null),
            ("Approval", null),
            ("Registration", null)
        };

        SetupSteps(stepDefinitions);
    }

    public override void OnDeleteRecord()
    {
        ConfirmDialogTitle = "Delete Record?";
        ConfirmDialogMessage = $"Are you sure you want to delete the BOI record for '{CompanyName}'? This action cannot be undone.";
        ConfirmActionDelegate = async () =>
        {
            if (Record != null)
            {
                await DataService.Instance.DeleteAuditRecordsAsync("BOI", new[] { Record });
                NavigateBack?.Invoke();
            }
        };
        IsConfirmDialogVisible = true;
    }
}