using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebFramework
{
    public class DownloadFile
    {
        public static async Task<string> Download(string url, string file = null, HttpMethod httpMethod = null, NameValueCollection headers = null)
        {
            var http = new HttpClient();
            if (httpMethod == null) httpMethod = HttpMethod.Get;
            var request = new HttpRequestMessage(httpMethod, url);
            if (headers != null)
            {
                foreach (string key in headers.Keys) request.Headers.Add(key, headers[key]);
            }
            var response = await http.SendAsync(request);
            response.EnsureSuccessStatusCode();
            if (string.IsNullOrEmpty(file)) file = Path.GetTempFileName();
            using (var fs = File.Open(file, FileMode.Create))
            {
                using var ms = response.Content.ReadAsStream(); await ms.CopyToAsync(fs);
            }
            return file;
        }

    }
}
