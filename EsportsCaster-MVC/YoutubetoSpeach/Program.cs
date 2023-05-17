using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using NAudio.Wave;
using System.Diagnostics;
using System.Text;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

class Program
{
    static async Task Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        // 下載音訊
        string videoUrl = "H5ZT9JLt5T4";
        int overlapInSeconds = 0;
        var youtube = new YoutubeClient();
        var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoUrl);
        var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
        if (streamInfo == null) throw new Exception("No suitable audio stream found for the video.");
        string audioFilePath = $"path_to_save_audio.{streamInfo.Container}";
        string mp4FilePath = $"{videoUrl}.mp4";

        var audiofileinfo = new FileInfo(videoUrl + "\\" + audioFilePath);
        var vediofileinfo = new FileInfo(videoUrl + "\\" + mp4FilePath);
        var srtfileinfo = new FileInfo(videoUrl + "\\" + videoUrl + ".srt");
        Directory.CreateDirectory(audiofileinfo.DirectoryName);
        string outputFolder = $"{audiofileinfo.DirectoryName}/output_folder";
        if (!File.Exists(audiofileinfo.FullName))
        {
            await youtube.Videos.Streams.DownloadAsync(streamInfo, audiofileinfo.FullName);
            // Get highest quality muxed stream
            // Get highest quality muxed stream
            var video = await youtube.Videos.GetAsync(videoUrl);

            // Get stream info set
            var streamInfoSet = await youtube.Videos.Streams.GetManifestAsync(videoUrl);

            // Select highest quality muxed stream
             streamInfo = streamInfoSet.GetMuxedStreams().GetWithHighestVideoQuality();

            // Download the stream to a file
            await youtube.Videos.Streams.DownloadAsync(streamInfo, vediofileinfo.FullName);

        }
        //var trackManifest = await youtube.Videos.ClosedCaptions.GetManifestAsync(videoUrl);
        //var trackInfo = trackManifest.GetByLanguage("cn");
        //await youtube.Videos.ClosedCaptions.DownloadAsync(trackInfo, srtfileinfo.FullName);
        string ffmpegPath = "./ffmpeg.exe";
        string convertedFilePath = new FileInfo($"{audiofileinfo.DirectoryName}/{Path.GetFileNameWithoutExtension(audioFilePath)}.wav").FullName;
        string arguments = $"-y -i \"{audiofileinfo.FullName}\" -acodec pcm_s16le -ar 16000 \"{convertedFilePath}\"";
        if (!File.Exists(convertedFilePath))
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardError = true
                }
            };
            process.Start();
            string stderr = await process.StandardError.ReadToEndAsync();
            process.WaitForExit();
            if (process.ExitCode != 0) throw new Exception("FFmpeg conversion failed: " + stderr);
        }
        if (Directory.Exists(outputFolder))
            Directory.Delete(outputFolder, true);
        // 分割音訊
        Directory.CreateDirectory(outputFolder);
        int segmentDurationInSeconds = 30; // 分割的音訊長度（秒）
        SplitAudioFile(convertedFilePath, outputFolder, segmentDurationInSeconds, overlapInSeconds);

        // 使用Azure Speech API進行語音識別和文本轉語音
        string speechSubscriptionKey = "Your API key";
        string speechRegion = "eastus";
        var config = SpeechConfig.FromSubscription(speechSubscriptionKey, speechRegion);

        var files = Directory.GetFiles(outputFolder);

        foreach (var file in files.Select((value, i) => new { i, value }))
        {
            var fi = new FileInfo(file.value);
            var text = string.Empty;
            var Content = GenerateVttSubtitleFile(file.value, file.i, text, srtfileinfo.FullName, overlapInSeconds);

            if (Content == null)
            {
                text = await TranscribeAudioToText(config, file.value);
                GenerateVttSubtitleFile(file.value, file.i, text, srtfileinfo.FullName, overlapInSeconds);
            }
            else
                text = Content;
            Console.WriteLine("Transcribed text: " + text);
            //await TextToSpeech(config, text);
            string ssmlText = @$"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='zh-CN'>
              <voice name='zh-CN-YunxiNeural'>
                <prosody rate='fast' pitch='+5%' volume='loud'>
                  {text}
                </prosody>
              </voice>
            </speak>";

            await TextToSpeech(config, ssmlText, text);

        }
    }


    public static void SplitAudioFile(string inputFilePath, string outputFolder, int segmentDurationInSeconds, int overlapInSeconds = 1)
    {
        using (WaveFileReader reader = new WaveFileReader(inputFilePath))
        {
            bool endOfFile = false;
            int i = 0;
            while (!endOfFile)
            {
                string beginTime = TimeSpan.FromSeconds(i).ToString(@"hhmmss");
                string endTime = TimeSpan.FromSeconds(i + segmentDurationInSeconds).ToString(@"hhmmss");
                string date = DateTime.Now.ToString("yyyyMMdd");
                string outputPath = Path.Combine(outputFolder, $"segment_{date}_{beginTime}_{endTime}.wav");

                using (WaveFileWriter writer = new WaveFileWriter(outputPath, reader.WaveFormat))
                {
                    int bytesPerMillisecond = reader.WaveFormat.AverageBytesPerSecond / 1000;
                    int segmentDurationInMilliseconds = segmentDurationInSeconds * 1000;
                    int overlapDurationInMilliseconds = overlapInSeconds * 1000;
                    int segmentSize = bytesPerMillisecond * segmentDurationInMilliseconds;
                    int overlapSize = bytesPerMillisecond * overlapDurationInMilliseconds;
                    byte[] buffer = new byte[segmentSize + overlapSize];

                    int bytesRead;
                    if ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        writer.Write(buffer, 0, bytesRead);
                    }

                    if (bytesRead < buffer.Length)
                    {
                        endOfFile = true;
                    }
                    else
                    {
                        reader.Position -= overlapSize;
                    }
                }

                i += segmentDurationInSeconds - overlapInSeconds;
            }
        }
    }





    static async Task<string> TranscribeAudioToText(SpeechConfig config, string audioFilePath)
    {
        config.SpeechRecognitionLanguage = "zh-CN";
        var FullName = new FileInfo(audioFilePath).FullName;
        using var audioInput = AudioConfig.FromWavFileInput(FullName);
        using var recognizer = new SpeechRecognizer(config, audioInput);
        var result = await recognizer.RecognizeOnceAsync();
        return result.Text;
    }

    static async Task TextToSpeech(SpeechConfig config, string ssml, string text)
    {

        using var synthesizer = new SpeechSynthesizer(config);
        using var result = await synthesizer.SpeakSsmlAsync(ssml);
        if (result.Reason == ResultReason.SynthesizingAudioCompleted)
        {
            Console.WriteLine($"Speech synthesized : {text}");
        }
        else if (result.Reason == ResultReason.Canceled)
        {
            var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
            Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");
            if (cancellation.Reason == CancellationReason.Error)
            {
                Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                Console.WriteLine($"CANCELED: ErrorDetails=[{cancellation.ErrorDetails}]");
                Console.WriteLine($"CANCELED: Did you update the subscription info?");
            }
        }
    }
    public static string GenerateVttSubtitleFile(string fileName, int contentIndex, string content, string vttOutputPath, int overlapInSeconds = 1)
    {
        if (!File.Exists(vttOutputPath))
        {
            // Write the VTT file header
            File.WriteAllText(vttOutputPath, "WEBVTT" + Environment.NewLine);
        }

        var vttContent = File.ReadAllLines(vttOutputPath);

        // Collect existing indexes from VTT content (excluding lines with content only)
        var existingIndexes = new Dictionary<int, string>();

        for (int i = 0; i < vttContent.Length - 2; i++)
        {
            var line = vttContent[i];

            if (!line.Contains(" --> ") && !string.IsNullOrWhiteSpace(line))
            {
                if (int.TryParse(line.Trim(), out int index))
                {
                    existingIndexes[index] = vttContent[i + 2];
                }
            }
        }

        // Check if the content index already exists in the VTT file
        var indexExists = existingIndexes.ContainsKey(contentIndex + 1);

        if (indexExists)
        {
            // Get the content of the line directly from the dictionary
            var existingIndexContent = existingIndexes[contentIndex + 1];

            return existingIndexContent;
        }
        else if (!indexExists && content.Equals(string.Empty))
        {
            return null;
        }
        else
        {
            // Parse the file name to extract the date, begin time, and end time
            var segments = new FileInfo(fileName).Name.Split('_', '.');
            var date = segments[1];
            var beginTime = TimeSpan.ParseExact(segments[2], "hhmmss", null);
            var endTime = TimeSpan.ParseExact(segments[3], "hhmmss", null);

            // Calculate the adjusted begin time and end time accounting for overlap
            var adjustedBeginTime = contentIndex == 0 ? beginTime : beginTime - TimeSpan.FromSeconds(overlapInSeconds);
            var adjustedEndTime = endTime;

            // Create a StreamWriter to write the VTT subtitle file
            using (var writer = new StreamWriter(vttOutputPath, append: true))
            {
                // Write the VTT subtitle entry
                writer.WriteLine(contentIndex + 1);
                writer.WriteLine($"{FormatTimeSpan(adjustedBeginTime)} --> {FormatTimeSpan(adjustedEndTime)}");
                writer.WriteLine(content);
                writer.WriteLine();
            }

            return null;
        }
    }






    private static string FormatTimeSpan(TimeSpan timeSpan)
    {
        return timeSpan.ToString(@"hh\:mm\:ss\.fff");
    }
}
