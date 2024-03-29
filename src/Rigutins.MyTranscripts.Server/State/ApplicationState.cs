﻿using Rigutins.MyTranscripts.Server.Data;
using Rigutins.MyTranscripts.Server.SpeechRecognition;

namespace Rigutins.MyTranscripts.Server.State;

public class ApplicationState
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

	public string? ApplicationFolderId { get; set; }
	public List<Transcript> Transcripts { get; set; } = new();
	public Transcript? TranscriptInProgress { get; set; }
	public List<string> RecognizedSentences { get; init; } = new();

	public bool HasUnsavedChanges => Transcripts.Any(t => t.Status != TranscriptStatus.Saved);

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
		RecognizedSentences.Clear(); // Clear recognized sentences from previous recognition

		var transcript = Transcripts.FirstOrDefault(t => t.Id == TranscriptInProgress?.Id);
		if (transcript == null)
		{
			return;
		}

		transcript.Status = TranscriptStatus.InProgress;
		transcript.ProgressPercentage = 0;
		NotifyStateChanged();
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
			transcript.RecognizedSentences = RecognizedSentences;
		}
		else
		{
			transcript.Status = TranscriptStatus.Failed;
			transcript.StatusMessage = recognitionResult.ErrorMessage;
		}

		TranscriptInProgress = null;
		NotifyStateChanged();
	}

	public void OnCompletionPercentageChanged(int percentage)
	{
		var transcript = Transcripts.FirstOrDefault(t => t.Id == TranscriptInProgress?.Id);
		if (transcript == null)
		{
			return;
		}

		transcript.ProgressPercentage = percentage;
		NotifyStateChanged();
	}

	private void NotifyStateChanged()
	{
		OnChange?.Invoke();
	}
}
