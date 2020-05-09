using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Puako.History
{
    internal class JsonHistoryProvider : IHistoryProvider
    {
        public JsonHistoryProvider(string filename)
        {
            _filename = filename;

            if (File.Exists(filename))
            {
                _history = JsonConvert.DeserializeObject<Dictionary<string, HashSet<string>>>(
                    File.ReadAllText(filename)
                );
            }
            else
            {
                _history = new Dictionary<string, HashSet<string>>();
            }
        }

        public Task<bool> HasDownloadedAsync(string resourceName, string version)
        {
            return Task.FromResult(
                _history.TryGetValue(resourceName, out var set)
                && set.Contains(version));
        }

        public Task AddDownloadAsync(string resourceName, string version)
        {
            lock(_history)
            {
                if (!_history.TryGetValue(resourceName, out var set))
                {
                    set = new HashSet<string>();
                    _history[resourceName] = set;
                }

                if (set.Add(version))
                {
                    var json = JsonConvert.SerializeObject(_history, Formatting.Indented);

                    File.WriteAllText(_filename, json);
                }
            }

            return Task.FromResult(true);
        }

        private readonly string _filename; 
        private readonly Dictionary<string, HashSet<string>> _history;
    }
}