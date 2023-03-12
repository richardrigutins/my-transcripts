namespace Rigutins.MyTranscripts.Server.SpeechRecognition;

public class SpeechRecognitionResult
{
	public string ErrorMessage { get; set; } = string.Empty;
	public SpeechRecognitionResultReason Reason { get; set; } = SpeechRecognitionResultReason.Success;
}
