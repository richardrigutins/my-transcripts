using Microsoft.CognitiveServices.Speech.Audio;

namespace Rigutins.MyTranscripts.Server.SpeechRecognition;

public class BinaryAudioStreamReader : PullAudioInputStreamCallback
{
	private readonly BinaryReader _reader;
	private int _bytesRead;

	public BinaryAudioStreamReader(BinaryReader reader)
	{
		_reader = reader;
		_bytesRead = 0;
	}

	public override int Read(byte[] dataBuffer, uint size)
	{
		_bytesRead += (int)size;
		return _reader.Read(dataBuffer, 0, (int)size);
	}
}
