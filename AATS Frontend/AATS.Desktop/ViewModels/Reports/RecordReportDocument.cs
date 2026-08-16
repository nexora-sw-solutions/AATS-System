using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AATS.Desktop.Models;

namespace AATS.Desktop.ViewModels.Reports;

/// <summary>
/// Data model carrying all information needed to render a record report.
/// </summary>
public class RecordReportModel
{
    // ── Meta ────────────────────────────────────────────────────────────────
    public string ModuleTitle     { get; init; } = string.Empty;
    public string GeneratedByName { get; init; } = "System";
    public string CreatedByName   { get; init; } = string.Empty;
    public DateTime GeneratedAt   { get; init; } = DateTime.Now;

    // ── Record Summary ───────────────────────────────────────────────────────
    public string RecordId      { get; init; } = string.Empty;
    public string Code          { get; init; } = string.Empty;
    public string ClientName    { get; init; } = string.Empty;
    public string CompanyName   { get; init; } = string.Empty;
    public string Branch        { get; init; } = string.Empty;
    public string Date          { get; init; } = string.Empty;
    public string Status        { get; init; } = string.Empty;
    public string Process       { get; init; } = string.Empty;
    public string PaymentStatus { get; init; } = string.Empty;
    public string Category      { get; init; } = string.Empty;

    // ── General Information ──────────────────────────────────────────────────
    public string Phone          { get; init; } = string.Empty;
    public string Email          { get; init; } = string.Empty;
    public string Address        { get; init; } = string.Empty;
    public string Country        { get; init; } = string.Empty;
    public string Period         { get; init; } = string.Empty;
    public string Assignment     { get; init; } = string.Empty;
    public string Notes          { get; init; } = string.Empty;

    // ── Service / Module-specific ────────────────────────────────────────────
    public string RecordType       { get; init; } = string.Empty;
    public string InvestmentValue  { get; init; } = string.Empty;
    public string Objective        { get; init; } = string.Empty;
    public string Description      { get; init; } = string.Empty;
    public string TIN              { get; init; } = string.Empty;
    public string PeriodNumber     { get; init; } = string.Empty;
    public string PeriodType       { get; init; } = string.Empty;
    public string CountryAddress   { get; init; } = string.Empty;
    public string NoOfStaffs       { get; init; } = string.Empty;

    // ── Payment ──────────────────────────────────────────────────────────────
    public decimal SubTotal      { get; init; }
    public decimal Discount      { get; init; }
    public decimal TotalPayment  { get; init; }
    public decimal PartialAmount { get; init; }
    public string PaymentOption  { get; init; } = string.Empty;
    public string ChequeBank     { get; init; } = string.Empty;
    public string ChequeNumber   { get; init; } = string.Empty;
    public string ChequeDate     { get; init; } = string.Empty;
    public decimal? ChequeAmount { get; init; }
    public string ChequeStatus   { get; init; } = string.Empty;

    // ── Corporate Characters ─────────────────────────────────────────────────
    public List<CompanyCharacter> Directors    { get; init; } = new();
    public List<CompanyCharacter> Secretaries  { get; init; } = new();
    public List<CompanyCharacter> Shareholders { get; init; } = new();
    public List<CompanyCharacter> Others       { get; init; } = new();
    public List<CompanyOfficer>   Officers     { get; init; } = new();

    // ── Source Documents ─────────────────────────────────────────────────────
    public List<SourceDocument> SourceDocuments { get; init; } = new();

    // ── Logo ─────────────────────────────────────────────────────────────────
    /// Absolute path to the logo PNG file embedded in assets.
    public string? LogoPath { get; init; }
}

/// <summary>
/// QuestPDF document for a structured, professional record report.
/// </summary>
public class RecordReportDocument : IDocument
{
    // ── Palette ──────────────────────────────────────────────────────────────
    private const string AccentDark   = "#1A3B70";
    private const string AccentMid    = "#2D6BE4";
    private const string TextPrimary  = "#1E293B";
    private const string TextMuted    = "#64748B";
    private const string BgAlt        = "#F8FAFC";
    private const string BgWhite      = "#FFFFFF";
    private const string BorderColor  = "#E2E8F0";
    private const string White        = "#FFFFFF";

    public RecordReportModel Model { get; }

    public RecordReportDocument(RecordReportModel model) => Model = model;

