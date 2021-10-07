using System.Net.Http;
using System.Net.Http.Headers;

namespace BlueStacksAssignment
{
    public static class ApiClient
    {
        public static HttpClient httpClient { get; set; }

        public static void InitializeApiClient()
        {
            if(httpClient == null)
                httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public static void Dispose()
        {
            if (httpClient != null)
                httpClient.Dispose();
        }
    }
}
