using AATS.Desktop.Models;
using AATS.Desktop.ViewModels.Shared;
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace AATS.Desktop.ViewModels.TaxFiling;

public partial class VATViewModel : TaxTableViewModelBase
{
    public override string PageTitle => "Value Added Tax (VAT)";
    public override string SearchPlaceholder => "Search by Name, VAT No, or Client ID";
    public override string TaxIdLabel => "VAT No.";
    public override string TaxIdPlaceholder => "e.g. VAT-1234567";
    public override string TaxIdHeader => "VAT No";
    // --- Add Record State ---
    public override bool IsAddRecordFormVisible => false;
    public override bool IsAddRecordButtonVisible => true;
    
    // Guide Content
    public override string GuideLinkText => "Learn more about VAT";
    public override string GuideDescription => "Master the tools for tracking and managing Value Added Tax (VAT) records.";
    public override string GuideFeature1Title => "VAT Validation";
    public override string GuideFeature1Text => "Ensure VAT numbers are correctly formatted for tracking compliance across all records.";
    public override string GuideFeature2Title => "Tax Period Cycles";
    public override string GuideFeature2Text => "Track standard quarterly or monthly VAT cycles efficiently using the period filters.";
    public override string GuideFeature3Title => "Reporting Accuracy";
    public override string GuideFeature3Text => "Update payment statuses as they happen to maintain parity with Inland Revenue Dept data.";
    public override string GuideFeature4Title => "Smart Search";
    public override string GuideFeature4Text => "Use the global search to quickly find VAT records by company name or VAT number in real-time.";
    public override string GuideProTip => "Maximize efficiency by combining 'IRD Paid' status filters with quarterly date ranges for audits.";
}
