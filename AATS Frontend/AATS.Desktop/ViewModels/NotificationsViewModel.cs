using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AATS.Desktop.Models;
using AATS.Desktop.Services;

namespace AATS.Desktop.ViewModels;

public partial class NotificationsViewModel : ViewModelBase
{
    public ObservableCollection<AppNotification> Notifications => NotificationService.Instance.Notifications;

    [ObservableProperty] private string _selectedFilter = "All";
    public string[] Filters { get; } = { "All", "Logins", "Logouts", "Unread" };

    [ObservableProperty] private ObservableCollection<AppNotification> _filteredNotifications;

    public NotificationsViewModel()
    {
        _filteredNotifications = new ObservableCollection<AppNotification>(Notifications);
        NotificationService.Instance.OnNotificationAdded += (s, e) => ApplyFilter();
        NotificationService.Instance.OnNotificationUpdated += (s, e) => ApplyFilter();
    }

    partial void OnSelectedFilterChanged(string value)
    {
        ApplyFilter();
    }

    [RelayCommand]
    private void ApplyFilter()
    {
        var filtered = Notifications.AsEnumerable();

        if (SelectedFilter == "Logins")
            filtered = filtered.Where(n => n.Action.ToLower().Contains("login"));
        else if (SelectedFilter == "Logouts")
            filtered = filtered.Where(n => n.Action.ToLower().Contains("logout"));
        else if (SelectedFilter == "Unread")
            filtered = filtered.Where(n => !n.IsRead);

        FilteredNotifications = new ObservableCollection<AppNotification>(filtered);
    }

    [RelayCommand]
    private void MarkAsRead(AppNotification notification)
    {
        NotificationService.Instance.MarkAsRead(notification);
    }

    [RelayCommand]
    private void MarkAllAsRead()
    {
        NotificationService.Instance.MarkAllAsRead();
    }
}
