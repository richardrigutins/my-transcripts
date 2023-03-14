namespace Rigutins.MyTranscripts.Server.Toasts;

public class ToastInstance
{
	public Guid Id { get; set; }
	public DateTime Timestamp { get; set; }
	public string Message { get; set; }
	public ToastSettings Settings { get; set; }
	public ToastColor Color { get; set; }
	public ToastPosition Position { get; set; }
	public string CssClass =>
		string.IsNullOrWhiteSpace(Settings.OverrideBaseClass)
			? $"blazor-toast-{Color.ToString().ToLower()}"
			: Settings.OverrideBaseClass;

	public ToastInstance(string message, ToastColor color, ToastPosition position, ToastSettings settings)
	{
		Id = Guid.NewGuid();
		Timestamp = DateTime.Now;
		Message = message;
		Color = color;
		Position = position;
		Settings = settings;
	}
}
