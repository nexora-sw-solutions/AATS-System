using System;
using System.IO;
using System.Diagnostics;
using System.Drawing.Printing;
using PdfiumViewer;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AATS.Desktop.Models;
using AATS.Desktop.Services;

namespace AATS.Desktop.ViewModels
{
    public class ProcessStep : ObservableObject
    {
        private int _number;
        private string _name = string.Empty;
        private bool _isActive;
        private bool _isClickable;
        private bool _isIconStep;
        private string _iconValue = string.Empty;

        private bool _isLast;
 
        public int Number { get => _number; set => SetProperty(ref _number, value); }
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public bool IsActive { get => _isActive; set => SetProperty(ref _isActive, value); }
        public bool IsClickable { get => _isClickable; set => SetProperty(ref _isClickable, value); }
        public bool IsIconStep { get => _isIconStep; set => SetProperty(ref _isIconStep, value); }
        public string IconValue { get => _iconValue; set => SetProperty(ref _iconValue, value); }
        public bool IsLast 
        { 
            get => _isLast; 
            set 
            {
                if (SetProperty(ref _isLast, value))
                {
                    OnPropertyChanged(nameof(IsNotLast));
                }
            } 
        }
        
        public bool IsNotLast => !IsLast;
    }

    public abstract partial class DetailViewModelBase : ViewModelBase
    {
        [ObservableProperty]
        private AuditRecord? _record;

        partial void OnRecordChanged(AuditRecord? value)
        {
            OnRecordLoaded(value);
        }

        protected virtual void OnRecordLoaded(AuditRecord? value) { }

        [ObservableProperty]
        private ObservableCollection<ProcessStep> _steps = new();

        [ObservableProperty]
        private bool _isConfirmDialogVisible;

        [ObservableProperty] private bool _isBusy;
        [ObservableProperty]
        private string _confirmDialogTitle = string.Empty;

        [ObservableProperty]
        private string _confirmDialogMessage = string.Empty;

        [ObservableProperty]
        private bool _isGuideVisible;

        protected Func<System.Threading.Tasks.Task>? ConfirmActionDelegate;
        private int _pendingStep;

        public Action? NavigateBack { get; set; }
        public Action<AuditRecord>? NavigateToEditRecord { get; set; }
        public Func<System.Threading.Tasks.Task>? DeleteRecordAction { get; set; }

        public virtual string GuideTitle => "Client Details Guide";
        public virtual string GuideDescription => "Review the overall progress and specific information for this client record.";
        public virtual string GuideProTip => "Use the process timeline to update the status of the clearance or registration.";

        public DetailViewModelBase(AuditRecord record)
        {
            Record = record;
        }

        // Common record properties
        public string ClientName => Record?.ClientName ?? "N/A";
        public string CompanyName => Record?.Company ?? "N/A";
        public string ID => Record?.Code ?? Record?.ID ?? "N/A";
        public string Code => Record?.Code ?? "N/A";
        public string IDDisplay => Record?.Code ?? Record?.ID ?? "N/A";
        public string Date => (Record != null && Record.Date.Year > 1) 
            ? Record.Date.ToString("yyyy-MM-dd") 
            : ((Record != null && Record.CreatedAt.Year > 1) 
                ? Record.CreatedAt.ToString("yyyy-MM-dd") 
                : DateTime.UtcNow.ToString("yyyy-MM-dd"));

        public string DateDisplay => (Record != null && Record.Date.Year > 1) 
            ? Record.Date.ToString("dd/MM/yyyy") 
            : ((Record != null && Record.CreatedAt.Year > 1) 
                ? Record.CreatedAt.ToString("dd/MM/yyyy") 
                : DateTime.UtcNow.ToString("dd/MM/yyyy"));
        public string BranchDisplay => Record?.Branch ?? "N/A";
        public string StatusDisplay => Record?.PaymentStatus ?? "N/A";
        public string AssignmentDisplay => Record?.Assignment ?? "N/A";
        public string PaymentOption => Record?.PaymentOption ?? "N/A";
        public string PaymentStatus => Record?.PaymentStatus ?? "N/A";
        public string Assignment => Record?.Assignment ?? "N/A";
        public string Period => Record?.Period ?? "N/A";
        public string ClientCategory => Record?.ClientCategory ?? string.Empty;
        public string ClientCategoryColor => Record?.ClientCategoryColor ?? "Transparent";
        public bool HasClientCategory => Record?.HasClientCategory ?? false;
        public ObservableCollection<SourceDocument> SourceDocuments => new(Record?.SourceDocuments ?? new List<SourceDocument>());

