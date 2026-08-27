using System;
using System.Collections.ObjectModel;
using AATS.Desktop.Models;
using AATS.Desktop.ViewModels.Shared;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory;

public partial class BusinessPlanViewModel : AuditTableViewModelBase
{
    public override string PageTitle => "Business Plan & Asset Valuation";
    public override string SearchPlaceholder => "Search Client, Company, ID...";
    public override string StatusHeader => "Payment Status";
    public override string GuideLinkText => "Learn more about Business Plans";
    public override string GuideDescription => "Track business plan development and advisory services.";
    public override string GuideFeature1Title => "Project Status";
    public override string GuideFeature1Text => "Monitor the progress of drafting, review, and finalization for each business plan.";
    public override string GuideProTip => "Use the country filter for valuations involving international assets.";
    
    public override bool IsProcessVisible => false;
    public override bool IsStatusVisible => true;
    public override bool IsProcessFilterVisible => false;
    public override bool IsCompanyVisible => true;

    public BusinessPlanViewModel()
    {
    }
}