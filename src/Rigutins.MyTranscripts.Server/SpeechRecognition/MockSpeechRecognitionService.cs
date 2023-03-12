namespace Rigutins.MyTranscripts.Server.SpeechRecognition;

public class MockSpeechRecognitionService : ISpeechRecognitionService
{
	public bool IsExecuting { get; private set; }

	public event Action<SpeechRecognitionResult>? RecognitionCompleted;
	public event Action<string>? SentenceRecognized;
	public event Action? RecognitionStarted;

	public async Task<SpeechRecognitionResult> RecognizeAsync(Stream stream, string language)
	{
		if (IsExecuting)
		{
			return new SpeechRecognitionResult
			{
				Reason = SpeechRecognitionResultReason.Error,
				ErrorMessage = "Speech recognition is already executing.",
			};
		}

		IsExecuting = true;
		RecognitionStarted?.Invoke();
		await Task.Delay(10000);
		IsExecuting = false;
		SpeechRecognitionResult result = new()
		{
			Reason = SpeechRecognitionResultReason.Success,
		};

		RecognitionCompleted?.Invoke(result);

		return result;
	}
}
