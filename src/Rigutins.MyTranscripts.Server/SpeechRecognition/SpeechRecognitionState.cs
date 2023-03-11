namespace Rigutins.MyTranscripts.Server.SpeechRecognition;

public class SpeechRecognitionState
{
	public bool IsRecognizing { get; set; } = false;
	public List<string> RegognizedSentences { get; set; } = new();
}
