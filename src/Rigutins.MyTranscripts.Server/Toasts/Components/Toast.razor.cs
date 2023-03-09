using Microsoft.AspNetCore.Components;

namespace Rigutins.MyTranscripts.Server.Toasts.Components;

public partial class Toast
{
	[Parameter]
	public ToastInstance ToastInstance { get; set; } = default!;

	private ToastSettings ToastSettings => ToastInstance.Settings;
	private string Message => ToastInstance.Message;

	private void Close()
	{
		InvokeAsync(() => ToastService.Hide(ToastInstance.Id));
	}
}
