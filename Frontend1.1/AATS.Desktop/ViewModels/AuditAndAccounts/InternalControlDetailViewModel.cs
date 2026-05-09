using System;
using System.Collections.Generic;
using AATS.Desktop.Models;

namespace AATS.Desktop.ViewModels.AuditAndAccounts
{
    public partial class InternalControlDetailViewModel : DetailViewModelBase
    {
        public InternalControlDetailViewModel(AuditRecord record) : base(record)
        {
            record.Type = "Internal Control";
            InitializeSteps();
        }

        protected override void InitializeSteps()
        {
            var stepDefs = new List<(string Name, string? Icon)>
            {
                ("Reporting", null),
                ("Meeting Complete", null)
            };
            SetupSteps(stepDefs);
        }
    }
}
