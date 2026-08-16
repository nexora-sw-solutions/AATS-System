using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.ObjectModel;
using AATS.Desktop.Models;
using AATS.Desktop.ViewModels.Shared;

namespace AATS.Desktop.ViewModels.TaxFiling;

public partial class TaxOthersViewModel : TaxTableViewModelBase
{
    public override string PageTitle => "Tax Others";
    public override string SearchPlaceholder => "Search by Client ID or Tax Number";
    public override string TaxIdLabel => "Tax No.";
    public override string TaxIdPlaceholder => "e.g. TAX-1234567";
    public override string TaxIdHeader => "Tax No";
    
    // Guide Content
    public override string GuideLinkText => "Learn more about Tax Others";
    public override string GuideDescription => "Manage uncategorized tax records.";
    public override string GuideFeature1Title => "Tax Tracking";
    public override string GuideFeature1Text => "Track custom tax payments.";
    public override string GuideFeature2Title => "Reporting";
    public override string GuideFeature2Text => "Generate reports for custom tax filings.";
    public override string GuideFeature3Title => "Payment Status";
    public override string GuideFeature3Text => "Monitor payments effectively.";
    public override string GuideFeature4Title => "Search";
    public override string GuideFeature4Text => "Search tax records easily.";
    public override string GuideProTip => "Use tags for custom tax categorization.";
}
