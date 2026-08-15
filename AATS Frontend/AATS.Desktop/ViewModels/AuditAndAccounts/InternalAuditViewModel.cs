using System;
using System.Collections.ObjectModel;
using AATS.Desktop.Models;
using AATS.Desktop.ViewModels.Shared;

namespace AATS.Desktop.ViewModels.AuditAndAccounts;

public partial class InternalAuditViewModel : AuditTableViewModelBase
{
    public override string PageTitle => "Internal Audit";
    public override string SearchPlaceholder => "Search Client, Assignment, ID...";
    public override string StatusHeader => "Status";
    public override string GuideLinkText => "Learn more about Internal Audit";

    public InternalAuditViewModel()
    {
        ProcessFilters = new ObservableCollection<string> { "All Process", "REPORTING", "MEETING COMPLETE", "-" };
    }
}
