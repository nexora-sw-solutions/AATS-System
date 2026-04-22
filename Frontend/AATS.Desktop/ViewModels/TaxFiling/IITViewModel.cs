using AATS.Desktop.Models;
using AATS.Desktop.ViewModels.Shared;
using System;
using System.Collections.Generic;

namespace AATS.Desktop.ViewModels.TaxFiling;

public partial class IITViewModel : TaxTableViewModelBase
{
    public override string PageTitle => "Individual Income Tax (IIT)";
    public override string SearchPlaceholder => "Search by Name, TIN, or Client ID";
    public override string TaxIdLabel => "Taxpayer ID (TIN)";
    public override string TaxIdPlaceholder => "e.g. TIN-123456";
    public override string TaxIdHeader => "TIN";
    
    // Guide Content
    public override string GuideLinkText => "Learn more about IIT";
    public override string GuideDescription => "Master the tools for tracking and managing Individual Income Tax records.";
    public override string GuideFeature1Title => "Personal TIN";
    public override string GuideFeature1Text => "Accurate record-keeping for individual Taxpayer Identification Numbers (TIN).";
    public override string GuideFeature2Title => "Income Sources";
    public override string GuideFeature2Text => "Categorize files by the regional branch the individual taxpayer belongs to.";
    public override string GuideFeature3Title => "Submission Deadlines";
    public override string GuideFeature3Text => "Monitor the 'Tax Period' to ensure timely individual submissions and avoid penalties.";
    public override string GuideFeature4Title => "Real-time Filtering";
    public override string GuideFeature4Text => "Combine 'Specific Date' filters with 'Unpaid' status to find overdue payments quickly.";
    public override string GuideProTip => "Use the global search bar to instantly find any individual by name or TIN as you type.";
}
