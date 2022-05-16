using Noctus.Domain.Interfaces.Services;
using Noctus.Domain.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Noctus.Application.Services
{
    public class MetalabsService : IMetalabsService
    {
        private readonly HttpClient _client;
        public MetalabsService(HttpClient client)
        {
            _client = client;
            _client.BaseAddress = new Uri("https://api.metalabs.io/v4/");
            _client.DefaultRequestHeaders.Add("Authorization", "Bearer MetalabsKeyHere");
        }
        
        public async Task<MetalabsLicense> GetMetalabsLicense(string key)
        {
            var metalabsRequest = await _client.GetAsync($"licenses/{key}");
            if (!metalabsRequest.IsSuccessStatusCode) 
                return null;

            var metalabsStr = await metalabsRequest.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<MetalabsLicense>(metalabsStr);
        }
    }
}
