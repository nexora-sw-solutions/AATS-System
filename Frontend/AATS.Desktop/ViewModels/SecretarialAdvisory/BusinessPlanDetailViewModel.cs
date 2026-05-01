using System;
using System.Collections.Generic;
using AATS.Desktop.Models;
using AATS.Desktop.Services;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory;

public partial class BusinessPlanDetailViewModel : DetailViewModelBase
{
    public override string GuideTitle => "Guide: Business Plan & Asset Valuation";
    public override string GuideDescription => "Organize and track business plan development and advisory services.";
    public override string GuideProTip => "Use the audit notes to document specific client strategy discussions and projections.";

    public BusinessPlanDetailViewModel(AuditRecord record) : base(record)
    {
        InitializeSteps();
    }

    protected override void InitializeSteps()
    {
        var stepDefinitions = new List<(string Name, string? Icon)>
        {
            ("Drafting", null),
            ("Review", null),
            ("Valuation", null),
            ("Finalization", null)
        };

        SetupSteps(stepDefinitions);
    }

    public override void OnDeleteRecord()
    {
        ConfirmDialogTitle = "Delete Record?";
        ConfirmDialogMessage = $"Are you sure you want to delete the Business Plan record for '{CompanyName}'? This action cannot be undone.";
        ConfirmActionDelegate = async () =>
        {
            if (Record != null)
            {
                await DataService.Instance.DeleteAuditRecordsAsync("Business Plan and Asset Valuation Consulting", new[] { Record });
                NavigateBack?.Invoke();
            }
        };
        IsConfirmDialogVisible = true;
    }
}