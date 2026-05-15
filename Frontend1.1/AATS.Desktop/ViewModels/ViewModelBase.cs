using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using AATS.Desktop.Services;
using AATS.Desktop.Models;

using AATS.Desktop.Helpers;

namespace AATS.Desktop.ViewModels
{
    public abstract partial class ViewModelBase : ObservableObject
    {
        [ObservableProperty] private bool _hasFormError = false;
        [ObservableProperty] private string _formErrorMessage = string.Empty;

        public List<string> Banks => BankHelper.GetBanks();

        // Client Code autocomplete support
        private List<ClientRecord> _allClients = new();

        [ObservableProperty] private ObservableCollection<ClientRecord> _clientCodeSuggestions = new();
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsAnyDropdownOpen))]
        private bool _isClientCodeDropdownOpen = false;

        // Bank autocomplete support
        [ObservableProperty] private ObservableCollection<string> _bankSuggestions = new();
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsAnyDropdownOpen))]
        private bool _isBankDropdownOpen = false;

        public bool IsAnyDropdownOpen => IsClientCodeDropdownOpen || IsBankDropdownOpen;

        [ObservableProperty] private string _selectedClientId = string.Empty;

        protected async System.Threading.Tasks.Task LoadClientCodesAsync(string? currentText = null)
        {
            try
            {
                var clients = await DataService.Instance.GetClientsAsync();
                _allClients = clients
                    .Where(c => !string.IsNullOrEmpty(c.ClientCode) && c.IsActiveStatus)
                    .OrderBy(c => c.ClientCode)
                    .ToList();

                // Re-trigger filter in case user started typing before load completed
                if (!string.IsNullOrEmpty(currentText))
                {
                    FilterClientCodes(currentText);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading client codes: {ex.Message}");
                _allClients = new List<ClientRecord>();
            }
        }

        protected void FilterClientCodes(string? text)
        {
            ClientCodeSuggestions.Clear();

            if (string.IsNullOrWhiteSpace(text))
            {
                foreach (var client in _allClients)
                    ClientCodeSuggestions.Add(client);
            }
            else
            {
                var filtered = _allClients
                    .Where(c => c.ClientCode!.Contains(text, StringComparison.OrdinalIgnoreCase) || 
                               (c.Name != null && c.Name.Contains(text, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
                foreach (var client in filtered)
                    ClientCodeSuggestions.Add(client);
            }

            IsClientCodeDropdownOpen = ClientCodeSuggestions.Count > 0;
        }

        protected void FilterBanks(string? text)
        {
            BankSuggestions.Clear();
            var allBanks = BankHelper.GetBanks();

            if (string.IsNullOrWhiteSpace(text))
            {
                foreach (var bank in allBanks)
                    BankSuggestions.Add(bank);
            }
            else
            {
                var filtered = allBanks
                    .Where(b => b.Contains(text, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                foreach (var bank in filtered)
                    BankSuggestions.Add(bank);
            }

            IsBankDropdownOpen = BankSuggestions.Count > 0;
        }

        [CommunityToolkit.Mvvm.Input.RelayCommand]
        public virtual void SelectClientCode(ClientRecord client)
        {
            // This should be overridden in derived ViewModels
            IsClientCodeDropdownOpen = false;
        }

        [CommunityToolkit.Mvvm.Input.RelayCommand]
        public virtual void SelectBank(string bank)
        {
            // This should be overridden in derived ViewModels
            IsBankDropdownOpen = false;
        }

        protected string GetClientName(string code)
        {
            return _allClients.FirstOrDefault(c => c.ClientCode == code)?.Name ?? string.Empty;
        }
    }
}
