using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotNetEnv;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using ClosedXML.Excel;
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

        // Parse command line arguments for specific targeting
        string? targetLang = null;
        string? targetFile = null;

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--lang" && i + 1 < args.Length)
            {
                targetLang = args[i + 1].Trim();
            }
            if (args[i] == "--file" && i + 1 < args.Length)
            {
                targetFile = args[i + 1].Trim();
            }
        }

        string spreadsheetPath = "TextToSpeechSpreadsheet.xltx"; 

        if (!File.Exists(spreadsheetPath))
        {
            Console.WriteLine($"Error: Spreadsheet file not found at {spreadsheetPath}");
            return;
        }

        Console.WriteLine("Reading multi-language layout from spreadsheet...");

        using var workbook = new XLWorkbook(spreadsheetPath);
        var worksheet = workbook.Worksheet(1);
        
        var rows = worksheet.RangeUsed().RowsUsed();
        var headerRow = worksheet.FirstRowUsed();
        int lastColumnNum = worksheet.LastColumnUsed().ColumnNumber();

        // Dynamically identify language headers (Columns B, D, F, H, etc.)
        var languageColumns = new System.Collections.Generic.List<(int voiceCol, int textCol, string locale)>();
        
        for (int col = 2; col <= lastColumnNum; col += 2)
        {
            string headerValue = headerRow.Cell(col).GetString().Trim();
            if (headerValue.EndsWith("_Voice"))
            {
                string locale = headerValue.Replace("_Voice", ""); 
                languageColumns.Add((col, col + 1, locale));
            }
        }

        bool isHeader = true;
        int generatedCount = 0;

        foreach (var row in rows)
        {
            if (isHeader)
            {
                isHeader = false;
                continue;
            }

            // Grab the exact filename written in Column A
            string baseFileName = row.Cell(1).GetString().Trim();
            if (string.IsNullOrEmpty(baseFileName)) continue;

            // Ensure it has a clean .mp3 extension if it doesn't already
            string finalFileName = baseFileName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) 
                ? baseFileName 
                : baseFileName + ".mp3";

            // FILTER: If a target file is specified, skip everything else
            if (!string.IsNullOrEmpty(targetFile) && !finalFileName.Equals(targetFile, StringComparison.OrdinalIgnoreCase) && !baseFileName.Equals(targetFile, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Iterate through every language pair found in this row
            foreach (var lang in languageColumns)
            {
                // FILTER: If a target language is specified, skip other languages
                if (!string.IsNullOrEmpty(targetLang) && !lang.locale.Equals(targetLang, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string voiceName = row.Cell(lang.voiceCol).GetString().Trim();
                string textValue = row.Cell(lang.textCol).GetString().Trim();

                if (string.IsNullOrEmpty(voiceName) || string.IsNullOrEmpty(textValue))
                    continue;

                // Create language-specific directories: output/ar-SA, output/zh-CN, etc.
                string outputDirectory = Path.Combine("output", lang.locale);
                Directory.CreateDirectory(outputDirectory);
                
                string fullOutputPath = Path.Combine(outputDirectory, finalFileName);

                Console.WriteLine($"Processing [{lang.locale}]: {finalFileName}...");

                var speechConfig = SpeechConfig.FromEndpoint(new Uri(endpoint), speechKey);
                speechConfig.SpeechSynthesisVoiceName = voiceName;
                speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio16Khz32KBitRateMonoMp3);

                using var synthesizer = new SpeechSynthesizer(speechConfig, null);

                // --- TUNED INTONATION SSML CHUNK APPLIED HERE ---
                var ssml = $@"<speak xmlns=""http://www.w3.org/2001/10/synthesis"" 
                                     xmlns:mstts=""http://www.w3.org/2001/mstts"" 
                                     xmlns:emo=""http://www.w3.org/2009/10/emotionml"" 
                                     version=""1.0"" 
                                     xml:lang=""{lang.locale}"">
                                <voice name=""{voiceName}"">
                                    <prosody rate=""-5%"">
                                        <lang xml:lang=""{lang.locale}"">{textValue}</lang>
                                    </prosody>
                                </voice>
                             </speak>";

                var result = await synthesizer.SpeakSsmlAsync(ssml);

                if (result.Reason == ResultReason.SynthesizingAudioCompleted)
                {
                    File.WriteAllBytes(fullOutputPath, result.AudioData);
                    Console.WriteLine($" -> Saved to {fullOutputPath}");
                    generatedCount++;
                }
                else
                {
                    Console.WriteLine($" -> Error synthesizing {finalFileName} in {lang.locale}");
                    OutputSpeechSynthesisResult(result, textValue);
                }
            }
        }

        Console.WriteLine($"\n Task completed! Total audio files generated in this run: {generatedCount}");
    }
}

// Speech Generation Guide: 
// dotnet run ➔ Generates everything.
// dotnet run -- --lang it-IT ➔ Generates only Italian files.
// dotnet run -- --file "Unlock the game.mp3" ➔ Generates that exact phrase across all languages.
// dotnet run -- --lang zh-CN --file "Unlock the game.mp3" ➔ Generates only that exact phrase in Chinese.