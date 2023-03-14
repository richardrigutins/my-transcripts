namespace Rigutins.MyTranscripts.Server.SpeechRecognition;

public class MockSpeechRecognitionService : ISpeechRecognitionService
{
	private const int TotalTime = 5000;
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
			throw new InvalidOperationException("Speech recognition is already executing.");
		}

		SpeechRecognitionResult result = new()
		{
			Reason = SpeechRecognitionResultReason.Success,
		};

		try
		{
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
				SentenceRecognized?.Invoke(Guid.NewGuid().ToString());
			}
		}
		catch (Exception ex)
		{
			result.Reason = SpeechRecognitionResultReason.Error;
			result.ErrorMessage = ex.Message;
		}
		finally
		{
			IsExecuting = false;
			RecognitionCompleted?.Invoke(result);
		}

		return result;
	}
}
