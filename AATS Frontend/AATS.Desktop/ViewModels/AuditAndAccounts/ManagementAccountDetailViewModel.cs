using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AATS.Desktop.Models;

namespace AATS.Desktop.ViewModels.AuditAndAccounts
{
    public partial class ManagementAccountDetailViewModel : DetailViewModelBase
    {
        [ObservableProperty] private string _selectedAttachmentTab = "BR";
        [ObservableProperty] private ObservableCollection<SourceDocument> _filteredAttachments = new();

        public ManagementAccountDetailViewModel(AuditRecord record) : base(record)
        {
            record.Type = "Management Accountings";
            InitializeSteps();
            UpdateFilteredAttachments();
        }

        [RelayCommand]
        private void SelectAttachmentTab(string tabName)
        {
            SelectedAttachmentTab = tabName;
            UpdateFilteredAttachments();
        }

        public void UpdateFilteredAttachments()
        {
            var filtered = SourceDocuments.Where(d => d.FileType == SelectedAttachmentTab).ToList();
            FilteredAttachments.Clear();
            foreach (var doc in filtered)
            {
                FilteredAttachments.Add(doc);
            }
        }

        protected override void InitializeSteps()
        {
            var stepDefs = new List<(string Name, string? Icon)>
            {
                ("Bookkeep", null),
                ("Draft Account", null),
                ("Finalize", null),
                ("Handover", null),
                ("Submit", "fa-solid fa-check"),
                ("Return", "fa-solid fa-circle-info")
            };
            SetupSteps(stepDefs);
        }

        public override string Category => "Management Accountings";
    }
}
