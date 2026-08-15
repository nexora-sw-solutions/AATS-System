using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AATS.Desktop.Models;
using AATS.Desktop.Services;
using AATS.Desktop.Helpers;
using System.Threading.Tasks;

namespace AATS.Desktop.ViewModels.Clients
{
    public partial class AddClientViewModel : ViewModelBase
    {
        private ClientRecord? _originalRecord;
        [ObservableProperty] private bool _isEdit;
        [ObservableProperty] private string _clientName = string.Empty;
        [ObservableProperty] private string _email = string.Empty;
        [ObservableProperty] private string _phone = string.Empty;
        [ObservableProperty] private Branch? _branch;
        [ObservableProperty] private string? _status;
        [ObservableProperty] private string? _category;
        [ObservableProperty] private string _auditorNotes = string.Empty;

        [ObservableProperty] private string _selectedAttachmentTab = "BR";

        public ObservableCollection<Branch> AvailableBranches { get; } = new();
        public List<string> Statuses { get; } = new() { "Active", "Inactive" };
        public List<string> Categories { get; } = new() { "Active", "Black Listed", "Suspended" };

        public ObservableCollection<SourceDocument> BrAttachments { get; } = new();
        public ObservableCollection<SourceDocument> TinAttachments { get; } = new();
        public ObservableCollection<SourceDocument> Form01Attachments { get; } = new();
        public ObservableCollection<SourceDocument> ArticleOfAssociationAttachments { get; } = new();
        public ObservableCollection<SourceDocument> NicAttachments { get; } = new();

        public Func<Task<string[]?>>? RequestMultipleFilePicker { get; set; }

        [ObservableProperty] private bool _hasFormError;
        [ObservableProperty] private string _formErrorMessage = string.Empty;
        [ObservableProperty] private bool _isDiscardConfirmVisible;

        public Action? GoBack { get; set; }

        public AddClientViewModel()
        {
            _ = LoadBranchesAsync();
        }

        public AddClientViewModel(ClientRecord record)
        {
            _originalRecord = record;
            IsEdit = true;
            ClientName = record.Name ?? string.Empty;
            Email = record.Email ?? string.Empty;
            Phone = record.Phone ?? string.Empty;
            Category = record.Category;
            Status = record.Status;
            AuditorNotes = record.Notes ?? string.Empty;

            if (record.BrAttachments != null) foreach (var doc in record.BrAttachments) BrAttachments.Add(doc);
            if (record.TinAttachments != null) foreach (var doc in record.TinAttachments) TinAttachments.Add(doc);
            if (record.Form01Attachments != null) foreach (var doc in record.Form01Attachments) Form01Attachments.Add(doc);
            if (record.ArticleOfAssociationAttachments != null) foreach (var doc in record.ArticleOfAssociationAttachments) ArticleOfAssociationAttachments.Add(doc);
            if (record.NicAttachments != null) foreach (var doc in record.NicAttachments) NicAttachments.Add(doc);

            _ = LoadBranchesAndSelectAsync(record);
        }

