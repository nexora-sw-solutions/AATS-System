using System;
using System.Collections.ObjectModel;
using AATS.Desktop.Models;
using AATS.Desktop.ViewModels.Shared;

namespace AATS.Desktop.ViewModels.TaxFiling;

public partial class TaxOthersViewModel : AuditTableViewModelBase
{
    public override string PageTitle => "Tax Others";
    public override string SearchPlaceholder => "Search ID, Client, Assignment...";
    public override string StatusHeader => "Status";
    public override string GuideLinkText => "Learn more about Tax Others";

    public override bool IsProcessVisible => false;
    public override bool IsBranchVisible => false;
    public override bool IsCompanyVisible => true;
    public override bool IsAssignmentVisible => true;

    public TaxOthersViewModel()
    {
        ProcessFilters = new ObservableCollection<string> { "All Process", "AUDIT", "REVIEW", "CONSULTING", "-" };
    }
}
