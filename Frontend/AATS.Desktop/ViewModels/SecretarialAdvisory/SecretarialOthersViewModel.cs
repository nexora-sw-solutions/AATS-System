using System;
using System.Collections.ObjectModel;
using AATS.Desktop.Models;
using AATS.Desktop.ViewModels.Shared;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory;

public partial class SecretarialOthersViewModel : AuditTableViewModelBase
{
    public override string PageTitle => "Secretarial Others";
    public override string SearchPlaceholder => "Search Client, Company, Assignment...";
    public override string StatusHeader => "Payment Status";
    public override string GuideLinkText => "Learn more about Others";
    public override string GuideDescription => "Efficiently manage and track additional secretarial and advisory services.";
    public override string GuideFeature1Title => "Custom Service Tracking";
    public override string GuideFeature1Text => "Manage diverse secretarial tasks with dedicated status tracking and process indicators.";
    public override string GuideProTip => "Maintain detailed notes for each service to ensure seamless client communication and compliance.";
    
    public override bool IsProcessVisible => false;
    public override bool IsStatusVisible => true;
    public override bool IsBranchVisible => true;
    public override bool IsCompanyVisible => true;
    public override bool IsAssignmentVisible => true;

    public SecretarialOthersViewModel()
    {
    }
}
