using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AATS.Desktop.Models;
using AATS.Desktop.Services;
using AATS.Desktop.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AATS.Desktop.Views.Reports;
using AATS.Desktop.ViewModels.Reports;

namespace AATS.Desktop.ViewModels.Nexora
{
    public partial class NexoraViewModel : ViewModelBase
    {
        private readonly List<NexoraRequest> _allRequests = new();
        [ObservableProperty] private ObservableCollection<NexoraRequest> _filteredRequests = new();
        public Func<Task<List<string>>>? RequestFilePicker { get; set; }
        
        // Form Fields
        [ObservableProperty] private string _requestId = "Auto-Generated";
        [ObservableProperty] private DateTime? _selectedDate = DateTime.Now;
        [ObservableProperty] private string _clientFirstName = string.Empty;
        [ObservableProperty] private string _clientLastName = string.Empty;
        [ObservableProperty] private string _companyName = string.Empty;
        [ObservableProperty] private string _selectedService = string.Empty;
        [ObservableProperty] private string _phone = string.Empty;
        [ObservableProperty] private string _notes = string.Empty;
        [ObservableProperty] private ObservableCollection<string> _uploadedFiles = new();
        [ObservableProperty] private ObservableCollection<ApiDocument> _currentRequestDocuments = new();
        
        [ObservableProperty] private ObservableCollection<ClientRecord> _clients = new();
        [ObservableProperty] private ClientRecord? _selectedClient;
        
        [ObservableProperty] private Guid? _clientId;
        [ObservableProperty] private Guid? _branchId;
        
        // Filters
        [ObservableProperty] private string _searchText = string.Empty;
        [ObservableProperty] private string _selectedServiceFilter = "All Services";
        public ObservableCollection<string> ServiceFilters { get; } = new() 
        { 
            "All Services", "Accounting Software", "Payroll Management", 
            "KOT System", "POS System", "Website", 
            "Marketing & Digital Marketing", "Other" 
        };

        // Selection
        [ObservableProperty] private bool _isAllSelected;
        [ObservableProperty] private NexoraRequest? _selectedRequest;
        [ObservableProperty] private bool _isDrawerOpen = false;

        public NexoraViewModel()
        {
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var requestsTask = DataService.Instance.GetNexoraRequestsAsync();
                var clientsTask = DataService.Instance.GetClientsAsync();

                await Task.WhenAll(requestsTask, clientsTask);

                var requests = await requestsTask;
                _allRequests.Clear();
                _allRequests.AddRange(requests);

                var clientsList = await clientsTask;
                Clients = new ObservableCollection<ClientRecord>(clientsList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading Nexora data: {ex.Message}");
            }

            ApplyFilters();
        }


        [RelayCommand]
        private async Task SubmitRequest()
        {
            HasFormError = false;

            if (!ValidationHelper.IsValidName(ClientFirstName) || !ValidationHelper.IsValidName(CompanyName))
            {
                FormErrorMessage = "Please enter valid first name and company name.";
                HasFormError = true;
                return;
            }

            var newRequest = new NexoraRequest
            {
                Id = RequestId,
                Date = SelectedDate ?? DateTime.Now,
                ClientFirstName = ClientFirstName,
                ClientLastName = ClientLastName,
                CompanyName = CompanyName,
                Service = SelectedService,
                Notes = Notes,
                Phone = Phone,
                Status = "Pending",
                ClientId = ClientId,
                BranchId = BranchId
            };

            try
            {
                var savedRequest = await DataService.Instance.AddNexoraRequestAsync(newRequest);
                var actualRequest = savedRequest ?? newRequest;
                
                if (savedRequest != null && UploadedFiles.Count > 0)
                {
                    var recordId = savedRequest.DbId;
                    var localFiles = UploadedFiles.ToList();
                    var uploaded = await ApiService.Instance.UploadDocumentsAsync(localFiles, "Nexora", recordId.ToString());
                    foreach (var u in uploaded)
                    {
                        var newDoc = new ApiDocument
                        {
                            Id = Guid.NewGuid(),
                            RecordId = recordId,
                            RecordType = "Nexora",
                            FileName = u.FileName,
                            StorageKey = u.Url,
                            Category = "General"
                        };
                        await ApiService.Instance.PostAsync<ApiDocument>("/api/v1/documents", newDoc);
                    }
                }

                _allRequests.Insert(0, actualRequest);
                
                CancelRequest(); // Reset form
                ApplyFilters();
            }
            catch (Exception ex)
            {
                FormErrorMessage = $"Error saving request: {ex.Message}";
                HasFormError = true;
            }
        }

