using Rigutins.MyTranscripts.Server.Data;

namespace Rigutins.MyTranscripts.Server.SpeechRecognition;

public class SpeechRecognitionState
{
	private bool _isRecognizing;

	public bool IsRecognizing
	{
		get
		{
			return _isRecognizing;
		}
		set
		{
			if (_isRecognizing != value)
			{
				_isRecognizing = value;
				NotifyStateChanged();
			}
		}
	}
	public Transcript? TranscriptInProgress { get; set; }
	public List<string> RecognizedSentences { get; init; } = new();

	public event Action? OnChange;

	public void OnSentenceRecognized(string sentence)
	{
		if (!string.IsNullOrWhiteSpace(sentence))
		{
			RecognizedSentences.Add(sentence);
		}
	}

	public void OnRecognitionStarted()
	{
		Clear();
		IsRecognizing = true;
	}

	public void OnRecognitionCompleted(SpeechRecognitionResult recognitionResult)
	{
		IsRecognizing = false;
	}

	public void Clear()
	{
		TranscriptInProgress = null;
		RecognizedSentences.Clear();
	}

	private void NotifyStateChanged()
	{
		OnChange?.Invoke();
	}
}
