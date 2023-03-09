namespace Rigutins.MyTranscripts.Server.Notifications;

public class NotificationsState
{
	public event Action? OnChange;

	private readonly List<Notification> _notifications = new();

	public List<Notification> Notifications => _notifications;

	public void AddNotification(Notification notification)
	{
		_notifications.Add(notification);
	}

	public void RemoveNotification(Notification notification)
	{
		_notifications.Remove(notification);
	}

	public void RemoveAllNotifications()
	{
		_notifications.Clear();
	}

	private void NotifyStateChanged() => OnChange?.Invoke();
}
