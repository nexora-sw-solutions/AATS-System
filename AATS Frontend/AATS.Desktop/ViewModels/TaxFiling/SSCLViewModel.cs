using AATS.Desktop.Models;
using AATS.Desktop.ViewModels.Shared;
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace AATS.Desktop.ViewModels.TaxFiling;

public partial class SSCLViewModel : TaxTableViewModelBase
{
    public override string PageTitle => "Social Security Contribution Levy (SSCL)";
    public override string SearchPlaceholder => "Search by Name, SSCL No, or Client ID";
    public override string TaxIdLabel => "SSCL No.";
    public override string TaxIdPlaceholder => "e.g. SSCL-555666";
    public override string TaxIdHeader => "SSCL No";
    // --- Add Record State ---
    public override bool IsAddRecordFormVisible => false;
    public override bool IsAddRecordButtonVisible => true;
    
    // Guide Content
    public override string GuideLinkText => "Learn more about SSCL";
    public override string GuideDescription => "Master the tools for tracking and managing Social Security Contribution Levy (SSCL).";
    public override string GuideFeature1Title => "Levy Compliance";
    public override string GuideFeature1Text => "Accurately track SSCL numbers for all liable entities in your portfolio.";
    public override string GuideFeature2Title => "Period Tracking";
    public override string GuideFeature2Text => "Monitor durations (days, months, or years) to ensure levy calculations are up to date.";
    public override string GuideFeature3Title => "Contribution Status";
    public override string GuideFeature3Text => "Easily identify pending and paid contributions using the color-coded status badges.";
    public override string GuideFeature4Title => "Regional Analysis";
    public override string GuideFeature4Text => "Filter by regional branches like 'South' or 'Central' to see localized levy collections.";
    public override string GuideProTip => "Filter by 'South' branch to view performance metrics for regional levy collections in real-time.";
}
