using System;
using System.Collections.Generic;
using System.IO;
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
            var driver = new DownloadDriver(history, config.OutputDirectory);
            var tasks = new List<Task>();

            foreach (var downloader in config.BuildDownloaders())
            {
                tasks.Add(Download(driver, downloader));
            }

            await Task.WhenAll(tasks);

            // TODO: retriever.GetVersion to retrieve the version of the resource we are pointing at.
            // TODO: Check to see if we've already pulled the version of the resource.
            // TODO: Download resource, if appropriate, which returns path to a file.
            // TODO: "Send" the file, and log it to the "history".

            return 0;
        }

        private static async Task Download(DownloadDriver driver, IDownloader downloader)
        {
            Console.Error.WriteLine($"[{downloader.Name}] Downloading...");

            try
            {
                var result = await driver.Download(downloader);

                if (result.downloaded)
                {
                    Console.Error.WriteLine($"[{downloader.Name}] Downloaded '{result.version}'.");
                }
                else
                {
                    Console.Error.WriteLine($"[{downloader.Name}] Nothing to do. Previously downloaded '{result.version}'.");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[{downloader.Name}] Error:\n{ex.ToString()}\n{ex.StackTrace}");
            }
        }
    }
}
