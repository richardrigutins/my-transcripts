namespace Rigutins.MyTranscripts.Server.SpeechRecognition;

public class MockSpeechRecognitionService : ISpeechRecognitionService
{
	private const int TotalTime = 10000;
	private const int NumberOfIntervals = 100;

	public bool IsExecuting { get; private set; }

	public event Action<SpeechRecognitionResult>? RecognitionCompleted;
	public event Action<string>? SentenceRecognized;
	public event Action? RecognitionStarted;
	public event Action<int>? CompletionPercentageChanged;

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

		var intervalLength = TotalTime / NumberOfIntervals;
		double percentageSteps = 100 / NumberOfIntervals;
		int percentage = 0;
		for (int i = 0; i < NumberOfIntervals; i++)
		{
			await Task.Delay(intervalLength);
			percentage += (int)percentageSteps;
			CompletionPercentageChanged?.Invoke(percentage);
		}

		IsExecuting = false;
		SpeechRecognitionResult result = new()
		{
			Reason = SpeechRecognitionResultReason.Success,
		};

		RecognitionCompleted?.Invoke(result);

		return result;
	}
}
