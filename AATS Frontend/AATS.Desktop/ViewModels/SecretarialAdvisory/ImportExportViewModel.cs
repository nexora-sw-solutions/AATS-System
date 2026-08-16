using System;
using System.Collections.ObjectModel;
using AATS.Desktop.Models;
using AATS.Desktop.ViewModels.Shared;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory;

public partial class ImportExportViewModel : AuditTableViewModelBase
{
    public override string PageTitle => "Import / Export";
    public override string SearchPlaceholder => "Search Client, Company, ID...";
    public override string StatusHeader => "Payment Status";
    public override string GuideLinkText => "Learn more about Import/Export";
    public override string GuideDescription => "Manage the import/export clearance workflow and customs documentation.";
    public override string GuideFeature1Title => "Clearance Workflow";
    public override string GuideFeature1Text => "Track applications through documentation, submission, and approval phases.";
    public override string GuideProTip => "Verify 'TIN' details before submitting customs applications.";
    
    public override bool IsProcessVisible => false;
    public override bool IsStatusVisible => true;
    public override bool IsProcessFilterVisible => false;
    public override bool IsCompanyVisible => true;
    public override bool IsCountryVisible => true;

    public ImportExportViewModel()
    {
    }
}