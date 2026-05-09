using System;
using System.Collections.ObjectModel;
using AATS.Desktop.Models;
using AATS.Desktop.ViewModels.Shared;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory;

public partial class BOIViewModel : AuditTableViewModelBase
{
    public override string PageTitle => "BOI Registration";
    public override string SearchPlaceholder => "Search Client, Company, ID...";
    public override string StatusHeader => "Payment Status";
    public override string GuideLinkText => "Learn more about BOI";
    public override string GuideDescription => "Manage Board of Investment (BOI) approvals and registrations.";
    public override string GuideFeature1Title => "Compliance Tracking";
    public override string GuideFeature1Text => "Track applications and periodic compliance requirements for BOI-registered companies.";
    public override string GuideProTip => "Ensure all investment value details are documented for periodic reporting.";
    
    public override bool IsProcessVisible => false;
    public override bool IsStatusVisible => true;
    public override bool IsProcessFilterVisible => false;
    public override bool IsCompanyVisible => true;
    public override bool IsCountryVisible => true;

    public BOIViewModel()
    {
    }
}