        // Partial payment helper properties
        public decimal PartiallyPaidAmount => Record?.PartialAmount ?? 0.00m;
        public decimal OutstandingBalance => Math.Max(0, (Record?.SubTotal ?? 0) - (Record?.Discount ?? 0) - (Record?.PartialAmount ?? 0));
        public decimal TotalPaymentDue => OutstandingBalance;
        public bool IsPaymentStatusPartial => PaymentStatus == "Partial";

        protected abstract void InitializeSteps();

        protected void SetupSteps(IEnumerable<(string Name, string? Icon)> stepDefs)
        {
            Steps.Clear();
            int count = 1;
            foreach (var (name, icon) in stepDefs)
            {
                Steps.Add(new ProcessStep
                {
                    Number = count,
                    Name = name,
                    IsIconStep = !string.IsNullOrEmpty(icon),
                    IconValue = icon ?? string.Empty
                });
                count++;
            }
            UpdateStepStates();
            
            // Mark the last step for UI line visibility
            if (Steps.Count > 0)
            {
                Steps[Steps.Count - 1].IsLast = true;
            }
        }

        protected virtual void UpdateStepStates()
        {
            if (Record == null) return;

            for (int i = 0; i < Steps.Count; i++)
            {
                int stepNum = i + 1;
                Steps[i].IsActive = stepNum <= Record.CurrentStep;
                
                // Clickable logic: 
                // 1. Next step (Forward Sequential)
                // 2. Any previous step (Backward correction)
                Steps[i].IsClickable = (stepNum == Record.CurrentStep + 1) || (stepNum < Record.CurrentStep);
            }
        }

        [RelayCommand]
        public void StepClick(ProcessStep step)
        {
            if (!step.IsClickable) return;

            _pendingStep = step.Number;
            ConfirmDialogTitle = step.Number > Record!.CurrentStep ? "Advance Process?" : "Move Back Process?";
            ConfirmDialogMessage = $"Are you sure you want to change the status to '{step.Name}'?";
            ConfirmActionDelegate = () =>
            {
                ApplyStepChange();
                return System.Threading.Tasks.Task.CompletedTask;
            };
            IsConfirmDialogVisible = true;
        }

        private void ApplyStepChange()
        {
            if (Record != null)
            {
                var step = Steps.FirstOrDefault(s => s.Number == _pendingStep);
                if (step != null)
                {
                    Action proceed = async () =>
                    {
                        Record.CurrentStep = _pendingStep;
                        Record.Status = step.Name.ToUpper();
                        Record.Process = step.Name.ToUpper();
                        
                        try 
                        {
                            await SaveRecordUpdateAsync();
                            NotificationService.Instance.AddNotification("Success", $"Status updated to {Record.Status}");
                        }
                        catch (Exception ex)
                        {
                            NotificationService.Instance.AddNotification("Error", $"Failed to update status: {ex.Message}");
                        }
                        
                        UpdateStepStates();
                        Refresh();
                    };

                    if (MainViewModel.Instance != null)
                    {
                        MainViewModel.Instance.ExecuteAuthorizedAction(proceed);
                    }
                    else
                    {
                        proceed();
                    }
                }
            }
        }

        public virtual string Category => "Assurance";

        protected virtual async System.Threading.Tasks.Task SaveRecordUpdateAsync()
        {
            if (Record != null)
            {
                await DataService.Instance.UpdateAuditRecordAsync(Category, Record);
            }
        }

        [RelayCommand]
        public virtual async System.Threading.Tasks.Task ConfirmAction()
        {
            IsConfirmDialogVisible = false;
            if (ConfirmActionDelegate != null)
            {
                await ConfirmActionDelegate.Invoke();
            }
            ConfirmActionDelegate = null;
        }

