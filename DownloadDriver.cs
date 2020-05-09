using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using puako.Downloaders;
using puako.History;

namespace puako
{
    internal class DownloadDriver
    {
        public DownloadDriver(IHistoryProvider history,
            string outputDirectory)
        {
            _history = history;
            _outputDirectory = outputDirectory;
            _tempDirectory = Path.Combine(_outputDirectory, ".tmp");

            Directory.CreateDirectory(_tempDirectory);
        }

        public async Task<(bool downloaded, string version)> Download(IDownloader downloader)
        {
            downloader.Init(_client);

            // Try to take a look at the available version.
            // Give it 3 attempts if exceptions occur.
            string version = null;
            for (var attempt = 1; attempt <= RetryCount; ++attempt)
            {
                try
                {
                    version = await downloader.PeekVersionAsync();
                    break;
                }
                catch { }
            }

            // If we were able to peek at the version, have we downloaded it
            // before? If so, we can stop now.
            if (!string.IsNullOrEmpty(version)
                && await _history.HasDownloadedAsync(downloader.Name, version))
            {
                return (false, version);
            }

            // Generate a temporary file path to store the downloaded file.
            var tempFilePath = Path.Combine(_tempDirectory, Path.GetRandomFileName());

            // Attempt to download 3 times.
            string suggestedFileName = null;
            for (var attempt = 1; attempt <= RetryCount; ++attempt)
            {
                try
                {
                    (version, suggestedFileName) = await downloader.DownloadAsync(tempFilePath);
                }
                catch
                {
                    TryDelete(tempFilePath);

                    if (attempt == RetryCount)
                    {
                        throw;
                    }
                }
            }

            if (string.IsNullOrEmpty(version))
            {
                throw new InvalidOperationException("Missing version after download.");
            }

            if (string.IsNullOrEmpty(suggestedFileName))
            {
                throw new InvalidOperationException("Missing suggestedFileName after download.");
            }

            // Check to see if we've downloaded this before.
            if (await _history.HasDownloadedAsync(downloader.Name, version))
            {
                TryDelete(tempFilePath);
                return (false, version);
            }

            // Rename the file and save the download to our history.
            File.Move(tempFilePath, Path.Combine(_outputDirectory, suggestedFileName));
            await _history.AddDownloadAsync(downloader.Name, version);
            return (true, version);
        }

        private void TryDelete(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch { }
        }

        private const int RetryCount = 3;
        private readonly IHistoryProvider _history;
        private readonly string _outputDirectory;
        private readonly string _tempDirectory;
        private readonly HttpClient _client = new HttpClient(new HttpClientHandler() 
        {
            AllowAutoRedirect = false
        });
    }
}