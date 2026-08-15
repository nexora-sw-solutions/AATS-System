using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AATS.Desktop.Models;

namespace AATS.Desktop.ViewModels.AuditAndAccounts
{
    public partial class ForensicAuditDetailViewModel : DetailViewModelBase
    {
        public ForensicAuditDetailViewModel(AuditRecord record) : base(record)
        {
            record.Type = "Forensic Audit";
            InitializeSteps();
        }

        protected override void InitializeSteps()
        {
            var stepDefs = new List<(string Name, string? Icon)>
            {
                ("Reporting", null),
                ("Meeting Complete", null),
                ("Submit", "fa-solid fa-check"),
                ("Return", "fa-solid fa-circle-info")
            };
            SetupSteps(stepDefs);
        }

        public override string Category => "Forensic Audit";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FilteredDocuments))]
        private string _selectedDocumentTab = "BR";

        public ObservableCollection<SourceDocument> FilteredDocuments =>
            new(SourceDocuments.Where(d => (d.Description ?? "BR") == SelectedDocumentTab));

        [RelayCommand]
        private void SelectDocumentTab(string tabName) => SelectedDocumentTab = tabName;

        [RelayCommand]
        private void PreviewSourceDocument(SourceDocument doc)
        {
            if (doc != null && !string.IsNullOrWhiteSpace(doc.Url ?? doc.FileName))
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(doc.Url ?? doc.FileName) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Error previewing document: {ex.Message}");
                }
            }
        }
    }
}
