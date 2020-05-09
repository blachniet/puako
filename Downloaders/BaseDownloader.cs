using System.Net.Http;
using System.Threading.Tasks;

namespace Puako.Downloaders
{
    internal abstract class BaseDownloader : IDownloader
    {
        public string Name { get; set; }

        protected HttpClient HttpClient { get; set; }

        public virtual void Init(HttpClient client)
        {
            HttpClient = client;
        }

        public abstract Task<string> PeekVersionAsync();

        public abstract Task<(string version, string suggestedFileName)> DownloadAsync(
            string destination);
    }
}