using System;
using System.Collections.Generic;
using AATS.Desktop.Models;

namespace AATS.Desktop.ViewModels.AuditAndAccounts
{
    public partial class AuditAssuranceDetailViewModel : DetailViewModelBase
    {
        public AuditAssuranceDetailViewModel(AuditRecord record) : base(record)
        {
            InitializeSteps();
            
            // Default documents if none exist
            if (record.SourceDocuments == null || record.SourceDocuments.Count == 0)
            {
                record.SourceDocuments = new List<SourceDocument>
                {
                    new() { FileName = "Invoice-JAN.pdf", Description = "January Invoices" },
                    new() { FileName = "Bank-Stmt.pdf", Description = "BOC Bank Statement" }
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
    }
}
