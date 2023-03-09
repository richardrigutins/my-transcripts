using Microsoft.CognitiveServices.Speech.Audio;

namespace Rigutins.MyTranscripts.Server.SpeechRecognition;

public class BinaryAudioStreamReader : PullAudioInputStreamCallback
{
	private readonly BinaryReader _reader;

	public BinaryAudioStreamReader(BinaryReader reader)
	{
		_reader = reader;
	}

	public override int Read(byte[] dataBuffer, uint size)
	{
		// Read bytes from the binary reader
		var readBytes = _reader.Read(dataBuffer, 0, (int)size);

		// Return the number of bytes read
		return readBytes;
	}
}
