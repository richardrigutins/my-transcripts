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
	private List<Transcript> Transcripts => SpeechRecognitionState.Transcripts;
	private bool Loading { get; set; } = false;
	private bool IsFabDisabled => IsRecognizing || Loading;

	private bool ShowModal { get; set; } = false;
	private string ModalClass => ShowModal ? "modal fade show" : "modal fade";
	private string ModalDisplayType => ShowModal ? "block" : "none";
	private bool ModalIsHidden => !ShowModal;

	private string InputFileId { get; set; } = Guid.NewGuid().ToString();
	private IBrowserFile? SelectedFile { get; set; }
	private string SelectedLanguage { get; set; } = Language.DefaultLanguage;
	private bool IsInvalidFile { get; set; }
	private string FileValidationErrorDisplayStyle => IsInvalidFile ? "block" : "none";
	private bool IsStartRecognitionDisabled => SelectedFile == null || IsRecognizing;

	private bool IsRecognizing => SpeechRecognitionState.IsRecognizing;

	protected override Task OnInitializedAsync()
	{
		SpeechRecognitionService.RecognitionStarted += OnRecognitionStarted;
		SpeechRecognitionService.RecognitionCompleted += OnRecognitionCompleted;
		SpeechRecognitionState.OnChange += OnStateChanged;
		return LoadFilesAsync();
	}

	public void Dispose()
	{
		SpeechRecognitionService.RecognitionStarted -= OnRecognitionStarted;
		SpeechRecognitionService.RecognitionCompleted -= OnRecognitionCompleted;
		SpeechRecognitionState.OnChange -= OnStateChanged;
	}

	private async Task LoadFilesAsync()
	{
		Loading = true;
		try
		{
			if (SpeechRecognitionState.Transcripts.Count == 0)
			{
				ApplicationFolder = await OneDriveService.GetApplicationFolderAsync();
				var files = await OneDriveService.GetFolderItemsAsync(ApplicationFolder!.Id);
				SpeechRecognitionState.Transcripts = files.Select(MapDriveItemToTranscript).OrderByDescending(t => t.CreatedDateTime).ToList();
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
			Status = TranscriptStatus.Saved,
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
	}

	private async Task StartRecognition()
	{
		if (SelectedFile == null)
		{
			return;
		}

		Transcript newTranscript = new()
		{
			Id = Guid.NewGuid().ToString(),
			Name = SelectedFile!.Name,
			Status = TranscriptStatus.InProgress,
			CreatedDateTime = DateTime.Now,
			Language = SelectedLanguage,
			SelectedFile = SelectedFile,
		};

		SpeechRecognitionState.TranscriptInProgress = newTranscript;
		SpeechRecognitionState.Transcripts = SpeechRecognitionState.Transcripts.Prepend(newTranscript).ToList();

		ToggleModal();

		try
		{
			// Load file into memory stream
			var stream = new MemoryStream();
			IBrowserFile file = newTranscript.SelectedFile;
			await file.OpenReadStream(file.Size).CopyToAsync(stream);
			stream.Position = 0;

			await InvokeAsync(() =>
			{
				SpeechRecognitionService.RecognizeAsync(stream, newTranscript.Language);
			});
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "An error occurred");
			newTranscript.Status = TranscriptStatus.Failed;
			ToastState.ShowToast(ex.Message, ToastColor.Error);
		}
	}


	private void OnRecognitionStarted()
	{
		InvokeAsync(() =>
		{
			try
			{
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

	private void RemoveTranscript(Transcript transcript)
	{
		Transcripts.RemoveAll(t => t.Id == transcript.Id);
		InvokeAsync(StateHasChanged);
	}

	private void OnStateChanged()
	{
		InvokeAsync(StateHasChanged);
	}
}
