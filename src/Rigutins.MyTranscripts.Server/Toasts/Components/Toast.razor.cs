using Microsoft.AspNetCore.Components;

namespace Rigutins.MyTranscripts.Server.Toasts.Components;

public partial class Toast
{
	[CascadingParameter]
	private ToastState ToastState { get; set; } = default!;

	[Parameter]
	public ToastInstance ToastInstance { get; set; } = default!;

	private ToastSettings ToastSettings => ToastInstance.Settings;
	private string Message => ToastInstance.Message;
	private string BtnCloseClass => ToastInstance.Color == ToastColor.Dark ? "btn-close btn-close-white" : "btn-close";

	private void Close()
	{
		InvokeAsync(() => ToastState.HideToast(ToastInstance.Id));
	}
}
