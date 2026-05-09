using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.Generic;

namespace AATS.Desktop.ViewModels.Reports;

public class ReportDocument : IDocument
{
    public ReportViewModel Model { get; }

    public ReportDocument(ReportViewModel model)
    {
        Model = model;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container
            .Page(page =>
            {
                page.Margin(50);
                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
    }

    void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("AATS").FontSize(28).Bold().FontColor("#1A3B70");
                col.Item().Text(Model.Title).FontSize(16).SemiBold().FontColor("#2D3436");
            });

            row.RelativeItem().AlignRight().Column(col =>
            {
                col.Item().Text(System.DateTime.Now.ToString("dd MMM yyyy")).FontSize(10);
                col.Item().Text($"Ref: {System.Guid.NewGuid().ToString()[..8].ToUpper()}").FontSize(8).FontColor("#636E72");
            });
        });
    }

    void ComposeContent(IContainer container)
    {
        container.PaddingVertical(30).Column(column =>
        {
            column.Spacing(20);

            // Client & Project Info
            column.Item().Row(row =>
            {
                row.RelativeItem().Border(1).BorderColor("#DFE6E9").Padding(10).Column(col =>
                {
                    col.Item().Text("CLIENT INFORMATION").FontSize(11).Bold().FontColor("#1A3B70");
                    col.Item().Text($"Name: {Model.ClientName}").FontSize(10);
                    col.Item().Text($"Phone: {Model.PhoneNumber}").FontSize(10);
                    col.Item().Text($"Email: {Model.Email}").FontSize(10);
                    col.Item().Text($"Address: {Model.Address}").FontSize(9).FontColor("#2D3436");
                });

                row.ConstantItem(20);

                row.RelativeItem().Border(1).BorderColor("#DFE6E9").Padding(10).Column(col =>
                {
                    col.Item().Text("ENTITY & STATUS").FontSize(11).Bold().FontColor("#1A3B70");
                    col.Item().Text($"Company/Branch: {Model.Company}").FontSize(10);
                    col.Item().Text($"Tax Period: {Model.TaxPeriod}").FontSize(10);
                    col.Item().Text($"Current Status: {Model.Status}").FontSize(10).Bold();
                    col.Item().Text($"Objective: {Model.Objective}").FontSize(9);
                });
            });

            // Service Details Header
            column.Item().PaddingTop(10).Text("SERVICE SUMMARY").FontSize(12).Bold().FontColor("#1A3B70");

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(80);
                    columns.RelativeColumn();
                    columns.ConstantColumn(80);
                    columns.ConstantColumn(50);
                    columns.ConstantColumn(80);
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("DATE");
                    header.Cell().Element(CellStyle).Text("DESCRIPTION");
                    header.Cell().Element(CellStyle).AlignRight().Text("PRICE");
                    header.Cell().Element(CellStyle).AlignCenter().Text("QTY");
                    header.Cell().Element(CellStyle).AlignRight().Text("AMOUNT");

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.DefaultTextStyle(x => x.SemiBold().FontSize(10)).PaddingVertical(8).BorderBottom(1.5f).BorderColor("#1A3B70");
                    }
                });

                foreach (var item in Model.Items)
                {
                    table.Cell().Element(ItemStyle).Text(item.Date).FontSize(9);
                    table.Cell().Element(ItemStyle).Text(item.Description).FontSize(9);
                    table.Cell().Element(ItemStyle).AlignRight().Text(item.PriceFormatted).FontSize(9);
                    table.Cell().Element(ItemStyle).AlignCenter().Text(item.Quantity.ToString()).FontSize(9);
                    table.Cell().Element(ItemStyle).AlignRight().Text(item.AmountFormatted).FontSize(9).Bold();

                    static IContainer ItemStyle(IContainer container)
                    {
                        return container.PaddingVertical(8).BorderBottom(1).BorderColor("#F1F5F9");
                    }
                }
            });

            // Summary
            column.Item().Row(row =>
            {
                row.RelativeItem();
                row.ConstantItem(200).Column(col =>
                {
                    col.Spacing(5);
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Subtotal:").FontSize(10);
                        r.RelativeItem().AlignRight().Text(Model.SubtotalFormatted).FontSize(10);
                    });
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Discount:").FontSize(10);
                        r.RelativeItem().AlignRight().Text($"-{Model.DiscountFormatted}").FontSize(10).FontColor(Colors.Red.Medium);
                    });
                    col.Item().PaddingVertical(5).BorderTop(1).BorderColor("#1A3B70");
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("TOTAL:").FontSize(14).Bold().FontColor("#1A3B70");
                        r.RelativeItem().AlignRight().Text(Model.TotalFormatted).FontSize(14).Bold().FontColor("#1A3B70");
                    });
                });
            });

            // Terms
            column.Item().PaddingTop(30).Column(col =>
            {
                col.Item().Text("TERMS & CONDITIONS").FontSize(10).Bold();
                col.Item().PaddingTop(5).Text("1. This is a computer-generated report and does not require a physical signature.").FontSize(8).FontColor("#636E72");
                col.Item().Text("2. Payment should be made within 30 days of the report generation date.").FontSize(8).FontColor("#636E72");
                col.Item().Text("3. For any discrepancies, please contact the AATS support team immediately.").FontSize(8).FontColor("#636E72");
            });
        });
    }

    void ComposeFooter(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Text(x =>
            {
                x.DefaultTextStyle(y => y.FontSize(9).FontColor("#636E72"));
                x.Span("Generated via AATS Management System | Page ");
                x.CurrentPageNumber();
            });

            row.RelativeItem().AlignRight().Text("www.aats.lk").FontSize(9).FontColor("#1A3B70").Bold();
        });
    }
}
