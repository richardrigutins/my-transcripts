using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Graph;
using Rigutins.MyTranscripts.Server.Data;
using Rigutins.MyTranscripts.Server.Services;
using Rigutins.MyTranscripts.Server.SpeechRecognition;
using Rigutins.MyTranscripts.Server.Toasts;
using System.Text;

namespace Rigutins.MyTranscripts.Server.Pages;

public partial class Index : IDisposable
{
	private const int MaxFileNameLength = 50; // Max file name length in OneDrive
											  // Invalid characters for file name
	private static readonly List<char> InvalidCharacters = new()
	{
		'<', '>', ':', '"', '/', '\\', '|', '?', '*'
	};

	[Inject]
	IOneDriveService OneDriveService { get; init; } = default!;

	[Inject]
	ISpeechRecognitionService SpeechRecognitionService { get; init; } = default!;

	[Inject]
	ILogger<Index> Logger { get; init; } = default!;

	private DriveItem? ApplicationFolder { get; set; }
	private List<Transcript> Transcripts => SpeechRecognitionState.Transcripts;
	private IOrderedEnumerable<Transcript> OrderedTranscripts => Transcripts.OrderBy(t => t.Status).ThenByDescending(t => t.CreatedDateTime);
	private bool IsLoading { get; set; } = false;
	private bool IsFabDisabled => IsRecognizing || IsLoading || IsReadingFile;

	private bool ShowOverlay => ShowTranscribeModal || ShowSaveModal;
	private string OverlayClass => ShowOverlay ? "modal fade show" : "modal fade";
	private string OverlayDisplayType => ShowOverlay ? "block" : "none";
	private bool OverlayIsHidden => !ShowOverlay;

	private bool ShowTranscribeModal { get; set; } = false;
	private string TranscribeModalClass => ShowTranscribeModal ? "modal fade show" : "modal fade";
	private string TranscribeModalDisplayType => ShowTranscribeModal ? "block" : "none";
	private bool TranscribeModalIsHidden => !ShowTranscribeModal;

	private bool ShowSaveModal { get; set; } = false;
	private string SaveModalClass => ShowSaveModal ? "modal fade show" : "modal fade";
	private string SaveModalDisplayType => ShowSaveModal ? "block" : "none";
	private bool SaveModalIsHidden => !ShowSaveModal;
	private string SaveFileName { get; set; } = string.Empty;
	private Transcript? SelectedTranscript { get; set; }
	private bool IsSaveTranscriptDisabled => string.IsNullOrWhiteSpace(SaveFileName) || SaveFileName.Any(c => InvalidCharacters.Contains(c)) || IsLoading || SaveFileName.Length > MaxFileNameLength;

	private string InputFileId { get; set; } = Guid.NewGuid().ToString();
	private IBrowserFile? SelectedFile { get; set; }
	private string SelectedLanguage { get; set; } = Language.DefaultLanguage;
	private bool IsInvalidFile { get; set; }
	private string FileValidationErrorDisplayStyle => IsInvalidFile ? "block" : "none";
	private bool IsStartRecognitionDisabled => SelectedFile == null || IsRecognizing;

	private bool IsRecognizing => SpeechRecognitionState.IsRecognizing;
	private bool IsReadingFile { get; set; }

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
		IsLoading = true;
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
			IsLoading = false;
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
			ProgressPercentage = 100,
		};
	}

	private void ToggleTranscribeModal()
	{
		ShowTranscribeModal = !ShowTranscribeModal;
		ResetModal();
	}

	private void StartSaveTranscript(Transcript transcript)
	{
		SelectedTranscript = transcript;
		ToggleSaveModal();
	}

	private void ToggleSaveModal()
	{
		SaveFileName = string.Empty;
		ShowSaveModal = !ShowSaveModal;
		if (!ShowSaveModal)
		{
			SelectedTranscript = null;
		}

		StateHasChanged();
	}

	private async Task SaveTranscript()
	{
		if (SelectedTranscript is null)
		{
			return;
		}

		try
		{
			if (ApplicationFolder is null)
			{
				ToastState.ShowToast("Can't find folder", ToastColor.Warning);
				return;
			}

			if (SelectedTranscript.RecognizedSentences.Count == 0)
			{
				ToastState.ShowToast("The transcript is empty!", ToastColor.Warning);
				return;
			}

			IsLoading = true;
			var fileName = FormatName(SaveFileName);
			var fileContent = SelectedTranscript.RecognizedSentences;
			var selectedTranscriptId = SelectedTranscript.Id;
			ToggleSaveModal();

			using var stream = new MemoryStream(Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, fileContent)));
			var saveResult = await OneDriveService.UploadFileAsync(fileName, stream, ApplicationFolder);
			Transcripts.RemoveAll(t => t.Id == selectedTranscriptId);
			var savedTranscript = MapDriveItemToTranscript(saveResult);
			Transcripts.Add(savedTranscript);

			ToastState.ShowToast("Saved transcript");
			SelectedTranscript = null;
			StateHasChanged();
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "An error occurred");
			ToastState.ShowToast(ex.Message, ToastColor.Error);
		}
		finally
		{
			IsLoading = false;
		}
	}

	private string FormatName(string name)
	{
		var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(name);
		if (fileNameWithoutExtension.Length > MaxFileNameLength)
		{
			fileNameWithoutExtension = fileNameWithoutExtension.Substring(0, MaxFileNameLength);
		}

		// remove whitespaces
		fileNameWithoutExtension = fileNameWithoutExtension.Trim();

		return fileNameWithoutExtension + ".txt";
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

		IsReadingFile = true;

		Transcript newTranscript = new()
		{
			Id = Guid.NewGuid().ToString(), // temporary id, will be replaced with the id of the saved file
			Name = SelectedFile!.Name,
			Status = TranscriptStatus.Uploading,
			CreatedDateTime = DateTime.Now,
			Language = SelectedLanguage,
			SelectedFile = SelectedFile,
		};

		SpeechRecognitionState.TranscriptInProgress = newTranscript;
		SpeechRecognitionState.Transcripts = SpeechRecognitionState.Transcripts.Prepend(newTranscript).ToList();

		ToggleTranscribeModal();

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
		finally
		{
			IsReadingFile = false;
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
		OnStateChanged();
	}

	private void OnStateChanged()
	{
		InvokeAsync(StateHasChanged);
	}
}
