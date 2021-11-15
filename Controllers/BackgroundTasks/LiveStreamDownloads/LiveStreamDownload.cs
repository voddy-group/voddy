using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using RestSharp;
using voddy.Exceptions.Streams;
using Xabe.FFmpeg;

namespace voddy.Controllers.BackgroundTasks.LiveStreamDownloads {
    public class LiveStreamDownload {
        DateTime _oldDateTime = default;
        public List<string> _partsCollection;
        private int _counter;
        private DirectoryInfo _partsDirectory;
        public DirectoryInfo _rootDirectory;
        private string m3u8;

        public LiveStreamDownload(DirectoryInfo rootDirectory) {
            _partsCollection = new List<string>();
            _rootDirectory = rootDirectory;
            _counter = 0;
        }

        public Task GetVodParts(CancellationToken cancellationToken) {
            var client = new RestClient(m3u8);
            client.Timeout = -1;
            int retries = 0;
            while (retries < 3) {
                if (cancellationToken.IsCancellationRequested) {
                    // cancellation requested, remove the live stream files
                    
                    return Task.CompletedTask;
                }
                IRestResponse response = client.Execute(new RestRequest(Method.GET));
                bool grabPart = true;
                string[] responseLineSplit = response.Content.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                
                if (!responseLineSplit.Any(line => line.EndsWith(".ts"))) {
                    if (retries == 2) {
                        // retried too many times. throw an error so it can be handled elsewhere.
                        throw new TsFileNotFound();
                    }
                    // if no .ts files found in the m3u8; stream has probably ended. Just in case, we check a few more times after a delay.
                    retries++;
                    Console.WriteLine("No .ts files found, retrying in 1 second...");
                    Thread.Sleep(1000);
                    continue;
                }

                foreach (var line in responseLineSplit) {
                    if (line.StartsWith("#EXT-X-PROGRAM-DATE-TIME:")) {
                        int metaTextPosition = line.IndexOf("#EXT-X-PROGRAM-DATE-TIME:") + 25;
                        DateTime parsedDateTime = DateTime.Parse(line.Substring(metaTextPosition, line.Length - metaTextPosition));
                        if (_oldDateTime != default) {
                            if (parsedDateTime > _oldDateTime) {
                                // set current time to old time so it is not picked up on next m3u8 iteration
                                _oldDateTime = parsedDateTime;
                                _counter++;
                                grabPart = true;
                            } else {
                                grabPart = false;
                            }
                        } else {
                            // no old time so must be first run
                            _oldDateTime = parsedDateTime;
                        }
                    }

                    if (!line.StartsWith("#") && line.EndsWith(".ts") && grabPart) {
                        DownloadPart(line);
                    }
                }
            }
            
            // no more .ts files found, stream has ended.
            return Task.CompletedTask;
        }

        private void DownloadPart(string url) {
            _partsDirectory = new DirectoryInfo(_rootDirectory.FullName + "/parts");
            if (!_partsDirectory.Exists) {
                _partsDirectory.Create();
            }

            using (var client = new WebClient()) {
                client.DownloadFile(url, _partsDirectory.FullName + $"/{_counter}.ts");
            }

            _partsCollection.Add(_partsDirectory.FullName + $"/{_counter}.ts");
        }

        public Task GetVodM3U8(string url) {
            int retries = 0;
            while (retries < 4) {
                try {
                    var process =
                        new Process();
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.FileName = "yt-dlp";
                    process.StartInfo.Arguments = $"{url} -g";

                    List<string> errorList = new List<string>();
                    process.ErrorDataReceived += (_, e) => {
                        errorList.Add(e.Data);
                        //Console.WriteLine("error>>" + e.Data);
                    };
                    process.OutputDataReceived += (_, e) => {
                        if (e.Data != null && e.Data.StartsWith("https://") && e.Data.EndsWith(".m3u8")) {
                            m3u8 = e.Data;
                        }

                        //Console.WriteLine("output>>" + e.Data);
                    };

                    process.Start();

                    process.BeginErrorReadLine();
                    process.BeginOutputReadLine();

                    //string stdout = process.StandardOutput.ReadToEnd();

                    process.WaitForExit();
                    foreach (var error in errorList) {
                        if (error != null && error.StartsWith("ERROR:")) {
                            throw new Exception(error);
                        }
                    }

                    break;
                } catch (Exception e) {
                    if (retries < 3 && !e.Message.Contains("is offline")) {
                        // stream is not offline, retry
                        Console.WriteLine("Retrying in 5 seconds...");
                        Thread.Sleep(5000);
                        retries++;
                    } else {
                        // insta kill if stream is offline
                        Console.WriteLine("Unable to retrieve m3u8 due to an error: " + e);
                        throw;
                    }
                }
            }

            return Task.CompletedTask;
        }


        public void CombineTsFiles(string title, long streamId) {
            CreateCombinedTsFile();

            IConversion conversion = FFmpeg.Conversions.New()
                .AddParameter($"-f concat -safe 0 -i {_partsDirectory.FullName}/combinedTs.txt -c copy {_rootDirectory.FullName}/stream.mp4")
                .SetOverwriteOutput(true);

            conversion.Start().Wait();
        }

        private void CreateCombinedTsFile() {
            using (StreamWriter file = new StreamWriter($"{_partsDirectory.FullName}/combinedTs.txt")) {
                foreach (var part in _partsCollection) {
                    file.WriteLine($"file '{part}'");
                }
            };
        }

        public void CleanUpFiles() {
            Directory.Delete(_partsDirectory.FullName, true);
        }
    }
}