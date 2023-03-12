using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Graph;
using Rigutins.MyTranscripts.Server.Data;
using Rigutins.MyTranscripts.Server.Services;
using Rigutins.MyTranscripts.Server.SpeechRecognition;
using Rigutins.MyTranscripts.Server.Toasts;

namespace Rigutins.MyTranscripts.Server.Pages;

public partial class Index : IDisposable
{
	[Inject]
	IOneDriveService OneDriveService { get; init; } = default!;

	[Inject] 
	ISpeechRecognitionService SpeechRecognitionService { get; init; } = default!;

	[Inject] 
	ILogger<Index> Logger { get; init; } = default!;

	private DriveItem? ApplicationFolder { get; set; }
	private IEnumerable<Transcript> Transcripts { get; set; } = new List<Transcript>();
	private bool Loading { get; set; } = false;
	private bool IsFabDisabled => IsRecognizing || Loading;

	private bool ShowModal { get; set; } = false;
	private string ModalClass => ShowModal ? "modal fade show" : "modal fade";
	private string ModalDisplayType => ShowModal ? "block" : "none";
	private bool ModalIsHidden => !ShowModal;

	private bool LoadingModal { get; set; }
	private string InputFileId { get; set; } = Guid.NewGuid().ToString();
	private IBrowserFile? SelectedFile { get; set; }
	private string SelectedLanguage { get; set; } = Language.DefaultLanguage;
	private bool IsInvalidFile { get; set; }
	private string FileValidationErrorDisplayStyle => IsInvalidFile ? "block" : "none";
	private bool IsStartRecognitionDisabled => SelectedFile == null || IsRecognizing;

	private bool IsRecognizing { get; set; } = false;
	private Transcript? TranscriptInProgress { get; set; }
	private List<string> RecognizedSentences { get; init; } = new();
	private bool ConfirmExternalNavigation => IsRecognizing;

	protected override Task OnInitializedAsync()
	{
		SpeechRecognitionService.RecognitionStarted += OnRecognitionStarted;
		SpeechRecognitionService.RecognitionCompleted += OnRecognitionCompleted;
		SpeechRecognitionService.SentenceRecognized += OnSentenceRecognized;
		return LoadFilesAsync();
	}

	public void Dispose()
	{
		SpeechRecognitionService.RecognitionStarted -= OnRecognitionStarted;
		SpeechRecognitionService.RecognitionCompleted -= OnRecognitionCompleted;
		SpeechRecognitionService.SentenceRecognized -= OnSentenceRecognized;
	}

	private async Task LoadFilesAsync()
	{
		Loading = true;
		try
		{
			ApplicationFolder = await OneDriveService.GetApplicationFolderAsync();
			var files = await OneDriveService.GetFolderItemsAsync(ApplicationFolder!.Id);
			Transcripts = files.Select(MapDriveItemToTranscript).OrderByDescending(t => t.CreatedDateTime);
			if (TranscriptInProgress != null)
			{
				Transcripts = Transcripts.Prepend(TranscriptInProgress);
			}
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "An error occurred");
			ConsentHandler.HandleException(ex);
		}
		finally
		{
			Loading = false;
		}
	}

	private Transcript MapDriveItemToTranscript(DriveItem file)
	{
		return new Transcript()
		{
			Id = file.Id,
			Name = file.Name,
			IsInProgress = false,
			OneDriveUrl = file.WebUrl,
			CreatedDateTime = file.CreatedDateTime,
		};
	}

	private void ToggleModal()
	{
		ShowModal = !ShowModal;
		ResetModal();
	}

	private void OnInputFileChange(InputFileChangeEventArgs e)
	{
		var file = e.File;

		if (file.ContentType != "audio/wav")
		{
			IsInvalidFile = true;
			ClearInputFileSelection();
			return;
		}

		IsInvalidFile = false;
		SelectedFile = file;
	}

	private void ClearInputFileSelection()
	{
		InputFileId = Guid.NewGuid().ToString();
		SelectedFile = null;
	}

	private void ResetModal()
	{
		ClearInputFileSelection();
		IsInvalidFile = false;
		SelectedLanguage = Language.DefaultLanguage;
		LoadingModal = false;
	}

	private async Task StartRecognition()
	{
		ClearRecognition();
		Transcript newTranscript = new()
		{
			Name = SelectedFile!.Name,
			IsInProgress = true
		};
		Transcripts = Transcripts.Prepend(newTranscript);
		TranscriptInProgress = newTranscript;

		if (SelectedFile == null)
		{
			return;
		}

		LoadingModal = true;

		// Load file into memory stream
		var stream = new MemoryStream();
		await SelectedFile.OpenReadStream(SelectedFile.Size).CopyToAsync(stream);
		stream.Position = 0;

		await InvokeAsync(() =>
		{
			SpeechRecognitionService.RecognizeAsync(stream, SelectedLanguage);
		});
	}

	private void ClearRecognition()
	{
		IsRecognizing = false;
		TranscriptInProgress = null;
		RecognizedSentences.Clear();
	}

	private void OnSentenceRecognized(string sentence)
	{
		if (!string.IsNullOrWhiteSpace(sentence))
		{
			RecognizedSentences.Add(sentence);
		}
	}


	private void OnRecognitionStarted()
	{
		InvokeAsync(() =>
		{
			try
			{
				IsRecognizing = true;
				ToggleModal();
				StateHasChanged();
				ToastState.ShowToast("Recognition started");
			}
			catch (Exception ex)
			{
				Logger.LogError(ex, "An error occurred");
			}
		});
	}

	private void OnRecognitionCompleted(SpeechRecognitionResult recognitionResult)
	{
		InvokeAsync(() =>
		{
			try
			{
				IsRecognizing = false;
				LoadingModal = false;
				StateHasChanged();
				if (recognitionResult.Reason == SpeechRecognitionResultReason.Success)
				{
					ToastState.ShowToast("Recognition completed");
				}
				else
				{
					ToastState.ShowToast(recognitionResult.ErrorMessage, ToastColor.Error);
				}
			}
			catch (Exception ex)
			{
				Logger.LogError(ex, "An error occurred");
			}
		});
	}
}
