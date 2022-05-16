#nullable enable
using FluentResults;
using Newtonsoft.Json;
using Noctus.Application.Helpers;
using Noctus.Domain.Entities;
using Noctus.Domain.Interfaces.Services;
using Noctus.Domain.Models.Dto;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Noctus.Domain.Models;
using static System.Int32;

namespace Noctus.Application.Services
{
    public class NoctusService : INoctusService
    {
        private readonly HttpClient _client;

        public NoctusService(HttpClient client)
        {
            _client = client;
            _client.BaseAddress = new Uri("API URL");
        }
        public async Task<bool> Login(string licenseKey, string mbSerialNumber, KeyEvent keyEvent, string? discordId)
        {
            try
            {
                var httpRequest = new HttpRequestMessage()
                {
                    Method = HttpMethod.Post,
                    Content = new StringContent(JsonConvert.SerializeObject(new LicenseEventDto()
                    {
                        LicenseKey = licenseKey,
                        MbSerialInfo = MachineInfoHelper.BuildLicenseIdentifiersInfo().MotherBoardSerialNumber,
                        Event = keyEvent
                    }), Encoding.UTF8, "application/json"),
                    RequestUri = new Uri($"{_client.BaseAddress}genwave/license/login/")
                };
                var response = await _client.SendAsync(httpRequest);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task Logout(string licenseKey, string mbSerialNumber, string discordId)
        {
            try
            {
                var httpRequest = new HttpRequestMessage()
                {
                    Method = HttpMethod.Post,
                    Content = new StringContent(JsonConvert.SerializeObject(new LicenseEventDto()
                    {
                        LicenseKey = licenseKey,
                        MbSerialInfo = MachineInfoHelper.BuildLicenseIdentifiersInfo().MotherBoardSerialNumber,
                        UserDiscordId = discordId,
                        Event = KeyEvent.Logout
                    }), Encoding.UTF8, "application/json"),
                    RequestUri = new Uri($"{_client.BaseAddress}genwave/license/logout")
                };
                await _client.SendAsync(httpRequest);
            }
            catch (Exception)
            {
                // ignored
            }
        }
        public async Task AddLicenseKeyEventAsync(KeyStatus keyStatus, KeyStatusReason statusReason, string description)
        {
            var keyStatusInfo = new LicenseKeyFlag()
            {
                Status = keyStatus,
                Reason = statusReason,
                Description = description,
                IdentifiersInfo = MachineInfoHelper.BuildLicenseIdentifiersInfo()
            };

            var content = new StringContent(JsonConvert.SerializeObject(keyStatusInfo), Encoding.UTF8, "application/json");
            await _client.PostAsync("license/flags", content);
        }
        public async Task<Result<int>> CreatePipeline(PipelineRunDto pipeline)
        {
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(pipeline), Encoding.UTF8, "application/json"),
                RequestUri = new Uri($"{_client.BaseAddress}pipeline")
            };

            var response = await _client.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return Result.Fail(new Error(response.ReasonPhrase));

            TryParse(responseContent, out var result);

            return Result.Ok(result);
        }
        public async Task CreatePipelineEvent(int pipelineId, PipelineEventType eventType)
        {
            var httpRequest = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{_client.BaseAddress}pipeline/{pipelineId}/event/{(int)eventType}")
            };
            await _client.SendAsync(httpRequest);
        }
        public async Task SendFinishJob(int pipelineId)
        {
            var httpRequest = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{_client.BaseAddress}pipeline/{pipelineId}/job/finish")
            };
            await _client.SendAsync(httpRequest);
        }
        public async Task<AccountGenBucket> GetLicenseBucket(string licenseKey)
        {
            var httpRequestMessage = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{_client.BaseAddress}genwave/license/{licenseKey}/bucket")
            };
            var request = await _client.SendAsync(httpRequestMessage);
            return JsonConvert.DeserializeObject<AccountGenBucket>(await request.Content.ReadAsStringAsync());
        }
    }
}
