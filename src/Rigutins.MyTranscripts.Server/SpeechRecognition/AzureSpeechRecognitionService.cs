using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Options;
using Rigutins.MyTranscripts.Server.Options;

namespace Rigutins.MyTranscripts.Server.SpeechRecognition;

public class AzureSpeechRecognitionService : ISpeechRecognitionService
{
	private readonly SpeechRecognitionOptions _options;

	public event Action<SpeechRecognitionResult>? RecognitionCompleted;
	public event Action? RecognitionStarted;
	public event Action<string>? SentenceRecognized;
	public event Action<int>? CompletionPercentageChanged;

	public bool IsExecuting { get; private set; }

	public AzureSpeechRecognitionService(IOptions<SpeechRecognitionOptions> options)
	{
		_options = options.Value;
	}

	public async Task<SpeechRecognitionResult> RecognizeAsync(Stream stream, string language)
	{
		SpeechRecognitionResult result = new()
		{
			Reason = SpeechRecognitionResultReason.Success,
		};

		try
		{
			if (IsExecuting)
			{
				result.Reason = SpeechRecognitionResultReason.Error;
				result.ErrorMessage = "Speech recognition is already executing.";
				return result;
			}

			var speechConfig = SpeechConfig.FromSubscription(_options.SubscriptionKey, _options.Region);
			speechConfig.SpeechRecognitionLanguage = language;

			// Create an audio format for the stream.
			var audioFormat = AudioStreamFormat.GetWaveFormatPCM(16000, 16, 1);

			// Create an audio input stream from the byte array and the format.
			var audioConfigStream = AudioInputStream.CreatePullStream(
				new BinaryAudioStreamReader(new BinaryReader(stream), stream.Length, OnCompletionPercentageChanged),
				audioFormat
			);

			using var audioConfig = AudioConfig.FromStreamInput(audioConfigStream);
			using var recognizer = new SpeechRecognizer(speechConfig, audioConfig);

			var stopRecognition = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

			recognizer.Recognized += (s, e) =>
			{
				if (e.Result.Reason == ResultReason.RecognizedSpeech)
				{
					OnSentenceRecognized(e.Result.Text);
				}
				else if (e.Result.Reason == ResultReason.NoMatch)
				{
					result.Reason = SpeechRecognitionResultReason.NoMatch;
				}
			};

			recognizer.Canceled += (s, e) =>
			{
				if (e.Reason != CancellationReason.EndOfStream)
				{
					result.Reason = SpeechRecognitionResultReason.Canceled;
					result.ErrorMessage = e.Reason.ToString();
				}

				if (e.Reason == CancellationReason.Error)
				{
					result.Reason = SpeechRecognitionResultReason.Error;
					result.ErrorMessage = $"ErrorCode={e.ErrorCode}; ErrorDetails={e.ErrorDetails}";
				}

				stopRecognition.TrySetResult(0);
			};

			recognizer.SessionStopped += (s, e) =>
			{
				stopRecognition.TrySetResult(0);
			};

			// Starts continuous recognition. Uses StopContinuousRecognitionAsync() to stop recognition.
			await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
			OnRecognitionStarted();

			// Waits for completion.
			// Use Task.WaitAny to keep the task rooted.
			Task.WaitAny(new[] { stopRecognition.Task });

			await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			result.Reason = SpeechRecognitionResultReason.Error;
			result.ErrorMessage = ex.ToString();
		}
		finally
		{
			OnRecognitionCompleted(result);
		}

		return result;
	}

	private void OnSentenceRecognized(string text)
	{
		SentenceRecognized?.Invoke(text);
	}

	private void OnRecognitionStarted()
	{
		IsExecuting = true;
		RecognitionStarted?.Invoke();
	}

	private void OnRecognitionCompleted(SpeechRecognitionResult result)
	{
		IsExecuting = false;
		RecognitionCompleted?.Invoke(result);
	}

	private void OnCompletionPercentageChanged(int percentage)
	{
		CompletionPercentageChanged?.Invoke(percentage);
	}
}
