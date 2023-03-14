namespace Rigutins.MyTranscripts.Server.Data;

public class SaveFileFormData
{
	public string Name { get; set; } = string.Empty;
	public bool SetReminder { get; set; }
	public DateTime? ReminderDate { get; set; }
}