        [RelayCommand]
        private void CancelRequest()
        {
            SelectedClient = null;
            ClientFirstName = string.Empty;
            ClientLastName = string.Empty;
            CompanyName = string.Empty;
            SelectedService = string.Empty;
            Phone = string.Empty;
            Notes = string.Empty;
            UploadedFiles.Clear();
        }

        [RelayCommand]
        private async Task OpenDetail(NexoraRequest detail)
        {
            SelectedRequest = detail;
            IsDrawerOpen = true;
            await LoadCurrentRequestDocumentsAsync(detail.DbId);
        }

        private async Task LoadCurrentRequestDocumentsAsync(Guid recordId)
        {
            CurrentRequestDocuments.Clear();
            try
            {
                var response = await ApiService.Instance.GetAsync<ApiResponse<PaginatedResult<ApiDocument>>>("/api/v1/documents");
                if (response?.Data?.Items != null)
                {
                    var docs = response.Data.Items
                        .Where(d => d.RecordId == recordId && d.RecordType.Equals("Nexora", StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    foreach (var doc in docs)
                    {
                        CurrentRequestDocuments.Add(doc);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading documents for request: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task DownloadDocument(ApiDocument doc)
        {
            if (doc == null || string.IsNullOrEmpty(doc.StorageKey)) return;
            try
            {
                await ApiService.Instance.DownloadDocumentAsync(doc.StorageKey, doc.FileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading document: {ex.Message}");
            }
        }

        [RelayCommand]
        private void CloseDrawer()
        {
            IsDrawerOpen = false;
        }

        [RelayCommand]
        public async Task PrintReport()
        {
            if (SelectedRequest == null) return;
            try {
                await RecordReportService.Instance.PrintReportAsync(SelectedRequest, "Nexora", MainViewModel.Instance?.CurrentUser?.Username ?? "System");
            } catch (Exception ex) {
                NotificationService.Instance.AddNotification("Error", $"Could not print report: {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task DownloadReport()
        {
            if (SelectedRequest == null) return;
            try {
                await RecordReportService.Instance.DownloadReportAsync(SelectedRequest, "Nexora", MainViewModel.Instance?.CurrentUser?.Username ?? "System");
            } catch (Exception ex) {
                NotificationService.Instance.AddNotification("Error", $"Could not download report: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task UploadDocument()
        {
            if (RequestFilePicker == null) return;
            var files = await RequestFilePicker();
            if (files != null)
            {
                foreach (var file in files)
                {
                    if (!UploadedFiles.Contains(file))
                        UploadedFiles.Add(file);
                }
            }
        }

        [RelayCommand]
        private void RemoveDocument(string fileName)
        {
            UploadedFiles.Remove(fileName);
        }

        partial void OnIsAllSelectedChanged(bool value)
        {
            foreach (var r in FilteredRequests)
            {
                r.IsSelected = value;
            }
        }

        partial void OnSelectedClientChanged(ClientRecord? value)
        {
            if (value != null)
            {
                ClientFirstName = value.Name ?? string.Empty;
                ClientLastName = string.Empty;
                CompanyName = value.Name ?? string.Empty;
                Phone = value.Phone ?? string.Empty;
                
                if (Guid.TryParse(value.Id, out Guid cid))
                {
                    ClientId = cid;
                }
                else
                {
                    ClientId = null;
                }
                
                BranchId = value.BranchId;
            }
            else
            {
                ClientFirstName = string.Empty;
                ClientLastName = string.Empty;
                CompanyName = string.Empty;
                Phone = string.Empty;
                ClientId = null;
                BranchId = null;
            }
        }

        partial void OnSearchTextChanged(string value) => ApplyFilters();
        partial void OnSelectedServiceFilterChanged(string value) => ApplyFilters();

        private void ApplyFilters()
        {
            var filtered = _allRequests.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(r => 
                    r.Id.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    r.CompanyName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    r.ClientFullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            if (SelectedServiceFilter != "All Services")
            {
                filtered = filtered.Where(r => r.Service == SelectedServiceFilter);
            }

            FilteredRequests = new ObservableCollection<NexoraRequest>(filtered);
        }
    }
}
