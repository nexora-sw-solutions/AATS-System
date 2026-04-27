using System;
using System.Collections.ObjectModel;
using AATS.Desktop.Models;
using AATS.Desktop.ViewModels.Shared;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory;

public partial class CompanyRegistrationViewModel : AuditTableViewModelBase
{
    public override string PageTitle => "Company Registration";
    public override string SearchPlaceholder => "Search Client, Company, ID...";
    public override string StatusHeader => "Payment Status";
    public override string GuideLinkText => "Learn more about Company Registration";
    public override string GuideDescription => "Track the lifecycle of company incorporations and statutory registrations.";
    public override string GuideFeature1Title => "Incorporation Workflow";
    public override string GuideFeature1Text => "Follow records through DRAFT, SUBMITTED, and COMPLETED incorporation stages.";
    public override string GuideProTip => "Monitor 'ISSUE RAISED' statuses to quickly address registrar queries and prevent delays.";
    
    public override bool IsProcessVisible => true;
    public override bool IsBranchVisible => true;
    public override bool IsCompanyVisible => true;

    public CompanyRegistrationViewModel()
    {
        ProcessFilters = new ObservableCollection<string> 
        { 
            "All Processes", "COMPLETED", "PENDING", "IN PROGRESS", "REVIEW", "DRAFT", "SUBMITTED", "ISSUE RAISED" 
        };
    }

    public override void AddRecord() => NavigateToAddRecord?.Invoke();
}
