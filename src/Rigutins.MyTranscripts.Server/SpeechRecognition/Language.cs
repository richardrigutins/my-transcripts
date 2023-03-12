namespace Rigutins.MyTranscripts.Server.SpeechRecognition;

public static class Language
{
	public const string DefaultLanguage = "en-US";

	public static readonly Dictionary<string, string> Languages = new() {
		{ "en-US", "English (United States)" },
		{ "en-GB", "English (United Kingdom)" },
		{ "en-AU", "English (Australia)" },
		{ "en-CA", "English (Canada)" },
		{ "en-IN", "English (India)" },
		{ "de-DE", "German (Germany)" },
		{ "es-ES", "Spanish (Spain)" },
		{ "es-MX", "Spanish (Mexico)" },
		{ "fr-FR", "French (France)" },
		{ "it-IT", "Italian (Italy)" },
		{ "ja-JP", "Japanese (Japan)" },
		{ "pt-BR", "Portuguese (Brazil)" },
		{ "zh-CN", "Chinese (China)" },
		{ "zh-HK", "Chinese (Hong Kong)" },
		{ "zh-TW", "Chinese (Taiwan)" }
	};
}
