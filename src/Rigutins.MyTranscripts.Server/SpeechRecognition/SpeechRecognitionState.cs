﻿using Rigutins.MyTranscripts.Server.Data;

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

	public List<Transcript> Transcripts { get; set; } = new();
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
		IsRecognizing = true;
	}

	public void OnRecognitionCompleted(SpeechRecognitionResult recognitionResult)
	{
		IsRecognizing = false;
		var transcript = Transcripts.FirstOrDefault(t => t.Id == TranscriptInProgress?.Id);
		if (transcript == null)
		{
			return;
		}

		if (recognitionResult.Reason == SpeechRecognitionResultReason.Success)
		{
			transcript.Status = TranscriptStatus.Completed;
		}
		else
		{
			transcript.Status = TranscriptStatus.Failed;
		}

		TranscriptInProgress = null;
		NotifyStateChanged();
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
