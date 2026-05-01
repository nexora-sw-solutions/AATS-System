using System;
using System.Collections.Generic;
using AATS.Desktop.Models;
using AATS.Desktop.Services;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory;

public partial class HRConsultingDetailViewModel : DetailViewModelBase
{
    public override string GuideTitle => "Guide: HR & Management Consulting";
    public override string GuideDescription => "Track human resources consulting projects and advisory engagements.";
    public override string GuideProTip => "Maintain comprehensive notes on HR auditing results to provide high-quality policy development advice.";

    public HRConsultingDetailViewModel(AuditRecord record) : base(record)
    {
        InitializeSteps();
    }

    protected override void InitializeSteps()
    {
        var stepDefinitions = new List<(string Name, string? Icon)>
        {
            ("Consultation", null),
            ("Planning", null),
            ("Implementation", null),
            ("Review", null)
        };

        SetupSteps(stepDefinitions);
    }

    public override void OnDeleteRecord()
    {
        ConfirmDialogTitle = "Delete Record?";
        ConfirmDialogMessage = $"Are you sure you want to delete the HR Consulting record for '{CompanyName}'? This action cannot be undone.";
        ConfirmActionDelegate = async () =>
        {
            if (Record != null)
            {
                await DataService.Instance.DeleteAuditRecordsAsync("HR and Management Consulting", new[] { Record });
                NavigateBack?.Invoke();
            }
        };
        IsConfirmDialogVisible = true;
    }
}