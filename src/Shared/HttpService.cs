using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Shared
{
    public static class HttpService
    {
        private static readonly Uri _baseAddress;

        static HttpService()
        {
            _baseAddress = new Uri("http://azure-iot-demo.azurewebsites.net/");
        }

        public static async Task<T> Get<T>(string url)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = _baseAddress;
                var responseMessage = await httpClient.GetAsync(url);
                responseMessage.EnsureSuccessStatusCode();
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseContent);
            }
        }
        public static async Task Post(string url, object request)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = _baseAddress;

                var jsonData = JsonConvert.SerializeObject(request);
                var requestContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(url, requestContent);
                response.EnsureSuccessStatusCode();
            }
        }
        public static async Task<T> Post<T>(string url, object request)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = _baseAddress;

                var jsonData = JsonConvert.SerializeObject(request);
                var requestContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(url, requestContent);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseContent);
            }
        }
    }
}