        private async Task LoadBranchesAsync()
        {
            try
            {
                var branches = await DataService.Instance.GetBranchesAsync();
                AvailableBranches.Clear();
                foreach (var b in branches)
                {
                    AvailableBranches.Add(b);
                }
                if (Branch == null && AvailableBranches.Count > 0)
                {
                    Branch = AvailableBranches[0];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Error loading branches: {ex.Message}");
            }
        }

        private async Task LoadBranchesAndSelectAsync(ClientRecord record)
        {
            try
            {
                var branches = await DataService.Instance.GetBranchesAsync();
                AvailableBranches.Clear();
                foreach (var b in branches)
                {
                    AvailableBranches.Add(b);
                }
                Branch = AvailableBranches.FirstOrDefault(b => b.Id == record.BranchId) 
                      ?? AvailableBranches.FirstOrDefault(b => b.Name == record.Branch) 
                      ?? AvailableBranches.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Error loading branches: {ex.Message}");
            }
        }

        private async Task ProcessAttachmentsUploadAsync(ObservableCollection<SourceDocument> docs, string recordType)
        {
            var localItems = docs
                .Where(d => !string.IsNullOrWhiteSpace(d.Url) && System.IO.File.Exists(d.Url))
                .ToList();

            if (localItems.Count > 0)
            {
                try
                {
                    var localPaths = localItems.Select(d => d.Url!).ToList();
                    Console.WriteLine($"[DEBUG] Uploading {localPaths.Count} {recordType} file(s) to Cloudflare R2...");
                    var uploadedDocs = await ApiService.Instance.UploadDocumentsAsync(localPaths, recordType, "");
                    for (int i = 0; i < uploadedDocs.Count && i < localItems.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(uploadedDocs[i].Url))
                        {
                            localItems[i].Url = uploadedDocs[i].Url;
                            if (!string.IsNullOrEmpty(uploadedDocs[i].FileName))
                            {
                                localItems[i].FileName = uploadedDocs[i].FileName;
                            }
                            Console.WriteLine($"[DEBUG] File '{localItems[i].FileName}' uploaded to R2: {localItems[i].Url}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WARNING] Failed to upload {recordType} files to Cloudflare R2: {ex.Message}");
                    throw;
                }
            }
        }

        [RelayCommand]
        private async Task SaveClient()
        {
            HasFormError = false;

            if (!ValidationHelper.IsValidName(ClientName))
            {
                FormErrorMessage = "Please enter a valid client name.";
                HasFormError = true;
                return;
            }

            if (!ValidationHelper.IsValidEmail(Email))
            {
                FormErrorMessage = "Please enter a valid email address.";
                HasFormError = true;
                return;
            }

            if (!ValidationHelper.IsValidPhone(Phone))
            {
                FormErrorMessage = "Please enter a valid phone number.";
                HasFormError = true;
                return;
            }

            if (Branch == null)
            {
                FormErrorMessage = "Please select a branch.";
                HasFormError = true;
                return;
            }

            if (string.IsNullOrWhiteSpace(Category))
            {
                FormErrorMessage = "Please enter or select a category.";
                HasFormError = true;
                return;
            }

            if (string.IsNullOrWhiteSpace(Status))
            {
                FormErrorMessage = "Please select a status.";
                HasFormError = true;
                return;
            }

            // Upload any local files to Cloudflare R2 and retrieve R2 public URLs
            try
            {
                await ProcessAttachmentsUploadAsync(BrAttachments, "BR");
                await ProcessAttachmentsUploadAsync(TinAttachments, "TIN");
                await ProcessAttachmentsUploadAsync(Form01Attachments, "Form01");
                await ProcessAttachmentsUploadAsync(ArticleOfAssociationAttachments, "ART");
                await ProcessAttachmentsUploadAsync(NicAttachments, "NIC");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] File upload error: {ex.Message}");
                FormErrorMessage = $"Document Upload Error: {ex.Message}";
                HasFormError = true;
                return;
            }

            if (IsEdit && _originalRecord != null)
            {
                _originalRecord.Name = ClientName;
                _originalRecord.Email = Email;
                _originalRecord.Phone = Phone;
                _originalRecord.BranchId = Branch.Id;
                _originalRecord.Branch = Branch.Name;
                _originalRecord.Category = Category;
                _originalRecord.Status = Status;
                _originalRecord.Notes = AuditorNotes;
                _originalRecord.BrAttachments = System.Linq.Enumerable.ToList(BrAttachments);
                _originalRecord.TinAttachments = System.Linq.Enumerable.ToList(TinAttachments);
                _originalRecord.Form01Attachments = System.Linq.Enumerable.ToList(Form01Attachments);
                _originalRecord.ArticleOfAssociationAttachments = System.Linq.Enumerable.ToList(ArticleOfAssociationAttachments);
                _originalRecord.NicAttachments = System.Linq.Enumerable.ToList(NicAttachments);

                try
                {
                    await DataService.Instance.UpdateClientAsync(_originalRecord);
                    LogService.Instance.AddLog("Update", "Clients", _originalRecord.Branch ?? "Central", $"Updated client: {_originalRecord.Name}");
                    GoBack?.Invoke();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DEBUG] Error updating client: {ex.Message}");
                    FormErrorMessage = $"Error: {ex.Message}";
                    HasFormError = true;
                }
                return;
            }

            var newClient = new ClientRecord
            {
                Name = ClientName,
                Email = Email,
                Phone = Phone,
                BranchId = Branch.Id,
                Branch = Branch.Name,
                Category = Category,
                Status = Status,
                Notes = AuditorNotes,
                BrAttachments = System.Linq.Enumerable.ToList(BrAttachments),
                TinAttachments = System.Linq.Enumerable.ToList(TinAttachments),
                Form01Attachments = System.Linq.Enumerable.ToList(Form01Attachments),
                ArticleOfAssociationAttachments = System.Linq.Enumerable.ToList(ArticleOfAssociationAttachments),
                NicAttachments = System.Linq.Enumerable.ToList(NicAttachments)
            };

            try
            {
                await DataService.Instance.AddClientAsync(newClient);
                LogService.Instance.AddLog("Create", "Clients", newClient.Branch ?? "Central", $"Registered new client: {newClient.Name}");
                GoBack?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Error saving client: {ex.Message}");
                FormErrorMessage = $"Error: {ex.Message}";
                HasFormError = true;
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            if (!string.IsNullOrWhiteSpace(ClientName) || 
                !string.IsNullOrWhiteSpace(Email) || 
                !string.IsNullOrWhiteSpace(Phone))
            {
                IsDiscardConfirmVisible = true;
            }
            else
            {
                GoBack?.Invoke();
            }
        }

        [RelayCommand]
        private void ConfirmDiscard()
        {
            IsDiscardConfirmVisible = false;
            GoBack?.Invoke();
        }

        [RelayCommand]
        private void CancelDiscard()
        {
            IsDiscardConfirmVisible = false;
        }

        [RelayCommand]
        private void SelectAttachmentTab(string tab) => SelectedAttachmentTab = tab;

        [RelayCommand]
        private async Task PickBrAttachment()
        {
            if (RequestMultipleFilePicker != null)
            {
                var files = await RequestMultipleFilePicker();
                if (files != null)
                {
                    foreach (var file in files) BrAttachments.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file });
                }
            }
        }

