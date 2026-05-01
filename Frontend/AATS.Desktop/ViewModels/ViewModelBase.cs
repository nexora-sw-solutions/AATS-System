using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using AATS.Desktop.Services;
using AATS.Desktop.Models;

namespace AATS.Desktop.ViewModels
{
    public abstract partial class ViewModelBase : ObservableObject
    {
        [ObservableProperty] private bool _hasFormError = false;
        [ObservableProperty] private string _formErrorMessage = string.Empty;

        // Client Code autocomplete support
        private List<ClientRecord> _allClients = new();

        [ObservableProperty] private ObservableCollection<ClientRecord> _clientCodeSuggestions = new();
        [ObservableProperty] private bool _isClientCodeDropdownOpen = false;

        [ObservableProperty] private string _selectedClientId = string.Empty;

        protected async System.Threading.Tasks.Task LoadClientCodesAsync()
        {
            try
            {
                var clients = await DataService.Instance.GetClientsAsync();
                _allClients = clients
                    .Where(c => !string.IsNullOrEmpty(c.ClientCode))
                    .OrderBy(c => c.ClientCode)
                    .ToList();
            }
            catch
            {
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

        [CommunityToolkit.Mvvm.Input.RelayCommand]
        public virtual void SelectClientCode(ClientRecord client)
        {
            // This should be overridden in derived ViewModels
            IsClientCodeDropdownOpen = false;
        }

        protected string GetClientName(string code)
        {
            return _allClients.FirstOrDefault(c => c.ClientCode == code)?.Name ?? string.Empty;
        }
    }
}
