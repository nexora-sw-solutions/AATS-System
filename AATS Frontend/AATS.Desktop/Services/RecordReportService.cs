using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using AATS.Desktop.Models;
using AATS.Desktop.ViewModels.Reports;
using Avalonia.Threading;

namespace AATS.Desktop.Services;

/// <summary>
/// Singleton service for generating, downloading, and printing professional record reports using QuestPDF.
/// </summary>
public sealed class RecordReportService
{
    private static RecordReportService? _instance;
    public static RecordReportService Instance => _instance ??= new RecordReportService();

    private RecordReportService()
    {
        // Ensure QuestPDF community licence is set.
        // QuestPDF 2026.x requires an explicit licence setting for free (Community) use.
        QuestPDF.Settings.License = LicenseType.Community;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Public API
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Generates the report PDF in memory and returns the raw bytes.
    /// Safe to call from any thread.
    /// </summary>
    public Task<byte[]> GenerateReportAsync(AuditRecord record, string moduleTitle, string? generatedByName)
    {
        return Task.Run(() =>
        {
            var model    = BuildModel(record, moduleTitle, generatedByName);
            var document = new RecordReportDocument(model);
            return document.GeneratePdf();
        });
    }

    public Task<byte[]> GenerateReportAsync(TaxRecord record, string moduleTitle, string? generatedByName)
    {
        return Task.Run(() =>
        {
            var model    = BuildModel(record, moduleTitle, generatedByName);
            var document = new RecordReportDocument(model);
            return document.GeneratePdf();
        });
    }

    public Task<byte[]> GenerateReportAsync(ClientRecord record, string moduleTitle, string? generatedByName)
    {
        return Task.Run(() =>
        {
            var model    = BuildModel(record, moduleTitle, generatedByName);
            var document = new RecordReportDocument(model);
            return document.GeneratePdf();
        });
    }

    public Task<byte[]> GenerateReportAsync(TeamMember record, string moduleTitle, string? generatedByName)
    {
        return Task.Run(() =>
        {
            var model    = BuildModel(record, moduleTitle, generatedByName);
            var document = new RecordReportDocument(model);
            return document.GeneratePdf();
        });
    }

    public Task<byte[]> GenerateReportAsync(NexoraRequest record, string moduleTitle, string? generatedByName)
    {
        return Task.Run(() =>
        {
            var model    = BuildModel(record, moduleTitle, generatedByName);
            var document = new RecordReportDocument(model);
            return document.GeneratePdf();
        });
    }

    /// <summary>
    /// Generates the report and opens a WinForms SaveFileDialog so the user can choose where to save it.
    /// </summary>
    private async Task DownloadReportInternalAsync(Task<byte[]> generateTask, string suggestedName)
    {
        var pdfBytes = await generateTask;

        string? savePath = null;

        System.Console.WriteLine("[DEBUG] DownloadReportInternalAsync invoked, marshaling to UIThread...");
        // Must show the Save dialog on the UI (STA) thread.
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            System.Console.WriteLine("[DEBUG] Inside Dispatcher.UIThread for SaveFileDialog...");
            try
            {
                using var dialog = new System.Windows.Forms.SaveFileDialog
                {
                    Title            = "Save Record Report",
                    Filter           = "PDF Files (*.pdf)|*.pdf",
                    DefaultExt       = "pdf",
                    FileName         = suggestedName,
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                                       + Path.DirectorySeparatorChar + "Downloads"
                };

                System.Console.WriteLine("[DEBUG] Calling SaveFileDialog.ShowDialog()...");
                var result = dialog.ShowDialog();
                System.Console.WriteLine($"[DEBUG] SaveFileDialog.ShowDialog() returned: {result}");
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    savePath = dialog.FileName;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[DEBUG] SaveFileDialog threw exception: {ex}");
            }
        });

        if (string.IsNullOrEmpty(savePath)) return;

        await File.WriteAllBytesAsync(savePath, pdfBytes);

        NotificationService.Instance.AddNotification("Report Saved",
            $"Report saved to '{Path.GetFileName(savePath)}'.");

