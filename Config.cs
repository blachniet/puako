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

                switch (kind)
                {
                    case "redirect":
                        yield return jobj.ToObject<RedirectDownloader>();
                        break;

                    case "vsix":
                        yield return jobj.ToObject<VsixDownloader>();
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(
                            $"Unsupported kind: '{kind}'"
                        );
                }
            }
        }
    }
}