        [RelayCommand]
        public virtual void CancelAction()
        {
            IsConfirmDialogVisible = false;
            ConfirmActionDelegate = null;
            _pendingStep = 0;
        }

        [RelayCommand] public void EditRecord() => OnEditRecord();
        [RelayCommand] public void DeleteRecord() => OnDeleteRecord();
        [RelayCommand] public void DownloadRecord() => OnDownloadRecord();
        [RelayCommand] public void PrintRecord() => OnPrintRecord();

        public virtual void OnEditRecord()
        {
            if (Record != null)
                NavigateToEditRecord?.Invoke(Record);
        }

        public virtual void OnDeleteRecord()
        {
            ConfirmDialogTitle = "Delete Record?";
            ConfirmDialogMessage = $"Are you sure you want to delete record '{ID}' for '{ClientName}'? This action cannot be undone.";
            ConfirmActionDelegate = async () =>
            {
                if (DeleteRecordAction != null)
                {
                    await DeleteRecordAction.Invoke();
                }
                NavigateBack?.Invoke();
            };
            IsConfirmDialogVisible = true;
        }

        protected virtual async void OnDownloadRecord()
        {
            if (Record == null) return;
            try
            {
                IsBusy = true;
                await RecordReportService.Instance.DownloadReportAsync(Record, Category, GetCurrentUserName());
            }
            catch (Exception ex)
            {
                NotificationService.Instance.AddNotification("Error", $"Could not generate report: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected virtual async void OnPrintRecord()
        {
            if (Record == null) return;
            try
            {
                IsBusy = true;
                await RecordReportService.Instance.PrintReportAsync(Record, Category, GetCurrentUserName());
            }
            catch (Exception ex)
            {
                NotificationService.Instance.AddNotification("Error", $"Could not print report: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>Returns the currently logged-in user's display name, or "System" as fallback.</summary>
        private static string GetCurrentUserName()
        {
            try
            {
                var user = MainViewModel.Instance?.CurrentUser;
                return string.IsNullOrWhiteSpace(user?.Username) ? "System" : user.Username;
            }
            catch
            {
                return "System";
            }
        }

        [RelayCommand]
        private void OpenGuide() => IsGuideVisible = true;

        [RelayCommand]
        private void CloseGuide() => IsGuideVisible = false;

        /// <summary>
        /// Downloads or opens the given source document.
        /// If the path is a remote URL (http/https), it downloads via ApiService to the user's Downloads folder.
        /// If it is a local path, it opens the file directly with the default application.
        /// </summary>
        [RelayCommand]
        public async System.Threading.Tasks.Task DownloadSourceDocument(SourceDocument doc)
        {
            if (doc == null) return;

            var target = doc.Url ?? doc.FileName;
            if (string.IsNullOrWhiteSpace(target)) return;

            try
            {
                if (target.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    // Remote file — download through ApiService
                    var fileName = doc.FileName ?? System.IO.Path.GetFileName(new Uri(target).LocalPath);
                    if (string.IsNullOrWhiteSpace(fileName)) fileName = "download";
                    await ApiService.Instance.DownloadDocumentAsync(target, fileName);
                    NotificationService.Instance.AddNotification("Downloaded", $"'{fileName}' saved to Downloads.");
                }
                else if (System.IO.File.Exists(target))
                {
                    // Local file — open directly
                    System.Diagnostics.Process.Start(
                        new System.Diagnostics.ProcessStartInfo(target) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                NotificationService.Instance.AddNotification("Error", $"Could not download file: {ex.Message}");
            }
        }

        [RelayCommand]
        public async System.Threading.Tasks.Task PrintSourceDocument(SourceDocument doc)
        {
            if (doc == null) return;

            var target = doc.Url ?? doc.FileName;
            if (string.IsNullOrWhiteSpace(target)) return;

            string? localFilePath = null;
            bool isTempFile = false;

            try
            {
                IsBusy = true;

                // 1. Retrieve the file (download if remote, check existence if local)
                if (target.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    var ext = System.IO.Path.GetExtension(doc.FileName ?? new Uri(target).LocalPath);
                    if (string.IsNullOrEmpty(ext)) ext = ".pdf";
                    
                    localFilePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid()}{ext}");
                    var bytes = await ApiService.Instance.Client.GetByteArrayAsync(target);
                    await System.IO.File.WriteAllBytesAsync(localFilePath, bytes);
                    isTempFile = true;
                }
                else if (System.IO.File.Exists(target))
                {
                    localFilePath = target;
                }
                else
                {
                    throw new FileNotFoundException("The specified source document file could not be found locally.", target);
                }

                // 2. Identify file extension and print accordingly
                var fileExtension = System.IO.Path.GetExtension(localFilePath).ToLowerInvariant();

                if (fileExtension == ".pdf")
                {
                    await PrintPdfAsync(localFilePath);
                }
                else if (fileExtension == ".png" || fileExtension == ".jpg" || fileExtension == ".jpeg")
                {
                    await PrintImageAsync(localFilePath);
                }
                else if (fileExtension == ".docx" || fileExtension == ".doc" || 
                         fileExtension == ".xlsx" || fileExtension == ".xls" || 
                         fileExtension == ".pptx" || fileExtension == ".ppt")
                {
                    await PrintOfficeDocumentAsync(localFilePath);
                }
                else
                {
                    throw new NotSupportedException($"Printing files with extension '{fileExtension}' is not supported.");
                }
            }
            catch (Exception ex)
            {
                NotificationService.Instance.AddNotification("Error", $"Printing failed: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
                
                // Cleanup remote downloaded temp file (deferred to allow native print processes to load the file)
                if (isTempFile && !string.IsNullOrEmpty(localFilePath))
                {
                    _ = System.Threading.Tasks.Task.Run(async () =>
                    {
                        await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(60));
                        try
                        {
                            if (System.IO.File.Exists(localFilePath))
                            {
                                System.IO.File.Delete(localFilePath);
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to delete temp file: {ex.Message}");
                        }
                    });
                }
            }
        }

        private async System.Threading.Tasks.Task PrintViaShellVerbAsync(string filePath)
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                bool launched = false;
                try
                {
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = filePath,
                        Verb = "print",
                        UseShellExecute = true,
                        CreateNoWindow = true,
                        WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                    };
                    System.Diagnostics.Process.Start(psi);
                    launched = true;
                    System.Diagnostics.Debug.WriteLine($"[DetailViewModelBase] Print verb launched for {filePath}.");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[DetailViewModelBase] Print verb failed ({ex.Message}), falling back to open.");
                }

                if (!launched)
                {
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(filePath)
                        {
                            UseShellExecute = true
                        });
                        Avalonia.Threading.Dispatcher.UIThread.Post(() => 
                        {
                            NotificationService.Instance.AddNotification("Document Opened", "Document opened in viewer — press Ctrl+P to print.");
                        });
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[DetailViewModelBase] Fallback open failed: {ex.Message}");
                        throw new InvalidOperationException("Could not open the document for printing.", ex);
                    }
                }
                else
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() => 
                    {
                        NotificationService.Instance.AddNotification("Printing", "The document has been sent to the print dialog.");
                    });
                }
            });
        }

