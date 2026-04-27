using System;
using System.Collections.ObjectModel;
using AATS.Desktop.Models;
using AATS.Desktop.ViewModels.Shared;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory;

public partial class BOIViewModel : AuditTableViewModelBase
{
    public override string PageTitle => "BOI";
    public override string SearchPlaceholder => "Search Client, Company, ID...";
    public override string StatusHeader => "Status";
    public override string GuideLinkText => "Learn more about BOI";
    public override string GuideDescription => "Manage Board of Investment (BOI) approvals, compliance, and reporting.";
    public override string GuideFeature1Title => "Investment Status";
    public override string GuideFeature1Text => "Monitor the progress of BOI projects from registration to full status implementation.";
    public override string GuideProTip => "Track periodic compliance requirements and status updates for BOI-registered companies.";
    
    public override bool IsProcessVisible => false;
    public override bool IsStatusVisible => false;
    public override bool IsCompanyVisible => true;
    public override bool IsCountryVisible => true;

    public BOIViewModel()
    {
    }
}
