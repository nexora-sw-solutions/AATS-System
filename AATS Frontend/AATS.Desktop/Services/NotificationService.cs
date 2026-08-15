using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AATS.Desktop.Models;

namespace AATS.Desktop.Services;

public class NotificationService
{
    private static NotificationService? _instance;
    public static NotificationService Instance => _instance ??= new NotificationService();

    private readonly ObservableCollection<AppNotification> _notifications = new();
    public ObservableCollection<AppNotification> Notifications => _notifications;

    public int UnreadCount => _notifications.Count(n => !n.IsRead);

    private NotificationService()
    {
        // Add some mock notifications for testing
        AddNotification("System", "Service Started");
    }

    public void AddNotification(string userName, string action)
    {
        var notification = new AppNotification
        {
            UserName = userName,
            Action = action,
            Timestamp = DateTime.Now
        };
        
        // Insert at beginning
        _notifications.Insert(0, notification);
        
        // Keep only last 100
        if (_notifications.Count > 100)
            _notifications.RemoveAt(_notifications.Count - 1);
            
        OnNotificationAdded?.Invoke(this, notification);
    }

    public void MarkAsRead(AppNotification notification)
    {
        notification.IsRead = true;
        OnNotificationUpdated?.Invoke(this, notification);
    }

    public void MarkAllAsRead()
    {
        foreach (var n in _notifications)
            n.IsRead = true;
        OnNotificationUpdated?.Invoke(this, null);
    }

    public event EventHandler<AppNotification>? OnNotificationAdded;
    public event EventHandler<AppNotification?>? OnNotificationUpdated;
}
