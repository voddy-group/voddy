using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using RestSharp;
using voddy.Exceptions.Streams;
using Xabe.FFmpeg;

namespace voddy.Controllers.BackgroundTasks.StreamDownloads {
    public class StreamDownload {
        private Logger _logger { get; set; } = new NLog.LogFactory().GetCurrentClassLogger();
        
        DateTime _oldDateTime = default;
        public List<string> _partsCollection;
        private int _counter;
        private DirectoryInfo _partsDirectory;
        public DirectoryInfo _rootDirectory;
        private string m3u8;
        private bool _isLive;

        public StreamDownload(DirectoryInfo rootDirectory, bool isLive) {
            _partsCollection = new List<string>();
            _rootDirectory = rootDirectory;
            _counter = 0;
            _isLive = isLive;
        }

        public void GetVodParts(CancellationToken cancellationToken) {
            var client = new RestClient(m3u8);
            client.Timeout = -1;
            int retries = 0;
            while (retries < 3) {
                _logger.Info($"Attempt number: {retries + 1}");
                if (cancellationToken.IsCancellationRequested) {
                    // cancellation requested, remove the live stream files

                    return;
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
                
                _logger.Info("Found .ts files.");

                List<Task> tasks = new List<Task>();


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
                        if (_isLive) {
                            // no need for async download since it is live
                            DownloadPart(line).Wait(cancellationToken);
                        } else {
                            // async download
                            Task task = DownloadPart(line);
                            tasks.Add(task);
                        }
                    }
                }
                
                
                if (!_isLive) {
                    _logger.Info("Waiting for all downloads to complete...");
                    Task.WaitAll(tasks.ToArray());
                    List<string> newPartsCollection = new List<string>();
                    // order the ts list so they are in the correct order, and so combined together in the correct order
                    newPartsCollection = _partsCollection.OrderBy(item => Int32.Parse(item.Substring(item.LastIndexOf("/") + 1, item.LastIndexOf(".") - item.LastIndexOf("/") - 1))).ToList();
                    _partsCollection = newPartsCollection;
                    _logger.Info("Done waiting.");
                    break;
                }
            }

            // no more .ts files found, stream has ended.
            return;
        }

        private async Task DownloadPart(string url) {
            _logger.Info($"Downloading {url}...");
            _partsDirectory = new DirectoryInfo(_rootDirectory.FullName + "/parts");
            if (!_partsDirectory.Exists) {
                _partsDirectory.Create();
            }

            using (var client = new WebClient()) {
                if (_isLive) {
                    client.DownloadFile(url, _partsDirectory.FullName + $"/{_counter}.ts");
                    _partsCollection.Add(_partsDirectory.FullName + $"/{_counter}.ts");
                } else {
                    string formattedUrl = m3u8.Substring(0, m3u8.LastIndexOf("/"));
                    await client.DownloadFileTaskAsync(new Uri($"{formattedUrl}/{url}"), $"{_partsDirectory.FullName}/{url}");
                    _partsCollection.Add(_partsDirectory.FullName + $"/{url}");
                }
            }
            _logger.Info($"Done downloading {url}");
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
            }
            _logger.Info("Created combined .ts file.");
        }

        public void CleanUpFiles() {
            Directory.Delete(_partsDirectory.FullName, true);
        }
    }
}