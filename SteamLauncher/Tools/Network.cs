using System.Net.Http;
using System.Threading.Tasks;

namespace SteamLauncher.Tools
{
    public static class Network
    {
        private static readonly HttpClient Client = new HttpClient();

        /// <summary>
        /// Downloads string data from the provided URL.
        /// </summary>
        /// <param name="url">The URL to download string data from.</param>
        /// <returns>A Task containing information about the network request.</returns>
        public static async Task<string> DownloadStringFromUrl(string url)
        {
            return await Client.GetStringAsync(url).ConfigureAwait(false);
        }
    }
}
