using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.ObjectModel;
using AATS.Desktop.Models;
using AATS.Desktop.ViewModels.Shared;

namespace AATS.Desktop.ViewModels.AuditAndAccounts;

public partial class ManagementAccountViewModel : AuditTableViewModelBase
{
    public override string PageTitle => "Management Accountings";
    public override string SearchPlaceholder => "Search Client, Assignment, ID...";
    public override string StatusHeader => "Status";
    public override string GuideLinkText => "Learn more about Management Accountings";

    public ManagementAccountViewModel()
    {
        ProcessFilters = new ObservableCollection<string> { "All Process", "BOOKKEEP", "DRAFT ACCOUNT", "FINALIZE", "HANDOVER", "-" };
    }
}
