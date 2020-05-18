using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;
using Puako.Downloaders;

namespace Puako
{
    internal class VsixDownloader : BaseDownloader
    {
        public string Publisher { get; set; }

        public string ID { get; set; }

        public override Task<string> PeekVersionAsync()
        {
            return Task.FromResult<string>(null);
        }

        public override async Task<(string version, string suggestedFileName)> DownloadAsync(string destination)
        {
            var uri = string.Format(UrlTemplate, Publisher, ID);

            using (var resp = await HttpClient.GetAsync(uri))
            using (var dstStream = File.Create(destination))
            {
                resp.EnsureSuccessStatusCodeEx();

                await resp.Content.CopyToAsync(dstStream);
            }

            var version = ParseVsixVersion(destination);
            var suggestedFileName = string.Format(FileNameTemplate, Publisher, ID, version);
            return (version, suggestedFileName);
        }

        private static string ParseVsixVersion(string path)
        {
            using var zip = ZipFile.OpenRead(path);
            using var manifest = zip.Entries
                .First(x => x.FullName.Equals("extension.vsixmanifest"))
                .Open();
            
            // The top of this manifest looks like this:
            // <?xml version="1.0" encoding="utf-8"?>
            // <PackageManifest Version="2.0.0" xmlns="http://schemas.microsoft.com/developer/vsx-schema/2011" xmlns:d="http://schemas.microsoft.com/developer/vsx-schema-design/2011">
            //   <Metadata>
            //     <Identity Language="en-US" Id="vscode-docker" Version="1.1.0" Publisher="ms-azuretools"/>
            //     <DisplayName>Docker</DisplayName>
            var doc = XDocument.Load(manifest);
            var identityName = XName.Get("{http://schemas.microsoft.com/developer/vsx-schema/2011}Identity");
            var identity = doc.Descendants(identityName).First();
            return identity.Attribute("Version").Value;
        }

        private const string UrlTemplate = "https://marketplace.visualstudio.com/_apis/public/gallery/publishers/{0}/vsextensions/{1}/latest/vspackage";
        private const string FileNameTemplate = "{0}.{1}-{2}.vsix";
    }
}