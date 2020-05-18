using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Puako.Downloaders;

namespace Puako
{
    internal class Config
    {
        public string OutputDirectory { get; set; }

        public JObject[] Downloaders { get; set; }

        public IEnumerable<IDownloader> BuildDownloaders()
        {
            foreach (var jobj in Downloaders)
            {
                var kind = jobj.Value<string>("kind").ToLowerInvariant();
                var name = jobj.Value<string>("name").ToLowerInvariant();

                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException(
                        $"Missing or invalid name on downloader.\n{jobj.ToString()}");
                }

                var fullName = $"{kind}/{name}";

                if (!_fullNames.Add(fullName))
                {
                    throw new ArgumentException(
                        $"Duplicate downloader name: {fullName}");
                }

                switch (kind)
                {
                    case RedirectDownloader.KindValue:
                        yield return jobj.ToObject<RedirectDownloader>();
                        break;

                    case VsixDownloader.KindValue:
                        yield return jobj.ToObject<VsixDownloader>();
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(
                            $"Unsupported kind: '{kind}'"
                        );
                }
            }
        }

        private HashSet<string> _fullNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }
}