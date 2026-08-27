using System;
using System.Collections.ObjectModel;
using AATS.Desktop.Models;
using AATS.Desktop.ViewModels.Shared;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory;

public partial class TradeLicenseViewModel : AuditTableViewModelBase
{
    public override string PageTitle => "Trade License";
    public override string SearchPlaceholder => "Search Client, Company, ID...";
    public override string StatusHeader => "Payment Status";
    public override string GuideLinkText => "Learn more about Trade License";
    public override string GuideDescription => "Track and manage the trade license renewal process including assessment and registration.";
    public override string GuideFeature1Title => "Renewal Tracking";
    public override string GuideFeature1Text => "Monitor renewal timelines and assessment status for client trade licenses.";
    public override string GuideProTip => "Ensure all source documents are verified before finalizing the assessment.";
    
    public override bool IsProcessVisible => false;
    public override bool IsStatusVisible => true;
    public override bool IsProcessFilterVisible => false;
    public override bool IsCompanyVisible => true;

    public TradeLicenseViewModel()
    {
    }
}