using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Options;
using Rigutins.MyTranscripts.Server.Options;

namespace Rigutins.MyTranscripts.Server.SpeechRecognition;

public class AzureSpeechRecognitionService : ISpeechRecognitionService
{
	private readonly ILogger<AzureSpeechRecognitionService> _logger;
	private readonly SpeechRecognitionOptions _options;

	public event Action? RecognitionStarted;
	public event Action? RecognitionCompleted;

	public bool IsExecuting { get; private set; }

	public AzureSpeechRecognitionService(ILogger<AzureSpeechRecognitionService> logger, IOptions<SpeechRecognitionOptions> options)
	{
		_logger = logger;
		_options = options.Value;
	}

	public async Task<SpeechRecognitionResult> ExecuteRecognitionAsync(Stream stream, string? language = null)
	{
		if (IsExecuting)
		{
			return new SpeechRecognitionResult()
			{
				Reason = SpeechRecognitionResultReason.Error,
				ErrorMessage = "Speech recognition is already executing."
			};
		}

		var speechConfig = SpeechConfig.FromSubscription(_options.SubscriptionKey, _options.Region);
		if (language != null)
		{
			speechConfig.SpeechRecognitionLanguage = language;
		}

		// Create an audio format for the stream.
		var audioFormat = AudioStreamFormat.GetWaveFormatPCM(16000, 16, 1);

		// Create an audio input stream from the byte array and the format.
		var audioConfigStream = AudioInputStream.CreatePullStream(
			new BinaryAudioStreamReader(new BinaryReader(stream)),
			audioFormat
		);

		using var audioConfig = AudioConfig.FromStreamInput(audioConfigStream);
		using var recognizer = new SpeechRecognizer(speechConfig, audioConfig);

		return await ExecuteContinuousRecognitionAsync(recognizer);
	}

	public async Task<SpeechRecognitionResult> ExecuteRecognitionAsync(byte[] fileContent, string? language = null)
	{
		if (IsExecuting)
		{
			return new SpeechRecognitionResult()
			{
				Reason = SpeechRecognitionResultReason.Error,
				ErrorMessage = "Speech recognition is already executing."
			};
		}

		var speechConfig = SpeechConfig.FromSubscription(_options.SubscriptionKey, _options.Region);
		if (language != null)
		{
			speechConfig.SpeechRecognitionLanguage = language;
		}

		using var audioConfigStream = AudioInputStream.CreatePushStream();
		using var audioConfig = AudioConfig.FromStreamInput(audioConfigStream);
		using var recognizer = new SpeechRecognizer(speechConfig, audioConfig);

		audioConfigStream.Write(fileContent);

		return await ExecuteContinuousRecognitionAsync(recognizer);
	}

	private async Task<SpeechRecognitionResult> ExecuteContinuousRecognitionAsync(SpeechRecognizer recognizer)
	{
		SpeechRecognitionResult result = new();

		try
		{
			List<string> recognizedText = new();

			var stopRecognition = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

			recognizer.Recognized += (s, e) =>
			{
				if (e.Result.Reason == ResultReason.RecognizedSpeech)
				{
					recognizedText.Add(e.Result.Text);
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
			IsExecuting = true;
			RecognitionStarted?.Invoke();
			await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

			// Waits for completion.
			// Use Task.WaitAny to keep the task rooted.
			Task.WaitAny(new[] { stopRecognition.Task });

			await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);

			if (result.Reason == SpeechRecognitionResultReason.Success)
			{
				result.RecognizedText = recognizedText;
			}

			return result;
		}
		finally
		{
			IsExecuting = false;
			RecognitionCompleted?.Invoke();
		}
	}
}
