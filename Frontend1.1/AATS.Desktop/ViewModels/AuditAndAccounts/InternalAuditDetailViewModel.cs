using System;
using System.Collections.Generic;
using AATS.Desktop.Models;

namespace AATS.Desktop.ViewModels.AuditAndAccounts
{
    public partial class InternalAuditDetailViewModel : DetailViewModelBase
    {
        public InternalAuditDetailViewModel(AuditRecord record) : base(record)
        {
            record.Type = "Internal Audit";
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

        public override string Category => "Internal Audit";
    }
}
