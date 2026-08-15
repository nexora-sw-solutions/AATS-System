using System;
using System.Collections.ObjectModel;
using AATS.Desktop.Models;
using AATS.Desktop.ViewModels.Shared;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory;

public partial class CompanyRegistrationViewModel : AuditTableViewModelBase
{
    public override string PageTitle => "Company Registration";
    public override string SearchPlaceholder => "Search Client, Company, ID...";
    public override string StatusHeader => "Payment Status";
    public override string GuideLinkText => "Learn more about Registration";
    public override string GuideDescription => "Manage the complete company registration lifecycle from name approval to incorporation.";
    public override string GuideFeature1Title => "Lifecycle Management";
    public override string GuideFeature1Text => "Track every step of registration including forms, signatures, and payments.";
    public override string GuideProTip => "Ensure all director and secretary details are updated before finalizing the incorporation.";
    
    public override bool IsProcessVisible => true;
    public override bool IsStatusVisible => true;
    public override bool IsCompanyVisible => true;

    public CompanyRegistrationViewModel()
    {
    }
}