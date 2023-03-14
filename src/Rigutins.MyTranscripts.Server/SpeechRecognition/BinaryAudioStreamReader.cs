using Microsoft.CognitiveServices.Speech.Audio;

namespace Rigutins.MyTranscripts.Server.SpeechRecognition;

public class BinaryAudioStreamReader : PullAudioInputStreamCallback
{
	private readonly BinaryReader _reader;
	private readonly long _totalSize;

	private long _bytesRead;
	private double _readRatio;
	private int _percentage;
	private event Action<int>? _completionPercentageChanged;

	public BinaryAudioStreamReader(BinaryReader reader, long totalSize, Action<int>? completionPercentageChanged = null)
	{
		_reader = reader;
		_totalSize = totalSize;
		_bytesRead = 0;
		_readRatio = 0;
		_percentage = 0;
		_completionPercentageChanged = completionPercentageChanged;
	}

	public override int Read(byte[] dataBuffer, uint size)
	{
		_bytesRead += (int)size;
		_readRatio = Math.Min((double)_bytesRead / _totalSize, 1);
		int newPercentage = (int)Math.Round(100 * _readRatio);
		if (newPercentage != _percentage)
		{
			OnCompletionPercentageChanged(newPercentage);
		}

		return _reader.Read(dataBuffer, 0, (int)size);
	}

	private void OnCompletionPercentageChanged(int percentage)
	{
		_percentage = percentage;
		_completionPercentageChanged?.Invoke(percentage);
	}
}
