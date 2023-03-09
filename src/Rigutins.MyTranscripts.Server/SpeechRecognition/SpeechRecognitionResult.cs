namespace Rigutins.MyTranscripts.Server.SpeechRecognition;

public class SpeechRecognitionResult
{
	public List<string> RecognizedText { get; set; } = new();
	public string ErrorMessage { get; set; } = string.Empty;
	public SpeechRecognitionResultReason Reason { get; set; } = SpeechRecognitionResultReason.Success;
}