        // Open the file with the default PDF viewer.
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(savePath)
            {
                UseShellExecute = true
            });
        }
        catch
        {
            // Opening is best-effort; ignore if no viewer is registered.
        }
    }

    /// <summary>
    /// Generates the report to a temporary file and hands it to the Windows Shell with
    /// the "print" verb so the OS-registered PDF handler opens its print dialog.
    /// Falls back to opening the PDF in the default viewer if the print verb is unavailable.
    ///
    /// This approach is used instead of PdfiumViewer because PdfiumViewer's native Win32
    /// GDI printing stack raises unmanaged SEH exceptions on .NET 10 that bypass managed
    /// try/catch blocks and crash the process.  The shell verb delegates entirely to the
    /// system PDF infrastructure (built-in on Windows 10 / 11) with no native-interop risk.
    /// </summary>
    public Task DownloadReportAsync(AuditRecord record, string moduleTitle, string? generatedByName) => 
        DownloadReportInternalAsync(GenerateReportAsync(record, moduleTitle, generatedByName), BuildSuggestedFileName(record.ClientName ?? record.Company, moduleTitle));

    public Task DownloadReportAsync(TaxRecord record, string moduleTitle, string? generatedByName) => 
        DownloadReportInternalAsync(GenerateReportAsync(record, moduleTitle, generatedByName), BuildSuggestedFileName(record.ClientName ?? record.Branch, moduleTitle));

    public Task DownloadReportAsync(ClientRecord record, string moduleTitle, string? generatedByName) => 
        DownloadReportInternalAsync(GenerateReportAsync(record, moduleTitle, generatedByName), BuildSuggestedFileName(record.Name ?? "Client", moduleTitle));

    public Task DownloadReportAsync(TeamMember record, string moduleTitle, string? generatedByName) => 
        DownloadReportInternalAsync(GenerateReportAsync(record, moduleTitle, generatedByName), BuildSuggestedFileName(record.Username ?? "Staff", moduleTitle));

    public Task DownloadReportAsync(NexoraRequest record, string moduleTitle, string? generatedByName) => 
        DownloadReportInternalAsync(GenerateReportAsync(record, moduleTitle, generatedByName), BuildSuggestedFileName(record.ClientFullName ?? "Nexora", moduleTitle));

    private async Task PrintReportInternalAsync(Task<byte[]> generateTask)
    {
        // Step 1 — Generate PDF bytes on a background thread.
        var pdfBytes = await generateTask;

        // Step 2 — Write to a temp file (async I/O).
        var tempFile = Path.Combine(
            Path.GetTempPath(),
            $"AATS_Report_{Guid.NewGuid():N}.pdf");

        await File.WriteAllBytesAsync(tempFile, pdfBytes);
        System.Diagnostics.Debug.WriteLine($"[RecordReportService] Temp PDF: {tempFile}");

        // Step 3 — Ask the Windows Shell to print it.
        //   • The "print" verb tells the system's registered PDF handler to open its
        //     own print dialog — no native DLLs, no COM, no STA needed on our side.
        //   • Windows 10/11 handles PDFs natively via Windows Print to PDF / Edge.
        //   • If the verb is not registered on this machine, we fall back to opening
        //     the file in the viewer and showing a tip to use Ctrl+P.
        bool launched = false;

        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName       = tempFile,
                Verb           = "print",
                UseShellExecute = true,
                CreateNoWindow  = true,
                WindowStyle     = System.Diagnostics.ProcessWindowStyle.Hidden
            };
            System.Diagnostics.Process.Start(psi);
            launched = true;
            System.Diagnostics.Debug.WriteLine("[RecordReportService] Print verb launched successfully.");
        }
        catch (Exception ex)
        {
            // "print" verb not registered — fall back to opening in the default viewer.
            System.Diagnostics.Debug.WriteLine(
                $"[RecordReportService] Print verb failed ({ex.Message}), falling back to open.");
        }

        if (!launched)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(tempFile)
                {
                    UseShellExecute = true
                });
                NotificationService.Instance.AddNotification(
                    "Report Opened",
                    "PDF opened in viewer — press Ctrl+P to print.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RecordReportService] Fallback open failed: {ex.Message}");
                throw new InvalidOperationException(
                    "Could not open the report PDF for printing.", ex);
            }
        }
        else
        {
            NotificationService.Instance.AddNotification(
                "Printing",
                "The report has been sent to the print dialog.");
        }

        // Step 4 — Deferred temp-file cleanup.
        //   60 seconds gives the shell/viewer enough time to open and fully read the file.
        //   Failure to delete is non-fatal and is only logged.
        _ = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(60));
            try
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
                System.Diagnostics.Debug.WriteLine(
                    $"[RecordReportService] Temp file cleaned up: {tempFile}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[RecordReportService] Could not delete temp file: {ex.Message}");
            }
        });
    }

    public Task PrintReportAsync(AuditRecord record, string moduleTitle, string? generatedByName) => 
        PrintReportInternalAsync(GenerateReportAsync(record, moduleTitle, generatedByName));

    public Task PrintReportAsync(TaxRecord record, string moduleTitle, string? generatedByName) => 
        PrintReportInternalAsync(GenerateReportAsync(record, moduleTitle, generatedByName));

    public Task PrintReportAsync(ClientRecord record, string moduleTitle, string? generatedByName) => 
        PrintReportInternalAsync(GenerateReportAsync(record, moduleTitle, generatedByName));

    public Task PrintReportAsync(TeamMember record, string moduleTitle, string? generatedByName) => 
        PrintReportInternalAsync(GenerateReportAsync(record, moduleTitle, generatedByName));

    public Task PrintReportAsync(NexoraRequest record, string moduleTitle, string? generatedByName) => 
        PrintReportInternalAsync(GenerateReportAsync(record, moduleTitle, generatedByName));

    // ─────────────────────────────────────────────────────────────────────────
    // Private helpers
    // ─────────────────────────────────────────────────────────────────────────

    private static RecordReportModel BuildModel(AuditRecord record, string moduleTitle, string? generatedByName)
    {
        // Resolve logo path from the Avalonia asset bundle path to a real filesystem path.
        // Assets are copied to output directory by the build.
        var logoPath = ResolveLogoPath();

        return new RecordReportModel
        {
            // ── Meta
            ModuleTitle     = moduleTitle,
            GeneratedByName = string.IsNullOrWhiteSpace(generatedByName) ? "System" : generatedByName,
            CreatedByName   = record.CreatedByName ?? string.Empty,
            GeneratedAt     = DateTime.Now,
            LogoPath        = logoPath,

            // ── Record Summary
            RecordId      = record.ID      ?? string.Empty,
            Code          = record.Code    ?? string.Empty,
            ClientName    = record.ClientName    ?? string.Empty,
            CompanyName   = record.Company ?? string.Empty,
            Branch        = record.Branch  ?? string.Empty,
            Date          = record.Date.ToString("dd/MM/yyyy"),
            Status        = record.Status         ?? string.Empty,
            Process       = record.Process        ?? string.Empty,
            PaymentStatus = record.PaymentStatus  ?? string.Empty,
            Category      = record.ClientCategory ?? string.Empty,

            // ── General Information
            Phone       = record.PhoneNo   ?? string.Empty,
            Email       = record.Email     ?? string.Empty,
            Address     = record.Address   ?? string.Empty,
            Country     = record.Country   ?? string.Empty,
            Period      = record.Period    ?? string.Empty,
            Assignment  = record.Assignment ?? string.Empty,
            Notes       = record.Notes     ?? string.Empty,

            // ── Service / Module-specific
            RecordType      = record.Type          ?? string.Empty,
            InvestmentValue = record.InvestmentValue ?? string.Empty,
            Objective       = record.Objective     ?? string.Empty,
            Description     = record.Description   ?? string.Empty,
            TIN             = record.TIN           ?? string.Empty,
            PeriodNumber    = record.PeriodNumber  ?? string.Empty,
            PeriodType      = record.PeriodType    ?? string.Empty,
            CountryAddress  = record.CountryAddress ?? string.Empty,
            NoOfStaffs      = record.NoOfStaffs > 0 ? record.NoOfStaffs.ToString() : string.Empty,

            // ── Payment
            SubTotal      = record.SubTotal,
            Discount      = record.Discount,
            TotalPayment  = record.TotalPayment,
            PartialAmount = record.PartialAmount,
            PaymentOption = record.PaymentOption ?? string.Empty,
            ChequeBank    = record.ChequeBank    ?? string.Empty,
            ChequeNumber  = record.ChequeNumber  ?? string.Empty,
            ChequeDate    = record.ChequeDate?.ToString("dd/MM/yyyy") ?? string.Empty,
            ChequeAmount  = record.ChequeAmount,
            ChequeStatus  = record.ChequeStatus  ?? string.Empty,

            // ── Corporate characters
            Directors    = record.DirectorsList   ?? new(),
            Secretaries  = record.SecretariesList ?? new(),
            Shareholders = record.ShareholdersList ?? new(),
            Others       = record.OthersList      ?? new(),
            Officers     = record.Officers        ?? new(),

            // ── Source documents
            SourceDocuments = record.SourceDocuments ?? new()
        };
    }

    private static RecordReportModel BuildModel(TaxRecord record, string moduleTitle, string? generatedByName)
    {
        return new RecordReportModel
        {
            ModuleTitle = moduleTitle,
            GeneratedByName = string.IsNullOrWhiteSpace(generatedByName) ? "System" : generatedByName,
            GeneratedAt = DateTime.Now,
            LogoPath = ResolveLogoPath(),
            
            RecordId = record.Code ?? string.Empty,
            Code = record.Code ?? string.Empty,
            ClientName = record.ClientName ?? string.Empty,
            Branch = record.Branch ?? string.Empty,
            Date = record.Date.ToString("dd/MM/yyyy"),
            Status = record.Status ?? string.Empty,
            Category = "Tax",
            
            Period = record.TaxPeriod ?? string.Empty,
        };
    }

    private static RecordReportModel BuildModel(ClientRecord record, string moduleTitle, string? generatedByName)
    {
        return new RecordReportModel
        {
            ModuleTitle = moduleTitle,
            GeneratedByName = string.IsNullOrWhiteSpace(generatedByName) ? "System" : generatedByName,
            GeneratedAt = DateTime.Now,
            LogoPath = ResolveLogoPath(),
            
            RecordId = record.Id ?? string.Empty,
            Code = record.ClientCode ?? string.Empty,
            ClientName = record.Name ?? string.Empty,
            Branch = record.Branch ?? string.Empty,
            Date = record.Date.ToString("dd/MM/yyyy"),
            Status = record.Status,
            Category = record.Category ?? string.Empty,
            
            Email = record.Email ?? string.Empty,
            Phone = record.Phone ?? string.Empty,
            SubTotal = record.TotalRevenue,
            TotalPayment = record.TotalRevenue,
            PartialAmount = record.OutstandingBalance
        };
    }

    private static RecordReportModel BuildModel(TeamMember record, string moduleTitle, string? generatedByName)
    {
        return new RecordReportModel
        {
            ModuleTitle = moduleTitle,
            GeneratedByName = string.IsNullOrWhiteSpace(generatedByName) ? "System" : generatedByName,
            GeneratedAt = DateTime.Now,
            LogoPath = ResolveLogoPath(),
            
            RecordId = record.Id ?? string.Empty,
            ClientName = record.Username ?? string.Empty,
            Branch = record.Branch ?? string.Empty,
            Date = record.CreatedAt.ToString("dd/MM/yyyy"),
            Category = "Staff",
            
            Email = record.Email ?? string.Empty,
            Phone = record.Phone ?? string.Empty,
            RecordType = record.Role ?? string.Empty
        };
    }

    private static RecordReportModel BuildModel(NexoraRequest record, string moduleTitle, string? generatedByName)
    {
        return new RecordReportModel
        {
            ModuleTitle = moduleTitle,
            GeneratedByName = string.IsNullOrWhiteSpace(generatedByName) ? "System" : generatedByName,
            GeneratedAt = DateTime.Now,
            LogoPath = ResolveLogoPath(),
            
            RecordId = record.Id,
            ClientName = record.ClientFullName,
            CompanyName = record.CompanyName ?? string.Empty,
            Date = record.Date.ToString("dd/MM/yyyy"),
            Status = record.Status,
            Category = "Nexora Service",
            
            Phone = record.Phone,
            Assignment = record.Service,
            Notes = record.Notes
        };
    }

    private static string BuildSuggestedFileName(string? clientOrCompany, string moduleTitle)
    {
        static string Sanitize(string? s) =>
            string.IsNullOrWhiteSpace(s) ? "Unknown"
            : string.Concat(s.Select(c => char.IsLetterOrDigit(c) || c == ' ' ? c : '_'))
                    .Replace(' ', '_').Trim('_');

        var client = Sanitize(clientOrCompany);
        var module = Sanitize(moduleTitle);
        var date   = DateTime.Now.ToString("yyyy-MM-dd");

        return $"{client}_{module}_{date}.pdf";
    }

    private static string? ResolveLogoPath()
    {
        // Try to find the logo relative to the running executable.
        var baseDir = AppContext.BaseDirectory;

        var candidates = new[]
        {
            Path.Combine(baseDir, "Assets", "New Logo.png"),
            Path.Combine(baseDir, "Assets", "logo.png"),
        };

        return candidates.FirstOrDefault(File.Exists);
    }
}
