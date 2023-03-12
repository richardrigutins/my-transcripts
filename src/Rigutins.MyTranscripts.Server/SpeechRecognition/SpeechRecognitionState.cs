namespace Rigutins.MyTranscripts.Server.SpeechRecognition;

public class SpeechRecognitionState
{
	public bool IsRecognizing { get; set; } = false;
	public List<string> RecognizedSentences { get; set; } = new();
}
