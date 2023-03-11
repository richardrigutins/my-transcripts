using Microsoft.AspNetCore.Components;

namespace Rigutins.MyTranscripts.Server.Toasts.Components;

public partial class ToastContainer : IDisposable
{
	[CascadingParameter]
	private ToastState ToastState { get; set; } = default!;

	private string GetPositionClass(ToastPosition position)
	{
		return $"position-{position.ToString().ToLower()}";
	}

	protected override void OnInitialized()
	{
		ToastState.OnChange += StateHasChanged;
	}

	public void Dispose()
	{
		ToastState.OnChange -= StateHasChanged;
	}
}
