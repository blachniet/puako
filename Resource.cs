using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Puako.Downloaders;

namespace Puako
{
    internal class Resource
    {
        public string Name { get; set; }

        [JsonProperty("Downloader")]
        public DownloaderDefinition DownloaderDefinition { get; set; }

        [JsonIgnore]
        public IDownloader Downloader
        {
            get
            {
                if (_downloader == null)
                {
                    switch (DownloaderDefinition.Kind.ToLowerInvariant())
                    {
                        case "redirect":
                            _downloader = DownloaderDefinition.Spec.ToObject<RedirectDownloader>();
                            break;
                        
                        default:
                            throw new ArgumentOutOfRangeException("Unsupported downloader kind.");
                    }
                }

                return _downloader;
            }
        }

        private IDownloader _downloader;
    }

    internal class DownloaderDefinition
    {
        public string Kind { get; set; }

        public JObject Spec { get; set; }
    }
}