using System;
using System.Collections.Generic;
using AATS.Desktop.Models;

namespace AATS.Desktop.ViewModels.AuditAndAccounts
{
    public partial class TaxAccountDetailViewModel : DetailViewModelBase
    {
        public TaxAccountDetailViewModel(AuditRecord record) : base(record)
        {
            record.Type = "Tax Account";
            InitializeSteps();
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

        public override string Category => "Tax Account";
    }
}
