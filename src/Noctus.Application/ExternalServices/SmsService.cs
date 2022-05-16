using FluentResults;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Noctus.Domain.Entities;
using Noctus.Domain.Interfaces.Services;
using Noctus.Domain.Models.Sms;
using Noctus.Infrastructure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Noctus.Domain.Models;

namespace Noctus.Application.ExternalServices
{
    public class SmsService : ISmsService
    {
        private readonly HttpClient _httpClient;

        private readonly IOptionsMonitor<UserSettings> _userSettings;

        private string _baseEndpoint =>
            $"stubs/handler_api.php?api_key={_userSettings.CurrentValue.ExternalServices.SmsActivateRuApiKey}";

        private const int Delay = 3000;
        private const int MaxRetries = 30;

        private readonly HashSet<string> _errors = new HashSet<string>
        {
            "NO_KEY",
            "BAD_KEY",
            "ERROR_SQL",
            "BAD_STATUS",
            "NO_ACTIVATION",
            "BAD_SERVICE",
            "BAD_ACTION"
        };

        public SmsService(HttpClient client, IOptionsMonitor<UserSettings> userSettings)
        {
            _httpClient = client;
            _httpClient.BaseAddress = new Uri("https://sms-activate.ru/");
            _userSettings = userSettings;
        }

