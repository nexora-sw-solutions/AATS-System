using System;
using System.Collections.ObjectModel;
using AATS.Desktop.Models;
using AATS.Desktop.ViewModels.Shared;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory;

public partial class HRConsultingViewModel : AuditTableViewModelBase
{
    public override string PageTitle => "HR & Management Consulting";
    public override string SearchPlaceholder => "Search Client, Company, ID...";
    public override string StatusHeader => "Payment Status";
    public override string GuideLinkText => "Learn more about HR Consulting";
    public override string GuideDescription => "Track human resources and management consulting advisory services.";
    public override string GuideFeature1Title => "Project Management";
    public override string GuideFeature1Text => "Organize consulting projects with dedicated status tracking and documentation.";
    public override string GuideProTip => "Maintain comprehensive notes on policy development advice for each client.";
    
    public override bool IsProcessVisible => false;
    public override bool IsStatusVisible => true;
    public override bool IsProcessFilterVisible => false;
    public override bool IsCompanyVisible => true;

    public HRConsultingViewModel()
    {
    }
}