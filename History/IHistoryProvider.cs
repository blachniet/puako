using System.Threading.Tasks;

namespace Puako.History
{
    internal interface IHistoryProvider
    {
        Task<bool> HasDownloadedAsync(string resourceName, string version);

        Task AddDownloadAsync(string resourceName, string version);
    }
}