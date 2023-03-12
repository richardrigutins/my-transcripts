using Microsoft.AspNetCore.Components;
using Rigutins.MyTranscripts.Server.SpeechRecognition;
using Rigutins.MyTranscripts.Server.Toasts;

namespace Rigutins.MyTranscripts.Server.Shared;

public abstract class StatefulComponentBase : ComponentBase
{
	[CascadingParameter]
	public ToastState ToastState { get; set; } = default!;

	[CascadingParameter]
	public SpeechRecognitionState SpeechRecognitionState { get; set; } = default!;
}
