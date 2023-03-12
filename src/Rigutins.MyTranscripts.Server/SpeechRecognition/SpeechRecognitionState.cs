using Rigutins.MyTranscripts.Server.Data;

namespace Rigutins.MyTranscripts.Server.SpeechRecognition;

public class SpeechRecognitionState
{
	public bool IsRecognizing { get; set; } = false;
	public Transcript? TranscriptInProgress { get; set; }
	public List<string> RecognizedSentences { get; set; } = new();
}
