using System;
using System.Collections.ObjectModel;
using AATS.Desktop.Models;
using AATS.Desktop.ViewModels.Shared;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory;

public partial class TradeLicenseViewModel : AuditTableViewModelBase
{
    public override string PageTitle => "Trade License";
    public override string SearchPlaceholder => "Search Client, Company, ID...";
    public override string StatusHeader => "Status";
    public override string GuideLinkText => "Learn more about Trade License";
    public override string GuideDescription => "Manage local authority trade licenses and periodic renewal records.";
    public override string GuideFeature1Title => "Renewal Cycles";
    public override string GuideFeature1Text => "Track licenses by assessment year and period to ensure timely renewals and avoid penalties.";
    public override string GuideProTip => "Organize records by regional branches and local government authorities for localized reporting.";
    
    public override bool IsProcessVisible => false;
    public override bool IsStatusVisible => false;
    public override bool IsStatusFilterVisible => true;
    public override bool IsProcessFilterVisible => false;
    public override bool IsCompanyVisible => true;

    public TradeLicenseViewModel()
    {
    }
}