        [RelayCommand]
        private void RemoveBrAttachment(SourceDocument doc)
        {
            if (doc != null) BrAttachments.Remove(doc);
        }

        [RelayCommand]
        private async Task PickTinAttachment()
        {
            if (RequestMultipleFilePicker != null)
            {
                var files = await RequestMultipleFilePicker();
                if (files != null)
                {
                    foreach (var file in files) TinAttachments.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file });
                }
            }
        }

        [RelayCommand]
        private void RemoveTinAttachment(SourceDocument doc)
        {
            if (doc != null) TinAttachments.Remove(doc);
        }

        [RelayCommand]
        private async Task PickForm01Attachment()
        {
            if (RequestMultipleFilePicker != null)
            {
                var files = await RequestMultipleFilePicker();
                if (files != null)
                {
                    foreach (var file in files) Form01Attachments.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file });
                }
            }
        }

        [RelayCommand]
        private void RemoveForm01Attachment(SourceDocument doc)
        {
            if (doc != null) Form01Attachments.Remove(doc);
        }

        [RelayCommand]
        private async Task PickArticleOfAssociationAttachment()
        {
            if (RequestMultipleFilePicker != null)
            {
                var files = await RequestMultipleFilePicker();
                if (files != null)
                {
                    foreach (var file in files) ArticleOfAssociationAttachments.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file });
                }
            }
        }

        [RelayCommand]
        private void RemoveArticleOfAssociationAttachment(SourceDocument doc)
        {
            if (doc != null) ArticleOfAssociationAttachments.Remove(doc);
        }

        [RelayCommand]
        private async Task PickNicAttachment()
        {
            if (RequestMultipleFilePicker != null)
            {
                var files = await RequestMultipleFilePicker();
                if (files != null)
                {
                    foreach (var file in files) NicAttachments.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file });
                }
            }
        }

        [RelayCommand]
        private void RemoveNicAttachment(SourceDocument doc)
        {
            if (doc != null) NicAttachments.Remove(doc);
        }

        [RelayCommand]
        private void PreviewAttachment(SourceDocument doc)
        {
            var filePath = doc?.Url ?? doc?.FileName;
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(filePath) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DEBUG] Error previewing document: {ex.Message}");
                }
            }
        }
    }
}
