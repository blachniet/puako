using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using puako.Downloaders;
using puako.History;

namespace puako
{
    class Program
    {
        static async Task<int> Main()
        {
            try
            {
                return await Run();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Unhandled exception: " + ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
                return 1;
            }
        }

        private static async Task<int> Run()
        {
            var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));
            var history = new JsonHistoryProvider("history.json");
            var tmpDir = Path.Combine(config.OutputDirectory, ".tmp");
            var tasks = new List<Task>();

            // Ensure the output directory exists.
            Directory.CreateDirectory(tmpDir);

            foreach (var resource in config.Resources)
            {
                tasks.Add(HandleResource(resource, config, history, tmpDir));
            }

            await Task.WhenAll(tasks);

            // TODO: retriever.GetVersion to retrieve the version of the resource we are pointing at.
            // TODO: Check to see if we've already pulled the version of the resource.
            // TODO: Download resource, if appropriate, which returns path to a file.
            // TODO: "Send" the file, and log it to the "history".

            return 0;
        }

        private static async Task<DownloadResult> Download(
            IDownloader downloader, string destination)
        {
            DownloadResult result = null;

            for (var i = 0; i < 3; ++i)
            {
                result = await downloader.DownloadAsync(destination);

                if (!result.IsError || !result.IsRetryableError)
                {
                    return result;
                }
            }

            return result;
        }

        private static async Task HandleResource(
            Resource resource, Config config, IHistoryProvider history,
            string tmpDir)
        {
            Console.Error.WriteLine($"Downloading '{resource.Name}'...");

            resource.Downloader.Init(_client);

            var version = await resource.Downloader.PeekVersionAsync();

            if (version == null 
                || !await history.HasDownloadedAsync(resource.Name, version))
            {
                var tmpFile = Path.Combine(
                    tmpDir,
                    Path.GetRandomFileName()
                );
                var result = await Download(resource.Downloader, tmpFile);

                if (!result.IsError)
                {
                    // TODO: Handle unset suggestedfilename
                    File.Move(tmpFile, Path.Combine(
                        config.OutputDirectory,
                        result.SuggestedFileName
                    ));
                    await history.AddDownloadAsync(resource.Name, result.Version);

                    Console.Error.WriteLine($"Downloaded '{resource.Name}', '{result.Version}'");
                }
                else
                {
                    var sb = new StringBuilder(
                        $"Error downloading '{resource.Name}'");

                    if (result.Exception != null)
                    {
                        sb.AppendLine(result.Exception.Message);
                        sb.AppendLine(result.Exception.StackTrace);
                    }
                }
            }
        }

        private static HttpClient _client = new HttpClient(new HttpClientHandler() 
        {
            AllowAutoRedirect = false
        });
    }
}
