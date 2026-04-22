using System;
using System.Collections.ObjectModel;
using AATS.Desktop.Models;
using AATS.Desktop.ViewModels.Shared;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory;

public partial class BusinessPlanViewModel : AuditTableViewModelBase
{
    public override string PageTitle => "Business Plan and Asset Valuation Consulting";
    public override string SearchPlaceholder => "Search Client, Company, ID...";
    public override string StatusHeader => "Status";
    public override string GuideLinkText => "Learn more about Business Plan and Asset Valuation Consulting";
    public override string GuideDescription => "Organize and track business plan development and advisory services.";
    public override string GuideFeature1Title => "Development Phases";
    public override string GuideFeature1Text => "Categorize business plans by stage: Draft, Review, and Final Submission.";
    public override string GuideProTip => "Use the audit notes to document specific client strategy discussions and projections.";
    
    public override bool IsProcessVisible => false;
    public override bool IsStatusVisible => true;
    public override bool IsBranchVisible => true;
    public override bool IsCompanyVisible => true;

    public BusinessPlanViewModel()
    {
    }
}
