using Rigutins.MyTranscripts.Server.Options;
using Rigutins.MyTranscripts.Server.SpeechRecognition;

namespace System.Collections.Generic;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddSpeechRecognition(this IServiceCollection services, IConfigurationSection configurationSection)
	{
		return services.AddSpeechRecognition(options => configurationSection.Bind(options));
	}

	private static IServiceCollection AddSpeechRecognition(this IServiceCollection services, Action<SpeechRecognitionOptions> configureOptions)
	{
		services.AddOptions<SpeechRecognitionOptions>().Configure(configureOptions);
		services.AddScoped<ISpeechRecognitionService, AzureSpeechRecognitionService>();
		return services;
	}
}
