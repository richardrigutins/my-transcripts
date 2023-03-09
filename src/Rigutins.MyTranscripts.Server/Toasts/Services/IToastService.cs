namespace Rigutins.MyTranscripts.Server.Toasts.Services;

public interface IToastService
{
	event Action? OnHide;
	event Action? OnShow;

	void Hide(Guid id);
	void Show(ToastLevel level, string message, ToastPosition position = ToastPosition.BottomCenter, ToastSettings? settings = null);
	void ShowError(string message, ToastPosition position = ToastPosition.BottomCenter, ToastSettings? settings = null);
	void ShowInfo(string message, ToastPosition position = ToastPosition.BottomCenter, ToastSettings? settings = null);
	void ShowSuccess(string message, ToastPosition position = ToastPosition.BottomCenter, ToastSettings? settings = null);
	void ShowWarning(string message, ToastPosition position = ToastPosition.BottomCenter, ToastSettings? settings = null);
}
