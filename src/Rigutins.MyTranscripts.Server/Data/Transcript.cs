namespace Rigutins.MyTranscripts.Server.Data;

public class Transcript
{
	public string Id { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public string? OneDriveUrl { get; set; }
	public DateTimeOffset? CreatedDateTime { get; set; }
	public TranscriptStatus Status { get; set; }
}
