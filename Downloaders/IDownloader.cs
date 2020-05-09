using System.Net.Http;
using System.Threading.Tasks;

namespace puako.Downloaders
{
    internal interface IDownloader
    {
        string Name { get; }

        void Init(HttpClient client);

        Task<string> PeekVersionAsync();

        Task<(string version, string suggestedFileName)> DownloadAsync(string destination);
    }
}