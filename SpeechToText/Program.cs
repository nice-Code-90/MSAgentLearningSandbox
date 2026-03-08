using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using NAudio.Utils;
using NAudio.Wave;
using OpenAI;
using OpenAI.Audio;
using OpenAI.Chat;
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
AudioClient audioClient = groqClient.GetAudioClient("whisper-large-v3");

ChatClientAgent agent = groqClient
    .GetChatClient(modelId)
        .AsAIAgent(instructions: "You are a Friendly AI Bot, answering questions");

AgentSession agentSession = await agent.CreateSessionAsync();

while (true)
{
    Console.WriteLine("Press any key to start recording...");
    Console.ReadKey();

    //Record the Audio
    using MemoryStream audioStream = RecordAudio();

    //Turn Audio into Text
    ClientResult<AudioTranscription> result = await audioClient.TranscribeAudioAsync(audioStream, "audio.wav");

    string questionFromAudio = result.Value.Text;
    Console.WriteLine($"> {questionFromAudio}");

    AgentResponse response = await agent.RunAsync(questionFromAudio, agentSession);
    Console.WriteLine(response);

    Utils.Separator();
}

MemoryStream RecordAudio()
{
    MemoryStream stream = new();
    using WaveInEvent waveIn = new();
    waveIn.WaveFormat = new WaveFormat(16000, 16, 1);
    WaveFileWriter writer = new(new IgnoreDisposeStream(stream), waveIn.WaveFormat);

    waveIn.DataAvailable += (_, args) => { writer.Write(args.Buffer, 0, args.BytesRecorded); };
    waveIn.StartRecording();

    Console.WriteLine("Recording... Press any key to stop");
    Console.ReadKey();

    waveIn.StopRecording();
    stream.Position = 0;
    return stream;
}