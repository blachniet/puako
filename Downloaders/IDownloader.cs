using System.Net.Http;
using System.Threading.Tasks;

namespace Puako.Downloaders
{
    internal interface IDownloader
    {
        string Name { get; }

        void Init(HttpClient client);

        Task<string> PeekVersionAsync();

        Task<(string version, string suggestedFileName)> DownloadAsync(string destination);
    }
}