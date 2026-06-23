using System;
using System.IO;
using System.Threading.Tasks;
using DotNetEnv;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Azure;
using Azure.Identity;
using Azure.Core;
using Azure.Core.Pipeline;


class Program
{
    static void OutputSpeechSynthesisResult(SpeechSynthesisResult speechSynthesisResult, string text)
    {
        switch (speechSynthesisResult.Reason)
        {
            case ResultReason.SynthesizingAudioCompleted:
                Console.WriteLine($"Speech synthesized for text: [{text}]");
                break;
            case ResultReason.Canceled:
                var cancellation = SpeechSynthesisCancellationDetails.FromResult(speechSynthesisResult);
                Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                if (cancellation.Reason == CancellationReason.Error)
                {
                    Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                    Console.WriteLine($"CANCELED: ErrorDetails=[{cancellation.ErrorDetails}]");
                    Console.WriteLine($"CANCELED: Did you set the speech resource key and endpoint values?");
                }
                break;
            default:
                break;
        }
    }

    async static Task Main(string[] args)
    {
        Env.Load();
        string speechKey = Environment.GetEnvironmentVariable("SPEECH_KEY") ?? throw new InvalidOperationException("SPEECH_KEY not set in .env");
        string endpoint = Environment.GetEnvironmentVariable("SPEECH_ENDPOINT") ?? throw new InvalidOperationException("SPEECH_ENDPOINT not set in .env");

        Dictionary<string, string> data = new()
        {
            //{"Are you sure about the feedback.mp3", "คุณแน่ใจเกี่ยวกับข้อเสนอแนะนั้นหรือเปล่า?"},
            //{"Do you want to quit.mp3", "คุณต้องการลาออกหรือไม่?"},
            //{"Do you want to take a break.mp3", "Anda sudah berlatih secukupnya, sila berehat sebentar sebelum meneruskan."},
            //{"Fantastic! We can enjoy together!.mp3", "เยี่ยมไปเลย! เราจะได้สนุกด้วยกัน!"},
            //{"How was the game.mp3", "เกมเป็นอย่างไรบ้าง?"},
            //{"I_ll make you happy in this training session!.mp3", "ฉันจะทำให้คุณมีความสุขในระหว่างการฝึกอบรมครั้งนี้!"},
            //{"I_m sorry to hear that, I_ll cheer you up!.mp3", "ฉันเสียใจที่ได้ยินเช่นนั้น ฉันจะทำให้คุณรู้สึกดีขึ้น!"},
            //{"Let me know how you are feeling today..mp3", "บอกฉันหน่อยสิว่าวันนี้คุณรู้สึกอย่างไรบ้าง."},
            //{"Let_s train today!.mp3", "มาฝึกซ้อมกันวันนี้เลย!"},
            //{"No problem, We can have fun training!.mp3", "ไม่มีปัญหา เราสามารถฝึกอบรมอย่างสนุกสนานได้!"},
            //{"Oh! that_s nice to hear, Let_s train!.mp3", "โอ้! ดีจังเลย งั้นเรามาฝึกกันเถอะ!"},
            //{"Please calibrate your movement now.mp3", "โปรดปรับเทียบการเคลื่อนไหวของคุณ "},
            //{"Unlock the selected item.mp3", " ปลดล็อกรายการที่เลือกใช่หรือไม่?"},
            {"Unlock the game.mp3", "ปลดล็อกเกมใช่หรือไม่?"},
            //{"You don_t have enough coins.mp3", " คุณมีเหรียญไม่พอ"}
        };
        var speechConfig = SpeechConfig.FromEndpoint(new Uri(endpoint), speechKey);
        speechConfig.SpeechSynthesisVoiceName = "th-TH-NiwatNeural";
        speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio16Khz32KBitRateMonoMp3);
        // Malay: ms-MY-YasminNeural
        // Tamil: ta-IN-PallaviNeural
        // Thai: th-TH-NiwatNeural
        foreach (var item in data)  
        {
            using var synthesizer = new SpeechSynthesizer(speechConfig, null);
            var ssml =
                "<speak xmlns=\"http://www.w3.org/2001/10/synthesis\" xmlns:mstts=\"http://www.w3.org/2001/mstts\" xmlns:emo=\"http://www.w3.org/2009/10/emotionml\" version=\"1.0\" xml:lang=\"th-TH\"><voice name=\"th-TH-NiwatNeural\">"
                + item.Value
                + "</voice></speak>";
            var result = await synthesizer.SpeakSsmlAsync(ssml);
            // var result = await synthesizer.SpeakTextAsync(item.Value);

            if (result.Reason == ResultReason.SynthesizingAudioCompleted)
            {
                Directory.CreateDirectory("output");
                File.WriteAllBytes(Path.Combine("output", item.Key), result.AudioData);
                Console.WriteLine($"MP3 file '{item.Key}' created!");
            }
            else
            {
                Console.WriteLine($"Error synthesizing '{item.Value}': {result.Reason}");
            }
        }
    }
}


