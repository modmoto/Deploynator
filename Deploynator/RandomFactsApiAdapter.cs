using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Deploynator
{
    public class RandomFactsApiAdapter
    {
        private readonly HttpClient _httpClient;

        public RandomFactsApiAdapter()
        {
            var handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) => { return true; };

            _httpClient = new HttpClient(handler);
            _httpClient.BaseAddress = new Uri("https://icanhazdadjoke.com");
        }

        public async Task<string> GetRandomFactAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("");
                var jsonOptions = new JsonSerializerOptions();
                jsonOptions.PropertyNameCaseInsensitive = true;
                jsonOptions.PropertyNamingPolicy = null;

                return JsonSerializer.Deserialize<WaitingSequenceApiResult>(await response.Content.ReadAsStringAsync()).Joke;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }

    public class WaitingSequenceApiResult
    {
        public string Joke { get; set; }
    }
}