using System.Collections.Generic;
using AATS.Desktop.Models;
using AATS.Desktop.Services;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory
{
    public partial class ImportExportDetailViewModel : DetailViewModelBase
    {
        public override string GuideTitle => "Guide: Clearance Details";
        public override string GuideDescription => "Manage the import/export clearance workflow from documentation to final approval.";
        public override string GuideProTip => "Double check the 'TIN' and 'Assignment' type before submitting the application to customs.";

        public ImportExportDetailViewModel(AuditRecord record) : base(record)
        {
            InitializeSteps();
            
            // Ensure default source documents if missing for demonstration
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
                ("Documentation", null),
                ("Application", null),
                ("Submission", null),
                ("Approval", null)
            };

            SetupSteps(stepDefinitions);
        }

        public override void OnDeleteRecord()
        {
            ConfirmDialogTitle = "Delete Import/Export Record?";
            ConfirmDialogMessage = $"Are you sure you want to delete the clearance record for '{CompanyName}'? This action cannot be undone.";
            ConfirmActionDelegate = async () =>
            {
                if (Record != null)
                {
                    await DataService.Instance.DeleteAuditRecordsAsync("Import / Export", new[] { Record });
                    NavigateBack?.Invoke();
                }
            };
            IsConfirmDialogVisible = true;
        }
    }
}
