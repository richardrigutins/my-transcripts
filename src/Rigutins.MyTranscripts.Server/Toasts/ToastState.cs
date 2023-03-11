namespace Rigutins.MyTranscripts.Server.Toasts;

public class ToastState
{
	public List<ToastInstance> Toasts { get; init; } = new List<ToastInstance>();

	public event Action? OnChange;

	public void ShowToast(
		string message,
		ToastColor color = ToastColor.Dark,
		ToastPosition position = ToastPosition.BottomCenter,
		ToastSettings? settings = null)
	{
		// Hide all other toasts in the same position before showing a new one
		RemoveAllByPosition(position);

		settings ??= new ToastSettings();

		var toast = new ToastInstance(message, color, position, settings);
		Toasts.Add(toast);

		NotifyStateChanged();
	}

	public void HideToast(Guid id)
	{
		var toastInstance = Toasts.FirstOrDefault(t => t.Id == id);
		if (toastInstance != null)
		{
			RemoveToast(toastInstance);
			NotifyStateChanged();
		}
	}

	private void RemoveToast(ToastInstance toastInstance)
	{
		Toasts.Remove(toastInstance);
	}

	private void RemoveAllByPosition(ToastPosition position)
	{
		for (var i = 0; i < Toasts.Count; i++)
		{
			var toastInstance = Toasts[i];
			if (toastInstance.Position == position)
			{
				RemoveToast(toastInstance);
				i--;
			}
		}
	}

	private void NotifyStateChanged()
	{
		OnChange?.Invoke();
	}
}
