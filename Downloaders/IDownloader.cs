using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace puako.Downloaders
{
    internal interface IDownloader
    {
        void Init(HttpClient client);

        Task<string> PeekVersionAsync();

        Task<DownloadResult> DownloadAsync(string destination);
    }

    internal abstract class BaseDownloader : IDownloader
    {
        protected HttpClient HttpClient { get; set; }

        public virtual void Init(HttpClient client)
        {
            HttpClient = client;
        }

        public abstract Task<string> PeekVersionAsync();

        public abstract Task<DownloadResult> DownloadAsync(string destination);
    }

    internal class DownloadResult
    {
        public string SuggestedFileName { get; set; }

        public string Version { get; set; }

        public bool IsError { get; set; }

        public bool IsRetryableError { get; set; }

        public Exception Exception { get; set; }
    }
}