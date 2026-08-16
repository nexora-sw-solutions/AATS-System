using System;
using System.Collections.ObjectModel;
using AATS.Desktop.Models;
using AATS.Desktop.ViewModels.Shared;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory;

public partial class TradeMarkViewModel : AuditTableViewModelBase
{
    public override string PageTitle => "Trade Mark";
    public override string SearchPlaceholder => "Search Client, Company, ID...";
    public override string StatusHeader => "Status";
    public override string GuideLinkText => "Learn more about Trade Mark";
    public override string GuideDescription => "Monitor trademark registration, search, and intellectual property protection.";
    public override string GuideFeature1Title => "IP Status Tracking";
    public override string GuideFeature1Text => "Track trademark applications through Search, Application, and Registration phases.";
    public override string GuideProTip => "Manage international trademark records across different jurisdictions using the Country filter.";
    
    public override bool IsProcessVisible => false;
    public override bool IsStatusVisible => true;
    public override bool IsProcessFilterVisible => false;
    public override bool IsCompanyVisible => true;
    public override bool IsCountryVisible => true;

    public TradeMarkViewModel()
    {
    }
}