    public DocumentMetadata GetMetadata() => new DocumentMetadata
    {
        Title     = $"{Model.ModuleTitle} Report",
        Author    = "AATS Management System",
        CreationDate = Model.GeneratedAt
    };

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(40);
            page.DefaultTextStyle(t => t.FontFamily("Arial").FontColor(TextPrimary).FontSize(10));
            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().Element(ComposeFooter);
        });
    }

    // ────────────────────────────── HEADER ──────────────────────────────────

    private void ComposeHeader(IContainer c)
    {
        c.Column(col =>
        {
            col.Item().Row(row =>
            {
                // Logo + Branding
                row.RelativeItem().Column(left =>
                {
                    // Try to embed logo
                    if (!string.IsNullOrEmpty(Model.LogoPath) && File.Exists(Model.LogoPath))
                    {
                        try
                        {
                            left.Item().Width(90).Image(Model.LogoPath).FitArea();
                        }
                        catch
                        {
                            left.Item().Text("AATS").FontSize(26).Bold().FontColor(AccentDark);
                        }
                    }
                    else
                    {
                        left.Item().Text("AATS").FontSize(26).Bold().FontColor(AccentDark);
                    }

                    left.Item().Text("AATS Management System")
                        .FontSize(9).FontColor(TextMuted);
                });

                // Report meta
                row.RelativeItem().AlignRight().Column(right =>
                {
                    right.Item().Text(Model.ModuleTitle.ToUpper() + " REPORT")
                        .FontSize(16).Bold().FontColor(AccentDark);
                    right.Item().Text($"Generated: {Model.GeneratedAt:dd MMM yyyy, HH:mm}")
                        .FontSize(8).FontColor(TextMuted);
                    right.Item().Text($"Generated By: {Model.GeneratedByName}")
                        .FontSize(8).FontColor(TextMuted);
                    if (!string.IsNullOrWhiteSpace(Model.CreatedByName))
                    {
                        right.Item().Text($"Created By: {Model.CreatedByName}")
                            .FontSize(8).FontColor(TextMuted);
                    }
                });
            });

            // Accent rule
            col.Item().PaddingTop(8).LineHorizontal(2).LineColor(AccentDark);
        });
    }

    // ────────────────────────────── CONTENT ─────────────────────────────────

    private void ComposeContent(IContainer c)
    {
        c.PaddingTop(16).Column(col =>
        {
            col.Spacing(14);

            // 1. Record Summary
            col.Item().Element(ComposeRecordSummary);

            // 2. General Information
            col.Item().Element(ComposeGeneralInformation);

            // 3. Service / Module Information (only if non-empty fields exist)
            if (HasServiceFields())
                col.Item().Element(ComposeServiceInformation);

            // 4. Payment Information
            if (Model.SubTotal > 0 || Model.TotalPayment > 0 || !string.IsNullOrWhiteSpace(Model.PaymentOption))
                col.Item().Element(ComposePaymentInformation);

            // 5. Corporate Characters (Company Registration etc.)
            if (Model.Directors.Any() || Model.Secretaries.Any() || Model.Shareholders.Any()
                || Model.Others.Any() || Model.Officers.Any())
                col.Item().Element(ComposeCharactersSection);

            // 6. Source Documents
            col.Item().Element(ComposeSourceDocuments);
        });
    }

    // ── Record Summary ────────────────────────────────────────────────────────

    private void ComposeRecordSummary(IContainer c)
    {
        c.Column(col =>
        {
            col.Item().Element(SectionHeader("Record Summary"));
            col.Item().Border(1).BorderColor(BorderColor).Padding(14).Column(inner =>
            {
                inner.Item().Row(row =>
                {
                    row.RelativeItem().Element(Field("Record ID",   string.IsNullOrWhiteSpace(Model.Code) ? Model.RecordId : Model.Code));
                    row.RelativeItem().Element(Field("Date",        Model.Date));
                    row.RelativeItem().Element(Field("Module",      Model.ModuleTitle));
                });

                inner.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Element(Field("Client Name",     Model.ClientName));
                    row.RelativeItem().Element(Field("Company",         Model.CompanyName));
                    row.RelativeItem().Element(Field("Branch",          Model.Branch));
                });

                inner.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Element(Field("Status",          Model.Status));
                    row.RelativeItem().Element(Field("Payment Status",  Model.PaymentStatus));
                    row.RelativeItem().Element(Field("Process",         Model.Process));
                });

                if (!string.IsNullOrWhiteSpace(Model.Category))
                {
                    inner.Item().PaddingTop(10).Element(Field("Client Category", Model.Category));
                }
            });
        });
    }

    // ── General Information ───────────────────────────────────────────────────

    private void ComposeGeneralInformation(IContainer c)
    {
        c.Column(col =>
        {
            col.Item().Element(SectionHeader("General Information"));
            col.Item().Border(1).BorderColor(BorderColor).Padding(14).Column(inner =>
            {
                inner.Item().Row(row =>
                {
                    row.RelativeItem().Element(Field("Phone",       Model.Phone));
                    row.RelativeItem().Element(Field("Email",       Model.Email));
                });

                if (!string.IsNullOrWhiteSpace(Model.Address))
                    inner.Item().PaddingTop(8).Element(Field("Address", Model.Address));

                inner.Item().PaddingTop(8).Row(row =>
                {
                    row.RelativeItem().Element(Field("Country",     Model.Country));
                    row.RelativeItem().Element(Field("Period",      Model.Period));
                });

                if (!string.IsNullOrWhiteSpace(Model.Assignment))
                    inner.Item().PaddingTop(8).Element(Field("Assignment", Model.Assignment));

                if (!string.IsNullOrWhiteSpace(Model.Notes))
                    inner.Item().PaddingTop(8).Element(Field("Notes", Model.Notes));
            });
        });
    }

    // ── Service Information ───────────────────────────────────────────────────

    private bool HasServiceFields() =>
        !string.IsNullOrWhiteSpace(Model.RecordType)      ||
        !string.IsNullOrWhiteSpace(Model.InvestmentValue) ||
        !string.IsNullOrWhiteSpace(Model.Objective)       ||
        !string.IsNullOrWhiteSpace(Model.Description)     ||
        !string.IsNullOrWhiteSpace(Model.TIN)             ||
        !string.IsNullOrWhiteSpace(Model.PeriodNumber)    ||
        !string.IsNullOrWhiteSpace(Model.CountryAddress)  ||
        !string.IsNullOrWhiteSpace(Model.NoOfStaffs);

    private void ComposeServiceInformation(IContainer c)
    {
        c.Column(col =>
        {
            col.Item().Element(SectionHeader("Service Information"));
            col.Item().Border(1).BorderColor(BorderColor).Padding(14).Column(inner =>
            {
                if (!string.IsNullOrWhiteSpace(Model.RecordType))
                    inner.Item().Element(Field("Record Type", Model.RecordType));

                if (!string.IsNullOrWhiteSpace(Model.InvestmentValue))
                    inner.Item().PaddingTop(8).Element(Field("Investment Value", Model.InvestmentValue));

                if (!string.IsNullOrWhiteSpace(Model.TIN))
                    inner.Item().PaddingTop(8).Element(Field("TIN", Model.TIN));

                if (!string.IsNullOrWhiteSpace(Model.PeriodNumber) || !string.IsNullOrWhiteSpace(Model.PeriodType))
                {
                    inner.Item().PaddingTop(8).Row(row =>
                    {
                        row.RelativeItem().Element(Field("Period Number", Model.PeriodNumber));
                        row.RelativeItem().Element(Field("Period Type",   Model.PeriodType));
                    });
                }

                if (!string.IsNullOrWhiteSpace(Model.CountryAddress))
                    inner.Item().PaddingTop(8).Element(Field("Country Address", Model.CountryAddress));

                if (!string.IsNullOrWhiteSpace(Model.NoOfStaffs))
                    inner.Item().PaddingTop(8).Element(Field("Number of Staffs", Model.NoOfStaffs));

                if (!string.IsNullOrWhiteSpace(Model.Objective))
                    inner.Item().PaddingTop(8).Element(Field("Objective", Model.Objective));

                if (!string.IsNullOrWhiteSpace(Model.Description))
                    inner.Item().PaddingTop(8).Element(Field("Description", Model.Description));
            });
        });
    }

    // ── Payment Information ───────────────────────────────────────────────────

    private void ComposePaymentInformation(IContainer c)
    {
        c.Column(col =>
        {
            col.Item().Element(SectionHeader("Payment Information"));
            col.Item().Border(1).BorderColor(BorderColor).Padding(14).Column(inner =>
            {
                inner.Item().Row(row =>
                {
                    row.RelativeItem().Element(Field("Sub Total",      $"Rs. {Model.SubTotal:N2}"));
                    row.RelativeItem().Element(Field("Discount",       $"Rs. {Model.Discount:N2}"));
                    row.RelativeItem().Element(Field("Payment Status",  Model.PaymentStatus ?? "N/A"));
                });

                inner.Item().PaddingTop(8).Row(row =>
                {
                    row.RelativeItem().Element(Field("Payment Option", Model.PaymentOption));
                    if (string.Equals(Model.PaymentStatus, "Partial", StringComparison.OrdinalIgnoreCase))
                    {
                        row.RelativeItem().Element(Field("Partially Paid Amount", $"Rs. {Model.PartialAmount:N2}"));
                        row.RelativeItem().Element(Field("Outstanding Balance",    $"Rs. {Model.TotalPayment:N2}"));
                    }
                    else
                    {
                        row.RelativeItem().Element(Field("Total Payment",  $"Rs. {Model.TotalPayment:N2}"));
                        row.RelativeItem(); // spacer
                    }
                });

                // Cheque details
                if (!string.IsNullOrWhiteSpace(Model.ChequeBank) || !string.IsNullOrWhiteSpace(Model.ChequeNumber))
                {
                    inner.Item().PaddingTop(10).LineHorizontal(0.5f).LineColor(BorderColor);
                    inner.Item().PaddingTop(8).Text("Cheque Details").FontSize(9).SemiBold().FontColor(AccentDark);
                    inner.Item().PaddingTop(4).Row(row =>
                    {
                        row.RelativeItem().Element(Field("Bank",          Model.ChequeBank));
                        row.RelativeItem().Element(Field("Cheque No.",    Model.ChequeNumber));
                        row.RelativeItem().Element(Field("Cheque Date",   Model.ChequeDate));
                    });
                    inner.Item().PaddingTop(4).Row(row =>
                    {
                        row.RelativeItem().Element(Field("Cheque Amount",
                            Model.ChequeAmount.HasValue ? $"Rs. {Model.ChequeAmount:N2}" : "N/A"));
                        row.RelativeItem().Element(Field("Cheque Status", Model.ChequeStatus));
                        row.RelativeItem(); // spacer
                    });
                }
            });
        });
    }

    // ── Corporate Characters ──────────────────────────────────────────────────

    private void ComposeCharactersSection(IContainer c)
    {
        c.Column(col =>
        {
            col.Item().Element(SectionHeader("Company Characters & Officers"));
            col.Item().Border(1).BorderColor(BorderColor).Padding(14).Column(inner =>
            {
                if (Model.Officers.Any())
                {
                    inner.Item().Element(CharacterTable("Officers",
                        Model.Officers.Select(o => (o.Name ?? "", o.Position ?? "", o.NicNumber ?? "")).ToList()));
                }

                if (Model.Directors.Any())
                {
                    inner.Item().PaddingTop(8).Element(CharacterTable("Directors",
                        Model.Directors.Select(d => (d.Name ?? "", d.Role ?? "", d.TIN ?? "")).ToList()));
                }

                if (Model.Secretaries.Any())
                {
                    inner.Item().PaddingTop(8).Element(CharacterTable("Secretaries",
                        Model.Secretaries.Select(s => (s.Name ?? "", s.Role ?? "", s.TIN ?? "")).ToList()));
                }

                if (Model.Shareholders.Any())
                {
                    inner.Item().PaddingTop(8).Element(CharacterTable("Shareholders",
                        Model.Shareholders.Select(s => (s.Name ?? "", $"{s.SharePercentage:N2}%", s.TIN ?? "")).ToList()));
                }

                if (Model.Others.Any())
                {
                    inner.Item().PaddingTop(8).Element(CharacterTable("Others",
                        Model.Others.Select(o => (o.Name ?? "", o.Role ?? "", o.Detail ?? "")).ToList()));
                }
            });
        });
    }

    private Action<IContainer> CharacterTable(string title, List<(string Name, string Role, string Extra)> rows)
    {
        return c =>
        {
            c.Column(col =>
            {
                col.Item().Text(title).FontSize(9).SemiBold().FontColor(AccentDark);
                col.Item().PaddingTop(4).Table(table =>
                {
                    table.ColumnsDefinition(cd =>
                    {
                        cd.RelativeColumn(3);
                        cd.RelativeColumn(2);
                        cd.RelativeColumn(2);
                    });

                    // Header
                    table.Header(h =>
                    {
                        static IContainer Th(IContainer tc) =>
                            tc.Background(AccentDark).Padding(5);

                        h.Cell().Element(Th).Text("Name").FontColor(White).FontSize(8).Bold();
                        h.Cell().Element(Th).Text("Role / Position").FontColor(White).FontSize(8).Bold();
                        h.Cell().Element(Th).Text("NIC / TIN / Note").FontColor(White).FontSize(8).Bold();
                    });

                    bool alt = false;
                    foreach (var (name, role, extra) in rows)
                    {
                        var bg = alt ? BgAlt : BgWhite;
                        alt = !alt;

                        table.Cell().Background(bg).Padding(5).Text(name).FontSize(8);
                        table.Cell().Background(bg).Padding(5).Text(role).FontSize(8);
                        table.Cell().Background(bg).Padding(5).Text(extra).FontSize(8);
                    }
                });
            });
        };
    }

    // ── Source Documents ──────────────────────────────────────────────────────

    private void ComposeSourceDocuments(IContainer c)
    {
        c.Column(col =>
        {
            col.Item().Element(SectionHeader("Source Documents"));
            col.Item().Border(1).BorderColor(BorderColor).Padding(14).Column(inner =>
            {
                if (!Model.SourceDocuments.Any())
                {
                    inner.Item().Text("No source documents attached to this record.")
                        .FontSize(9).FontColor(TextMuted).Italic();
                    return;
                }

                inner.Item().Table(table =>
                {
                    table.ColumnsDefinition(cd =>
                    {
                        cd.ConstantColumn(20);  // #
                        cd.RelativeColumn(4);   // File Name
                        cd.RelativeColumn(1.5f);// Type
                        cd.RelativeColumn(2);   // Description
                    });

                    // Header
                    table.Header(h =>
                    {
                        static IContainer Th(IContainer tc) =>
                            tc.Background(AccentDark).Padding(5);

                        h.Cell().Element(Th).Text("#").FontColor(White).FontSize(8).Bold();
                        h.Cell().Element(Th).Text("File Name").FontColor(White).FontSize(8).Bold();
                        h.Cell().Element(Th).Text("Type").FontColor(White).FontSize(8).Bold();
                        h.Cell().Element(Th).Text("Description").FontColor(White).FontSize(8).Bold();
                    });

                    var docs = Model.SourceDocuments;
                    for (int i = 0; i < docs.Count; i++)
                    {
                        var doc = docs[i];
                        var bg  = i % 2 == 0 ? BgWhite : BgAlt;
                        var ext = Path.GetExtension(doc.FileName ?? "").TrimStart('.').ToUpperInvariant();
                        if (string.IsNullOrWhiteSpace(ext)) ext = "—";

                        table.Cell().Background(bg).Padding(5).Text((i + 1).ToString()).FontSize(8);
                        table.Cell().Background(bg).Padding(5).Text(doc.FileName ?? "—").FontSize(8);
                        table.Cell().Background(bg).Padding(5).Text(ext).FontSize(8).FontColor(AccentMid);
                        table.Cell().Background(bg).Padding(5).Text(doc.Description ?? "—").FontSize(8).FontColor(TextMuted);
                    }
                });
            });
        });
    }

    // ────────────────────────────── FOOTER ──────────────────────────────────

    private void ComposeFooter(IContainer c)
    {
        c.Column(col =>
        {
            col.Item().LineHorizontal(1).LineColor(BorderColor);
            col.Item().PaddingTop(6).Row(row =>
            {
                row.RelativeItem().Text(x =>
                {
                    x.DefaultTextStyle(s => s.FontSize(8).FontColor(TextMuted));
                    x.Span("AATS Management System  |  Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
                row.RelativeItem().AlignRight()
                    .Text("www.aats.lk").FontSize(8).FontColor(AccentDark).Bold();
            });
        });
    }

    // ────────────────────────── HELPER BUILDERS ──────────────────────────────

    private static Action<IContainer> SectionHeader(string title) => c =>
    {
        c.PaddingBottom(4).Row(row =>
        {
            row.RelativeItem().Text(title).FontSize(11).SemiBold().FontColor(AccentDark);
            row.RelativeItem().AlignRight(); // no-op spacer
        });
    };

    private static Action<IContainer> Field(string label, string? value) => c =>
    {
        c.Column(col =>
        {
            col.Item().Text(label.ToUpper())
                .FontSize(7).Bold().FontColor(TextMuted)
                .LetterSpacing(0.5f);
            col.Item().PaddingTop(1)
                .Text(string.IsNullOrWhiteSpace(value) ? "—" : value)
                .FontSize(10);
        });
    };
}
