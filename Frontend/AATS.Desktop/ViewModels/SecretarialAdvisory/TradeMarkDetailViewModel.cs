using System.Collections.Generic;
using AATS.Desktop.Models;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory
{
    public partial class TradeMarkDetailViewModel : DetailViewModelBase
    {
        public TradeMarkDetailViewModel(AuditRecord record) : base(record)
        {
            InitializeSteps();
        }

        public override string GuideTitle => "Guide: Trade Mark Details";
        public override string GuideDescription => "Monitor trademark registration, search, and intellectual property protection statuses.";
        public override string GuideProTip => "Review the 'Class Specification' document to ensure the trademark covers all relevant service categories.";

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
            ConfirmDialogTitle = "Delete Trade Mark Record?";
            ConfirmDialogMessage = $"Are you sure you want to delete the Trade Mark record for '{CompanyName}'? This action cannot be undone.";
            ConfirmActionDelegate = async () =>
            {
                if (Record != null)
                {
                    await Services.DataService.Instance.DeleteAuditRecordsAsync("Trade Mark", new[] { Record });
                    NavigateBack?.Invoke();
                }
            };
            IsConfirmDialogVisible = true;
        }
    }
}