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

namespace AATS.Desktop.ViewModels.Nexora
{
    public partial class NexoraViewModel : ViewModelBase
    {
        private readonly List<NexoraRequest> _allRequests = new();
        [ObservableProperty] private ObservableCollection<NexoraRequest> _filteredRequests = new();
        public Func<Task<List<string>>>? RequestFilePicker { get; set; }
        
        // Form Fields
        [ObservableProperty] private string _requestId = "NEX-005";
        [ObservableProperty] private DateTime? _selectedDate = DateTime.Now;
        [ObservableProperty] private string _clientFirstName = string.Empty;
        [ObservableProperty] private string _clientLastName = string.Empty;
        [ObservableProperty] private string _companyName = string.Empty;
        [ObservableProperty] private string _selectedService = string.Empty;
        [ObservableProperty] private string _notes = string.Empty;
        [ObservableProperty] private ObservableCollection<string> _uploadedFiles = new();
        
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
            var requests = await DataService.Instance.GetNexoraRequestsAsync();
            _allRequests.Clear();
            _allRequests.AddRange(requests);
            ApplyFilters();
        }


        [RelayCommand]
        private void SubmitRequest()
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
                Phone = "+94 7X XXX XXXX", // Default for mock
                Status = "Pending"
            };

            _allRequests.Insert(0, newRequest);
            
            // Auto-generate next ID
            var nextNum = int.Parse(RequestId.Split('-').Last()) + 1;
            RequestId = $"NEX-{nextNum:D3}";
            
            CancelRequest(); // Reset form
            ApplyFilters();
        }

        [RelayCommand]
        private void CancelRequest()
        {
            ClientFirstName = string.Empty;
            ClientLastName = string.Empty;
            CompanyName = string.Empty;
            SelectedService = string.Empty;
            Notes = string.Empty;
            UploadedFiles.Clear();
        }

        [RelayCommand]
        private void OpenDetail(NexoraRequest detail)
        {
            SelectedRequest = detail;
            IsDrawerOpen = true;
        }

        [RelayCommand]
        private void CloseDrawer()
        {
            IsDrawerOpen = false;
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
