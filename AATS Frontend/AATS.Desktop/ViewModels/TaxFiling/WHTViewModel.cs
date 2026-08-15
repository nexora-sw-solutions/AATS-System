using AATS.Desktop.Models;
using AATS.Desktop.ViewModels.Shared;
using System;
using System.Collections.Generic;

namespace AATS.Desktop.ViewModels.TaxFiling;

public partial class WHTViewModel : TaxTableViewModelBase
{
    public override string PageTitle => "Withholding Tax (WHT)";
    public override string SearchPlaceholder => "Search by Name, WHT No, or Client ID";
    public override string TaxIdLabel => "WHT Reg No.";
    public override string TaxIdPlaceholder => "e.g. WHT-999000";
    public override string TaxIdHeader => "WHT No";
    // --- Add Record State ---
    public override bool IsAddRecordFormVisible => false;
    public override bool IsAddRecordButtonVisible => true;
    
    // Guide Content
    public override string GuideLinkText => "Learn more about WHT";
    public override string GuideDescription => "Master the tools for tracking and managing Withholding Tax (WHT) records.";
    public override string GuideFeature1Title => "WHT Certificates";
    public override string GuideFeature1Text => "Manage and track withholding tax registration numbers for all registered entities.";
    public override string GuideFeature2Title => "Branch Categorization";
    public override string GuideFeature2Text => "Filter and organize records based on regional withholding requirements and locations.";
    public override string GuideFeature3Title => "Auditor Notes";
    public override string GuideFeature3Text => "Add detailed auditor notes for specific withholding exceptions or calculation details.";
    public override string GuideFeature4Title => "Record Selection";
    public override string GuideFeature4Text => "Use bulk selection to manage multiple withholding certificates simultaneously for audits.";
    public override string GuideProTip => "Mark withholding taxes as 'IRD pending' when waiting for official government tax receipts.";
}
