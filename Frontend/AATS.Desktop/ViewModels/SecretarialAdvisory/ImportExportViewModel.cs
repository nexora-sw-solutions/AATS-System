using System;
using System.Collections.ObjectModel;
using AATS.Desktop.Models;
using AATS.Desktop.ViewModels.Shared;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory;

public partial class ImportExportViewModel : AuditTableViewModelBase
{
    public override string PageTitle => "Import / Export";
    public override string SearchPlaceholder => "Search Client, Company, ID...";
    public override string StatusHeader => "Status";
    public override string GuideLinkText => "Learn more about Import and Export Clearance";
    public override string GuideDescription => "Track import/export licensing, registration, and trade compliance advisory.";
    public override string GuideFeature1Title => "License Management";
    public override string GuideFeature1Text => "Monitor the application and renewal status of various trade licenses and regulatory approvals.";
    public override string GuideProTip => "Stay updated on customs registration statuses to ensure smooth trade operations for your clients.";
    
    public override bool IsProcessVisible => false;
    public override bool IsStatusVisible => false;
    public override bool IsCompanyVisible => true;

    public ImportExportViewModel()
    {
    }
}
