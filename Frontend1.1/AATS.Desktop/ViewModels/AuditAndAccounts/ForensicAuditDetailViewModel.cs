using System;
using System.Collections.Generic;
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
    }
}
