using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AATS.Desktop.Models;
using AATS.Desktop.ViewModels.Shared;

namespace AATS.Desktop.ViewModels.AuditAndAccounts;

public partial class AuditOthersViewModel : AuditTableViewModelBase
{
    public override string PageTitle => "Audit Others";
    public override string SearchPlaceholder => "Search ID, Client, Assignment...";
    public override string StatusHeader => "Status";
    public override string GuideLinkText => "Learn more about Audit Others";
    
    public override bool IsProcessVisible => false;
    public override bool IsBranchVisible => false;
    public override bool IsCompanyVisible => true;
    public override bool IsAssignmentVisible => true;

    public AuditOthersViewModel()
    {
        ProcessFilters = new ObservableCollection<string> { "All Process", "AUDIT", "REVIEW", "CONSULTING", "-" };
    }
}
