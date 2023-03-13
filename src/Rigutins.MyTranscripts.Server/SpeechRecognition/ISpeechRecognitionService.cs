namespace Rigutins.MyTranscripts.Server.SpeechRecognition;

public interface ISpeechRecognitionService
{
	event Action<SpeechRecognitionResult>? RecognitionCompleted;
	event Action<string>? SentenceRecognized;
	event Action? RecognitionStarted;
	event Action<int>? CompletionPercentageChanged;

	bool IsExecuting { get; }

	Task<SpeechRecognitionResult> RecognizeAsync(Stream stream, string language);
}
