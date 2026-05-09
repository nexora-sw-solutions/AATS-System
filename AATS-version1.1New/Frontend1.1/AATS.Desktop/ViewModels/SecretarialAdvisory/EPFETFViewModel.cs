using System;
using System.Collections.ObjectModel;
using AATS.Desktop.Models;
using AATS.Desktop.ViewModels.Shared;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory;

public partial class EPFETFViewModel : AuditTableViewModelBase
{
    public override string PageTitle => "EPF / ETF";
    public override string SearchPlaceholder => "Search Client, Company, ID...";
    public override string StatusHeader => "Payment Status";
    public override string GuideLinkText => "Learn more about EPF/ETF";
    public override string GuideDescription => "Manage EPF/ETF registrations and periodic filings for clients.";
    public override string GuideFeature1Title => "Staff Tracking";
    public override string GuideFeature1Text => "Track staff counts and registration details for each employer.";
    public override string GuideProTip => "Ensure all periodic returns are submitted before the statutory deadlines.";
    
    public override bool IsProcessVisible => false;
    public override bool IsStatusVisible => true;
    public override bool IsProcessFilterVisible => false;
    public override bool IsCompanyVisible => true;
    public override bool IsNoOfStaffsVisible => true;

    public EPFETFViewModel()
    {
    }
}