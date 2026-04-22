using System;
using System.Collections.Generic;
using AATS.Desktop.Models;

namespace AATS.Desktop.ViewModels.AuditAndAccounts
{
    public partial class TaxAccountDetailViewModel : DetailViewModelBase
    {
        public TaxAccountDetailViewModel(AuditRecord record) : base(record)
        {
            InitializeSteps();
        }

        protected override void InitializeSteps()
        {
            var stepDefs = new List<(string Name, string? Icon)>
            {
                ("Bookkeep", null),
                ("Draft Account", null),
                ("Finalize", null),
                ("Handover", null)
            };
            SetupSteps(stepDefs);
        }
    }
}
