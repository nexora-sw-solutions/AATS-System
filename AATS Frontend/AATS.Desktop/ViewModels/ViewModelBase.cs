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
        protected List<ClientRecord> SharedClientsList => _allClients;

        [ObservableProperty] private ObservableCollection<ClientRecord> _clientCodeSuggestions = new();
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsAnyDropdownOpen))]
        private bool _isClientCodeDropdownOpen = false;

        [ObservableProperty] private int _highlightedSuggestionIndex = -1;

        partial void OnIsClientCodeDropdownOpenChanged(bool value)
        {
            if (!value)
            {
                HighlightedSuggestionIndex = -1;
            }
        }

        protected bool _isSelectingClient = false;

        // Bank autocomplete support
        [ObservableProperty] private ObservableCollection<string> _bankSuggestions = new();
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsAnyDropdownOpen))]
        private bool _isBankDropdownOpen = false;

        public bool IsAnyDropdownOpen => IsClientCodeDropdownOpen || IsBankDropdownOpen;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SelectedClientCategoryColor))]
        [NotifyPropertyChangedFor(nameof(HasSelectedClientCategory))]
        private ClientRecord? _selectedClient;

        public string SelectedClientCategoryColor => SelectedClient?.CategoryColor ?? "Transparent";
        public bool HasSelectedClientCategory => SelectedClient != null && SelectedClient.CategoryColor != "Transparent";

        [ObservableProperty] private string _selectedClientId = string.Empty;

        partial void OnSelectedClientChanged(ClientRecord? value)
        {
            if (value != null)
            {
                _ = LoadClientDocumentsAndNotifyAsync(value);
            }
        }

        private async System.Threading.Tasks.Task LoadClientDocumentsAndNotifyAsync(ClientRecord client)
        {
            if (client != null && !string.IsNullOrEmpty(client.Id))
            {
                try
                {
                    var freshClient = await DataService.Instance.GetClientByIdAsync(client.Id);
                    if (freshClient != null)
                    {
                        client.BrAttachments = freshClient.BrAttachments;
                        client.TinAttachments = freshClient.TinAttachments;
                        client.Form01Attachments = freshClient.Form01Attachments;
                        client.ArticleOfAssociationAttachments = freshClient.ArticleOfAssociationAttachments;
                        client.NicAttachments = freshClient.NicAttachments;
                        
                        if (string.IsNullOrEmpty(client.Email)) client.Email = freshClient.Email;
                        if (string.IsNullOrEmpty(client.Phone)) client.Phone = freshClient.Phone;
                        if (string.IsNullOrEmpty(client.Name)) client.Name = freshClient.Name;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DEBUG] Error fetching fresh client details: {ex.Message}");
                }
            }

            OnClientSelected(client!);
        }

        protected virtual void OnClientSelected(ClientRecord client)
        {
            // Derived view models can override to implement automatic document inheritance
        }

        protected async System.Threading.Tasks.Task LoadClientCodesAsync(Func<string>? getClientIdText = null)
        {
            try
            {
                var clients = await DataService.Instance.GetClientsAsync();
                _allClients = clients
                    .Where(c => !string.IsNullOrEmpty(c.ClientCode))
                    .OrderBy(c => c.ClientCode)
                    .ToList();

                // Re-trigger filter in case user started typing before load completed
                var currentText = getClientIdText?.Invoke();
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
            HighlightedSuggestionIndex = -1;

            if (string.IsNullOrWhiteSpace(text))
            {
                SelectedClient = null;
                IsClientCodeDropdownOpen = false;
                return;
            }
            else
            {
                var filtered = _allClients
                    .Where(c => c.ClientCode!.Contains(text, StringComparison.OrdinalIgnoreCase) || 
                               (c.Name != null && c.Name.Contains(text, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
                foreach (var client in filtered)
                    ClientCodeSuggestions.Add(client);

                // Dynamically resolve SelectedClient on exact case-insensitive match
                SelectedClient = _allClients.FirstOrDefault(c => string.Equals(c.ClientCode, text, StringComparison.OrdinalIgnoreCase));
            }

            if (!_isSelectingClient)
            {
                bool isExactMatch = SelectedClient != null && string.Equals(SelectedClient.ClientCode, text, StringComparison.OrdinalIgnoreCase);
                IsClientCodeDropdownOpen = ClientCodeSuggestions.Count > 0 && !isExactMatch;
            }
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
            if (client == null) return;
            _isSelectingClient = true;
            SelectedClient = client;
            SelectedClientId = client.ClientCode ?? string.Empty;
            IsClientCodeDropdownOpen = false;
            _isSelectingClient = false;

            _ = LoadClientDocumentsAndNotifyAsync(client);
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
