using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.ObjectModel;
using AATS.Desktop.Models;
using AATS.Desktop.ViewModels.Shared;

namespace AATS.Desktop.ViewModels.AuditAndAccounts;

public partial class InternalControlViewModel : AuditTableViewModelBase
{
    public override string PageTitle => "Internal Control Systems & Outsourcing";
    public override string SearchPlaceholder => "Search Client, Assignment, ID...";
    public override string StatusHeader => "Status";
    public override string GuideLinkText => "Learn more about Internal Control Systems & Outsourcing";

    public InternalControlViewModel()
    {
        ProcessFilters = new ObservableCollection<string> { "All Process", "REPORTING", "MEETING COMPLETE", "-" };
    }
}
