using Microsoft.AspNetCore.Components;
using Rigutins.MyTranscripts.Server.State;
using Rigutins.MyTranscripts.Server.Toasts;

namespace Rigutins.MyTranscripts.Server.Shared;

public abstract class StatefulComponentBase : ComponentBase
{
	[CascadingParameter]
	public ToastState ToastState { get; set; } = default!;

	[CascadingParameter]
	public ApplicationState ApplicationState { get; set; } = default!;

	protected bool ConfirmExternalNavigation => ApplicationState.HasUnsavedChanges;
}
