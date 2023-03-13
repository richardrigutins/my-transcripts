using Microsoft.CognitiveServices.Speech.Audio;

namespace Rigutins.MyTranscripts.Server.SpeechRecognition;

public class BinaryAudioStreamReader : PullAudioInputStreamCallback
{
	private readonly BinaryReader _reader;
	private long _bytesRead;
	private long _totalSize;
	private double _completionPercentage;
	private int _roundedPercentage;
	private event Action<int>? _completionPercentageChanged;

	public BinaryAudioStreamReader(BinaryReader reader, long totalSize, Action<int>? completionPercentageChanged = null)
	{
		_reader = reader;
		_totalSize = totalSize;
		_bytesRead = 0;
		_completionPercentage = 0;
		_roundedPercentage = 0;
		_completionPercentageChanged = completionPercentageChanged;
	}

	public override int Read(byte[] dataBuffer, uint size)
	{
		_bytesRead += (int)size;
		_completionPercentage = Math.Min((double)_bytesRead/_totalSize, 100);
		int newRoundedPercentage = (int)Math.Round(100 * _completionPercentage);
		if (newRoundedPercentage != _roundedPercentage)
		{
			OnCompletionPercentageChanged(newRoundedPercentage);
		}

		return _reader.Read(dataBuffer, 0, (int)size);
	}

	private void OnCompletionPercentageChanged(int percentage) 
	{
		_roundedPercentage = percentage;
		_completionPercentageChanged?.Invoke(percentage);
	}
}
