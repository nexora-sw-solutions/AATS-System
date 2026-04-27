using System;
using System.Collections.ObjectModel;
using AATS.Desktop.Models;
using AATS.Desktop.ViewModels.Shared;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory;

public partial class HRConsultingViewModel : AuditTableViewModelBase
{
    public override string PageTitle => "HR and Management Consulting";
    public override string SearchPlaceholder => "Search Client, Company, ID...";
    public override string StatusHeader => "Status";
    public override string GuideLinkText => "Learn more about HR and Management Consulting";
    public override string GuideDescription => "Track human resources consulting projects and advisory engagements.";
    public override string GuideFeature1Title => "Project Classification";
    public override string GuideFeature1Text => "Organize HR projects by client, branch, and consulting scope for efficient management.";
    public override string GuideProTip => "Maintain comprehensive notes on HR auditing results to provide high-quality policy development advice.";
    
    public override bool IsProcessVisible => false;
    public override bool IsStatusVisible => false;
    public override bool IsBranchVisible => false;
    public override bool IsCompanyVisible => true;

    public HRConsultingViewModel()
    {
    }
}
