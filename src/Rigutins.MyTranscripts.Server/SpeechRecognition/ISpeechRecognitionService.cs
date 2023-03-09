namespace Rigutins.MyTranscripts.Server.SpeechRecognition;

public interface ISpeechRecognitionService
{
	event Action? RecognitionStarted;
	event Action? RecognitionCompleted;

	bool IsExecuting { get; }

	Task<SpeechRecognitionResult> ExecuteRecognitionAsync(byte[] fileContent, string? language = null);
	Task<SpeechRecognitionResult> ExecuteRecognitionAsync(Stream stream, string? language = null);
}