        private async System.Threading.Tasks.Task PrintPdfAsync(string pdfPath)
        {
            await PrintViaShellVerbAsync(pdfPath);
        }

        private async System.Threading.Tasks.Task PrintImageAsync(string imagePath)
        {
            await PrintViaShellVerbAsync(imagePath);
        }

        private async System.Threading.Tasks.Task PrintOfficeDocumentAsync(string officePath)
        {
            var sofficePath = FindLibreOfficePath();
            if (string.IsNullOrEmpty(sofficePath) || !System.IO.File.Exists(sofficePath))
            {
                throw new FileNotFoundException("LibreOffice installation could not be found. Please install LibreOffice to print office documents.");
            }

            var tempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
            System.IO.Directory.CreateDirectory(tempDir);
            
            string? generatedPdfPath = null;
            try
            {
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = sofficePath,
                    Arguments = $"--headless --convert-to pdf --outdir \"{tempDir}\" \"{officePath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var process = System.Diagnostics.Process.Start(startInfo))
                {
                    if (process == null)
                        throw new Exception("Failed to start LibreOffice conversion process.");

                    await process.WaitForExitAsync();
                    if (process.ExitCode != 0)
                    {
                        var err = await process.StandardError.ReadToEndAsync();
                        throw new Exception($"LibreOffice conversion failed (Code {process.ExitCode}): {err}");
                    }
                }

                var expectedPdfName = System.IO.Path.GetFileNameWithoutExtension(officePath) + ".pdf";
                generatedPdfPath = System.IO.Path.Combine(tempDir, expectedPdfName);

                if (!System.IO.File.Exists(generatedPdfPath))
                {
                    var pdfFiles = System.IO.Directory.GetFiles(tempDir, "*.pdf");
                    if (pdfFiles.Length > 0)
                    {
                        generatedPdfPath = pdfFiles[0];
                    }
                    else
                    {
                        throw new FileNotFoundException("LibreOffice conversion completed, but the generated PDF file could not be found.");
                    }
                }

                await PrintPdfAsync(generatedPdfPath);
            }
            finally
            {
                _ = System.Threading.Tasks.Task.Run(async () =>
                {
                    await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(60));
                    try
                    {
                        if (System.IO.Directory.Exists(tempDir))
                        {
                            System.IO.Directory.Delete(tempDir, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to clean up LibreOffice temp dir: {ex.Message}");
                    }
                });
            }
        }

        private string FindLibreOfficePath()
        {
            string[] commonPaths = new[]
            {
                @"C:\Program Files\LibreOffice\program\soffice.exe",
                @"C:\Program Files (x86)\LibreOffice\program\soffice.exe"
            };

            foreach (var path in commonPaths)
            {
                if (System.IO.File.Exists(path))
                    return path;
            }

            try
            {
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\soffice.exe"))
                {
                    if (key != null)
                    {
                        var value = key.GetValue("") as string;
                        if (!string.IsNullOrEmpty(value) && System.IO.File.Exists(value))
                            return value;
                    }
                }
            }
            catch { }

            return "soffice.exe";
        }

        [RelayCommand]
        public virtual async System.Threading.Tasks.Task LoadFullRecordAsync()
        {
            try
            {
                if (Record != null && !string.IsNullOrEmpty(Record.ID))
                {
                    IsBusy = true;
                    var fullRecord = await DataService.Instance.GetRecordByIdAsync(Category, Record.ID);
                    if (fullRecord != null)
                    {
                        Record = fullRecord;
                        
                        // Fetch client details to get the category if it's not present in the record
                        if (string.IsNullOrEmpty(Record.ClientCategory) && (Record.ClientId != null || !string.IsNullOrEmpty(Record.ClientCode)))
                        {
                            string idToFetch = Record.ClientId?.ToString() ?? Record.ClientCode!;
                            var client = await DataService.Instance.GetClientByIdAsync(idToFetch);
                            if (client != null)
                            {
                                Record.ClientCategory = client.Category;
                            }
                        }
                        
                        Refresh();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading full record: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public virtual void Refresh()
        {
            OnPropertyChanged(nameof(ID));
            OnPropertyChanged(nameof(Code));
            OnPropertyChanged(nameof(ClientName));
            OnPropertyChanged(nameof(Assignment));
            OnPropertyChanged(nameof(Date));
            OnPropertyChanged(nameof(PaymentOption));
            OnPropertyChanged(nameof(PaymentStatus));
            OnPropertyChanged(nameof(StatusDisplay));
            OnPropertyChanged(nameof(Period));
            OnPropertyChanged(nameof(SourceDocuments));
            OnPropertyChanged(nameof(ClientCategory));
            OnPropertyChanged(nameof(ClientCategoryColor));
            OnPropertyChanged(nameof(HasClientCategory));
            OnPropertyChanged(nameof(PartiallyPaidAmount));
            OnPropertyChanged(nameof(OutstandingBalance));
            OnPropertyChanged(nameof(TotalPaymentDue));
            OnPropertyChanged(nameof(IsPaymentStatusPartial));
        }
    }
}
