
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace puako.Downloaders
{
    internal class RedirectDownloader : BaseDownloader
    {
        public Uri Uri { get; set; }

        public VersionStrategyType VersionStrategy { get; set; }

        public override async Task<string> PeekVersionAsync()
        {
            await Prefetch();
            return _version;
        }

        public override async Task<(string version, string suggestedFileName)> DownloadAsync(
            string destination)
        {
            if (_location == null || _version == null)
            {
                await Prefetch();
            }

            using var srcStream = await HttpClient.GetStreamAsync(_location);
            using var dstStream = File.Create(destination);

            await srcStream.CopyToAsync(dstStream);

            return (_version, _version);
        }

        private async Task Prefetch()
        {
            var response = await HttpClient.GetAsync(Uri);

            if ((int)response.StatusCode < 300
                || (int)response.StatusCode >= 400)
            {
                throw new InvalidOperationException("Unexpected status code");
            }

            if (response.Headers.Location == null)
            {
                throw new InvalidOperationException("Redirect location is empty");
            }

            _location = response.Headers.Location;

            switch (VersionStrategy)
            {
                case VersionStrategyType.ContentDisposition:
                    _version = response.Content.Headers.ContentDisposition.FileName;
                    break;

                case VersionStrategyType.UriFilename:
                    _version = HttpUtility.UrlDecode(
                        Path.GetFileName(response.Headers.Location.AbsolutePath));
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException("Unhandled version strategy.");
            }
        }

        private Uri _location;
        private string _version;

        public enum VersionStrategyType
        {
            ContentDisposition,
            UriFilename,
        }
    }
}