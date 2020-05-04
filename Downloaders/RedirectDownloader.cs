
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace puako.Downloaders
{
    internal class RedirectDownloader : BaseDownloader
    {
        public Uri Uri { get; set; }

        public VersionStrategyType VersionStrategy { get; set; }

        public override async Task<string> PeekVersionAsync()
        {
            // TODO: Retryable peek version?
            try
            {
                await Prefetch();
            }
            catch
            {
                return null;
            }

            return _version;
        }

        public override async Task<DownloadResult> DownloadAsync(string destination)
        {
            try
            {
                if (_location == null || _version == null)
                {
                    await Prefetch();
                }

                using var srcStream = await HttpClient.GetStreamAsync(_location);
                using var dstStream = File.Create(destination);

                srcStream.CopyTo(dstStream);

                return new DownloadResult
                {
                    SuggestedFileName = _version,
                    Version = _version,
                };
            }
            catch (Exception ex)
            {
                return new DownloadResult
                {
                    IsError = true,
                    IsRetryableError = true,
                    Exception = ex,
                };
            }
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
                    _version = Path.GetFileName(response.Headers.Location.AbsolutePath);
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