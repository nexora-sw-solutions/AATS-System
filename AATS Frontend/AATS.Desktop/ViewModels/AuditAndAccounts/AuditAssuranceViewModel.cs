using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.ObjectModel;
using AATS.Desktop.Models;
using AATS.Desktop.ViewModels.Shared;
using AATS.Desktop.Services;

namespace AATS.Desktop.ViewModels.AuditAndAccounts;

public partial class AuditAssuranceViewModel : AuditTableViewModelBase
{
    public override string PageTitle => "Audit & Assurance";
    public override string SearchPlaceholder => "Search Client Name, ID...";
    public override string StatusHeader => "Status";
    public override string GuideLinkText => "Learn more about Audit & Assurance";

    public AuditAssuranceViewModel()
    {
        ProcessFilters = new ObservableCollection<string> { "All Process", "Bookkeep", "Draft", "Finalize", "Handover", "Submit", "Return" };

        LogService.Instance.AddLog("Login", "Audit & Assurance", "Central", "User accessed Audit module.");
    }
}
