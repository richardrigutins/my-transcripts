using Microsoft.AspNetCore.Components;

namespace Rigutins.MyTranscripts.Server.Toasts.Components;

public partial class ToastContainer : IDisposable
{
	[Inject]
	private ToastState ToastState { get; set; } = default!;

	private string GetPositionClass(ToastPosition position)
	{
		return $"position-{position.ToString().ToLower()}";
	}

	protected override void OnInitialized()
	{
		ToastService.OnShow += StateHasChanged;
		ToastService.OnHide += StateHasChanged;
	}

	public void Dispose()
	{
		ToastService.OnShow -= StateHasChanged;
		ToastService.OnHide -= StateHasChanged;
	}
}
