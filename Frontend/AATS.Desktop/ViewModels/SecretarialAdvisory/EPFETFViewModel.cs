using System;
using System.Collections.ObjectModel;
using AATS.Desktop.Models;
using AATS.Desktop.ViewModels.Shared;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory;

public partial class EPFETFViewModel : AuditTableViewModelBase
{
    public override string PageTitle => "EPF / ETF";
    public override string SearchPlaceholder => "Search Client, Company, ID...";
    public override string StatusHeader => "Status";
    public override string GuideLinkText => "Learn more about EPF / ETF";
    public override string GuideDescription => "Manage Employee Provident Fund (EPF) and Employee Trust Fund (ETF) compliance.";
    public override string GuideFeature1Title => "Registration Tracking";
    public override string GuideFeature1Text => "Monitor client registrations and amendment requests for statutory retirement funds.";
    public override string GuideProTip => "Track the status of periodic remittances to ensure full statutory compliance for all employees.";
    
    public override bool IsProcessVisible => false;
    public override bool IsStatusVisible => false;
    public override bool IsCompanyVisible => true;
    public override bool IsNoOfStaffsVisible => true;

    public EPFETFViewModel()
    {
    }
}
