using System.Net;
using System.Net.Http;

namespace Puako
{
    public static class HttpExtensions
    {
        public static void EnsureSuccessStatusCodeEx(this HttpResponseMessage resp)
        {
            if (resp.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new TooManyRequestsException
                {
                    RetryAfter = resp.Headers.RetryAfter.Delta,
                };
            }

            resp.EnsureSuccessStatusCode();
        }
    }
}