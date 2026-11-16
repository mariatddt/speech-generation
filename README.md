# Multilingual Speech Generation

A multilingual text-to-speech system developed in C#/.NET, supported by Python tooling for translation synchronisation. The project generates speech audio from spreadsheet-based translation and voice configurations, supporting multiple languages and custom speech settings.

The system was developed for use in an interactive rehabilitation application, where consistent, understandable audio across languages is important.

## Features

- **Multilingual text-to-speech:** Generate speech audio across multiple languages and locales.
- **Configurable voices:** Select language-specific voices through spreadsheet-based configuration.
- **Spreadsheet integration:** Read translated text and voice settings directly from Excel files.
- **Selective generation:** Generate audio for a specific language, phrase or combination of both.
- **SSML customisation:** Control speech characteristics, including speaking rate, using Speech Synthesis Markup Language.
- **Organised audio output:** Automatically save generated MP3 files in language-specific directories.
- **Translation synchronisation:** Use Python tooling to support the management and synchronisation of translation data.

## Technologies

| Technology | Purpose |
|---|---|
| C# | Main application logic |
| .NET 9 | Application framework |
| Microsoft Azure Speech SDK | Text-to-speech generation |
| SSML | Speech output customisation |
| ClosedXML | Excel spreadsheet integration |
| DotNetEnv | Environment variable configuration |
| Python | Translation synchronisation tooling |
| OpenPyXL | Excel processing in Python |

## How It Works

The C# application follows a spreadsheet-driven workflow:

1. Loads Azure Speech credentials from environment variables.
2. Reads the Excel template containing translated text and language-specific voice configurations.
3. Identifies the available language and voice columns.
4. Selects the requested language and text, if generation filters are provided.
5. Constructs SSML using the selected voice and speech settings.
6. Sends the request to Azure Speech.
7. Saves the generated MP3 files in language-specific output directories.

## Usage

### Requirements

- .NET 9 SDK
- Azure Speech service credentials
- Python 3.10+ for the translation synchronisation tooling
- Required spreadsheet configuration files

### Configuration

Create a `.env` file in the project root containing:

```env
SPEECH_KEY=your_azure_speech_key
SPEECH_ENDPOINT=your_azure_speech_endpoint
```

Do not commit this file or any service-account credentials to GitHub.

### Running the Application

Restore dependencies:

```bash
dotnet restore
```

Generate speech for all configured languages:

```bash
dotnet run
```

Generate speech for one language:

```bash
dotnet run -- --lang it-IT
```

Generate one phrase across all languages:

```bash
dotnet run -- --file "Unlock the game.mp3"
```

Generate one phrase in a specific language:

```bash
dotnet run -- --lang zh-CN --file "Unlock the game.mp3"
```

Generated audio is saved under `output/`, organised by locale.

### Python Translation Synchronisation

Install the Python dependencies:

```bash
pip install -r tools/translation_sync/requirements.txt
```

The translation synchronisation tooling is located in:

```text
tools/translation_sync/
```

Additional access or configuration may be required for the translation toolkit dependency.

## Project Structure

```text
speech-generation/
│
├── Program.cs
├── cynteract-tts.csproj
├── cynteract-tts.sln
│
├── tools/
│   └── translation_sync/
│       ├── sync_tts_sheet.py
│       └── requirements.txt
│
├── Malay/
├── Tamil/
├── Thai/
│
└── output/
    ├── en-US/
    ├── es-ES/
    ├── fr-FR/
    └── ...
```

The output and language directories contain generated audio and should only be included in a public repository if their publication is permitted.

## My Contributions

I worked on both the C# speech-generation application and the Python translation-synchronisation tooling, as well as testing and improving the multilingual speech-generation workflow.

My contributions included:

- Developing and improving functionality in the C#/.NET text-to-speech application.
- Working on Python tooling for synchronising translation data.
- Auditing generated speech across multiple languages by comparing audio output with the corresponding translated text.
- Identifying missing content, translation inconsistencies and pronunciation issues.
- Investigating SSML settings and speech-generation parameters to improve audio output.
- Working with spreadsheet-based translation and voice configurations to support multilingual content management.

## Notes

This repository contains tooling developed in a professional project context. Some configuration files, translation resources, generated audio and dependencies may be subject to access restrictions. Credentials and proprietary materials should not be published.

## Screenshots

<img width="14" height="8" alt="image" src="https://github.com/user-attachments/assets/dedf001a-99e7-4d49-b830-fff326713cd5" />

<img width="1917" height="1118" alt="image" src="https://github.com/user-attachments/assets/f47ada4c-5f37-49d2-95b1-9762d2f960d4" />

<img width="1902" height="1082" alt="image" src="https://github.com/user-attachments/assets/a42e0167-f8a4-4cbf-89dc-84278ad752ee" />
