using AATS.Desktop.Models;
using AATS.Desktop.ViewModels.Shared;
using System;
using System.Collections.Generic;

namespace AATS.Desktop.ViewModels.TaxFiling;

public partial class CITViewModel : TaxTableViewModelBase
{
    public override string PageTitle => "Corporate Income Tax (CIT)";
    public override string SearchPlaceholder => "Search by Name, DIN, or Client ID";
    public override string TaxIdLabel => "Director ID (DIN)";
    public override string TaxIdPlaceholder => "e.g. DIN-123456";
    public override string TaxIdHeader => "DIN No";
    
    // Guide Content
    public override string GuideLinkText => "Learn more about CIT";
    public override string GuideDescription => "Master the tools for tracking and managing Corporate Income Tax records.";
    public override string GuideFeature1Title => "Registration Tracking";
    public override string GuideFeature1Text => "Track DIN (Director ID) for all corporate entities using the dedicated ID field.";
    public override string GuideFeature2Title => "Quarterly Filing";
    public override string GuideFeature2Text => "Monitor quarterly payment statuses (Paid/Pending/IRD Paid) to ensure compliance.";
    public override string GuideFeature3Title => "Assessment Year Sync";
    public override string GuideFeature3Text => "Ensure the assessment year matches the corporate fiscal cycle for accurate reporting.";
    public override string GuideFeature4Title => "Regional Distribution";
    public override string GuideFeature4Text => "Categorize corporate records by branch (South, West, Central, Northeast) for regional analysis.";
    public override string GuideProTip => "Use the 'IRD Paid' status to specifically flag records that have been confirmed by the Inland Revenue Department.";
}