        /// <summary>
        /// Get account balance
        /// </summary>
        /// <returns></returns>
        public async Task<Result<double>> GetBalance()
        {
            var response = await _httpClient.GetAsync($"{_baseEndpoint}&action=getBalance");

            if (!response.IsSuccessStatusCode)
                return Result.Fail(new Error(response.ReasonPhrase));

            var content = await response.Content.ReadAsStringAsync();
            var parts = content.Split(':');

            if (_errors.Contains(parts[0]))
                return Result.Fail(new Error("Failed to get balance").WithMetadata("reason", parts[0]));

            return Result.Ok(double.Parse(parts[1], CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Check account
        /// </summary>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        public async Task<ApiKeyStatus> ApiKeyHealthCheck(string apiKey)
        {
            var response = await _httpClient.GetAsync($"stubs/handler_api.php?api_key={apiKey}&action=getBalance");

            if (!response.IsSuccessStatusCode)
                return ApiKeyStatus.Ko;

            var content = await response.Content.ReadAsStringAsync();
            var parts = content.Split(':');

            return _errors.Contains(parts[0]) ? ApiKeyStatus.Ko : ApiKeyStatus.Ok;
        }

        /// <summary>
        /// Order phone number
        /// </summary>
        /// <param name="code"></param>
        /// <param name="type"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Result<PhoneNumberOrder>> OrderPhoneNumber(SmsCountryCode code, SmsServiceType type, CancellationToken cancellationToken)
        {
            var counter = 0;
            string content;

            do
            {
                counter++;

                if (counter > MaxRetries)
                    return Result.Fail(new Error("Max retries exceeded"));

                if (counter > 1) await Task.Delay(Delay, cancellationToken);

                var request =
                    await _httpClient.GetAsync($"{_baseEndpoint}&action=getNumber&service={type.ShortName}&ref=1034269&country={(int)code}", cancellationToken);

                if (!request.IsSuccessStatusCode)
                    return Result.Fail(new Error(request.ReasonPhrase));

                content = await request.Content.ReadAsStringAsync(cancellationToken);

                if (content.Contains("NO_BALANCE") || content.Contains("NO_BALANCE_FORWARD"))
                    return Result.Fail(new SmsServiceBalanceError());

            } while (!content.Contains("ACCESS_NUMBER"));

            var args = content.Split(':');
            var pCc = args[2].Substring(0, 2);
            var pn = $"0{args[2].Remove(0, 2)}";
            var operationCode = args[1];
            return Result.Ok(new PhoneNumberOrder(pCc, pn, operationCode));
        }

        /// <summary>
        /// Change activation status after requesting phone number
        /// </summary>
        /// <param name="pnOperationCode"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public async Task<Result> ChangeActivationStatus(string pnOperationCode, ActivationStatus status)
        {
            var response =
                await _httpClient.GetAsync($"{_baseEndpoint}&action=setStatus&status={(int)status}&id={pnOperationCode}");

            if (!response.IsSuccessStatusCode)
                return Result.Fail(new Error(response.ReasonPhrase));

            var content = await response.Content.ReadAsStringAsync();

            return _errors.Contains(content)
                ? Result.Fail(new Error("Failed to change activation status").WithMetadata("reason", content))
                : Result.Ok();
        }

        /// <summary>
        /// Retrieve sms
        /// </summary>
        /// <param name="pnOperationCode"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Result<string>> GetSmsCode(string pnOperationCode, CancellationToken cancellationToken)
        {
            string content;
            var counter = 0;

            do
            {
                counter++;

                if (counter > MaxRetries)
                    return Result.Fail(new Error("Max retries exceeded"));

                if (counter > 1) await Task.Delay(Delay, cancellationToken);

                var response = await _httpClient.GetAsync($"{_baseEndpoint}&action=getStatus&id={pnOperationCode}", cancellationToken);

                if (!response.IsSuccessStatusCode)
                    return Result.Fail(new Error(response.ReasonPhrase));

                content = await response.Content.ReadAsStringAsync(cancellationToken);
            } while (!content.Contains("STATUS_OK"));

            var args = content.Split(':');
            return Result.Ok(args[1]);
        }

        /// <summary>
        /// Get phone availability by country
        /// </summary>
        /// <param name="code"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public async Task<Result<Dictionary<string, int>>> GetAvailablePhones(SmsCountryCode code, IEnumerable<SmsServiceType> types)
        {
            var response =
                await _httpClient.GetAsync($"{_baseEndpoint}&action=getNumbersStatus&country={(int)code}");

            var content = await response.Content.ReadAsStringAsync();

            if (_errors.Contains(content))
                return Result.Fail(new Error("Failed to get available phones").WithMetadata("reason", content));

            var json = JsonConvert.DeserializeObject<dynamic>(content);

            var dictionary = new Dictionary<string, int>();

            foreach (var type in types)
            {
                var value = int.Parse(json[$"{type.ShortName}_{type.IncludeRedirection}"].ToString());
                dictionary.Add(type.Label, value);
            }

            return Result.Ok(dictionary);
        }

        /// <summary>
        /// Get current price & phone numbers availability by country and service type
        /// </summary>
        /// <param name="code"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<Result<SmsPriceAvailability>> GetPrices(SmsCountryCode code, SmsServiceType type)
        {
            var requestTime = DateTime.Now;

            var response =
                await _httpClient.GetAsync($"{_baseEndpoint}&action=getPrices&country={(int)code}&service={type.ShortName}");

            var content = await response.Content.ReadAsStringAsync();

            if (_errors.Contains(content))
                return Result.Fail(new Error("Failed to get prices").WithMetadata("reason", content));

            var json = JsonConvert.DeserializeObject<dynamic>(content);
            var value = json[$"{(int)code}"][type.ShortName];
            var cost = value["cost"].ToString();
            var count = value["count"].ToString();

            return Result.Ok(new SmsPriceAvailability(type, cost, int.Parse(count), requestTime));
        }

        /// <summary>
        /// Get current prices & phone numbers availability by service type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<Result<Dictionary<SmsCountryCode, SmsPriceAvailability>>> GetPrices(SmsServiceType type)
        {
            var requestTime = DateTime.Now;

            var response =
                await _httpClient.GetAsync($"{_baseEndpoint}&action=getPrices&service={type.ShortName}");

            var content = await response.Content.ReadAsStringAsync();

            if (_errors.Contains(content))
                return Result.Fail(new Error("Failed to get prices").WithMetadata("reason", content));

            var json = JsonConvert.DeserializeObject<dynamic>(content);

            var dictionary = new Dictionary<SmsCountryCode, SmsPriceAvailability>();

            foreach (var code in Enum.GetValues<SmsCountryCode>())
            {
                try
                {
                    var value = json[$"{(int) code}"][type.ShortName];
                    var cost = value["cost"].ToString();
                    var count = value["count"].ToString();
                    dictionary.Add(code, new SmsPriceAvailability(type, double.Parse(cost), int.Parse(count), requestTime));
                }
                catch (Exception e)
                {
                }
            }

            return Result.Ok(dictionary);
        }
    }
}
