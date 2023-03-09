namespace Rigutins.MyTranscripts.Server.Toasts.Services;

public class ToastService : IToastService
{
	private readonly ToastState _toastState;

	public ToastService(ToastState toastState)
	{
		_toastState = toastState;
	}

	public event Action? OnShow;
	public event Action? OnHide;

	public void Show(ToastLevel level, string message, ToastPosition position = ToastPosition.BottomCenter, ToastSettings? settings = null)
	{
		HideAllByPosition(position);

		settings ??= new ToastSettings();

		var toast = new ToastInstance(message, level, position, settings);
		var id = toast.Id;

		_toastState.Toasts.Add(toast);

		OnShow?.Invoke();
	}

	public void ShowError(string message, ToastPosition position = ToastPosition.BottomCenter, ToastSettings? settings = null)
		=> Show(ToastLevel.Error, message, position, settings);

	public void ShowInfo(string message, ToastPosition position = ToastPosition.BottomCenter, ToastSettings? settings = null)
		=> Show(ToastLevel.Info, message, position, settings);

	public void ShowSuccess(string message, ToastPosition position = ToastPosition.BottomCenter, ToastSettings? settings = null)
		=> Show(ToastLevel.Success, message, position, settings);

	public void ShowWarning(string message, ToastPosition position = ToastPosition.BottomCenter, ToastSettings? settings = null)
		=> Show(ToastLevel.Warning, message, position, settings);

	public void Hide(Guid id)
	{
		var toastInstance = _toastState.Toasts.FirstOrDefault(t => t.Id == id);
		if (toastInstance != null)
		{
			Hide(toastInstance);
			OnHide?.Invoke();
		}
	}

	private void Hide(ToastInstance toastInstance)
	{
		_toastState.Toasts.Remove(toastInstance);
	}

	private void HideAllByPosition(ToastPosition position)
	{
		for (var i = 0; i < _toastState.Toasts.Count; i++)
		{
			var toastInstance = _toastState.Toasts[i];
			if (toastInstance.Position == position)
			{
				Hide(toastInstance);
				i--;
			}
		}

		OnHide?.Invoke();
	}
}
