using Rigutins.MyTranscripts.Server.Options;
using Rigutins.MyTranscripts.Server.SpeechRecognition;
using Rigutins.MyTranscripts.Server.Toasts;
using Rigutins.MyTranscripts.Server.Toasts.Services;

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

	public static IServiceCollection AddToasts(this IServiceCollection services)
	{
		services.AddScoped<ToastState>();
		services.AddScoped<IToastService, ToastService>();
		return services;
	}
}
