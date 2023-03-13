using Microsoft.AspNetCore.Components.Forms;

namespace Rigutins.MyTranscripts.Server.Data;

public class Transcript
{
	public string Id { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public string? OneDriveUrl { get; set; }
	public DateTimeOffset? CreatedDateTime { get; set; }
	public TranscriptStatus Status { get; set; }
	public string Language { get; set; } = "";
	public IBrowserFile? SelectedFile { get; set; }
	public int ProgressPercentage { get; set; }
	public List<string> RecognizedSentences { get; set; } = new();
}
