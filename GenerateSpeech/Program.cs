using Azure.AI.OpenAI;
using NAudio.Wave;
using OpenAI;
using OpenAI.Audio;
using Shared;
using System.ClientModel;

Console.Clear();
Secrets secrets = SecretManager.GetSecrets();
string apiKey = secrets.LLMApiKey;
string modelId = secrets.ModelId;


OpenAIClient groqClient = new OpenAIClient(
    new ApiKeyCredential(apiKey),
    new OpenAIClientOptions { Endpoint = new Uri("https://api.groq.com/openai/v1") }
);

/* Pricing (as of 1st of December 2025)
 * - gpt-4o-mini-tts    ~0.015 USD / minute
 * - tts:                15 USD / 1 Million Chars
 * - tts HD:             30 USD / 1 Million Chars
 */

AudioClient audioClient = groqClient.GetAudioClient("canopylabs/orpheus-v1-english");

GeneratedSpeechVoice voice = new GeneratedSpeechVoice("hannah"); //autumn, diana, hannah, austin, daniel, troy.

string text = "Hi! Welcome to this video about OpenAI's AudioClient. I'm an AI speaking the words Milan entered in his program";
ClientResult<BinaryData> result = audioClient.GenerateSpeech(text, voice, new SpeechGenerationOptions
{
    ResponseFormat = new GeneratedSpeechFormat("wav"),
    SpeedRatio = 1
});

byte[] bytes = result.Value.ToArray();

Console.WriteLine($"Generált adat mérete: {bytes.Length} bájt.");

if (bytes.Length < 100)
{
    
    string errorCheck = System.Text.Encoding.UTF8.GetString(bytes);
    Console.WriteLine("Lehetséges hibaüzenet az API-tól: " + errorCheck);
    return;
}


string filePath = Path.Combine(Path.GetTempPath(), "test.wav");
File.WriteAllBytes(filePath, bytes);


using (var ms = new MemoryStream(bytes))
{
    
    var waveFormat = new WaveFormat(24000, 16, 1);

    using (var rawStream = new RawSourceWaveStream(ms, waveFormat))
    {
        using (IWavePlayer player = new WaveOutEvent())
        {
            player.Init(rawStream);
            player.Play();
            Console.WriteLine("Playing... Press enter to exit.");
            Console.ReadLine();
        }
    }
}