using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Noctus.Domain.Entities;
using Noctus.Domain.Models.Sms;

namespace Noctus.Domain.Interfaces.Services
{
    public interface ISmsService
    {
        /// <summary>
        /// Get account balance
        /// </summary>
        /// <returns></returns>
        Task<Result<double>> GetBalance();

        /// <summary>
        /// Check account
        /// </summary>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        Task<ApiKeyStatus> ApiKeyHealthCheck(string apiKey);

        /// <summary>
        /// Order phone number
        /// </summary>
        /// <param name="code"></param>
        /// <param name="type"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Result<PhoneNumberOrder>> OrderPhoneNumber(SmsCountryCode code, SmsServiceType type, CancellationToken cancellationToken);

        /// <summary>
        /// Change activation status after requesting phone number
        /// </summary>
        /// <param name="pnOperationCode"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        Task<Result> ChangeActivationStatus(string pnOperationCode, ActivationStatus status);

        /// <summary>
        /// Retrieve sms
        /// </summary>
        /// <param name="pnOperationCode"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Result<string>> GetSmsCode(string pnOperationCode, CancellationToken cancellationToken);

        /// <summary>
        /// Get phone availability by country
        /// </summary>
        /// <param name="code"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        Task<Result<Dictionary<string, int>>> GetAvailablePhones(SmsCountryCode code, IEnumerable<SmsServiceType> types);

        /// <summary>
        /// Get current price & phone numbers availability by country and service type
        /// </summary>
        /// <param name="code"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        Task<Result<SmsPriceAvailability>> GetPrices(SmsCountryCode code, SmsServiceType type);

        /// <summary>
        /// Get current prices & phone numbers availability by service type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        Task<Result<Dictionary<SmsCountryCode, SmsPriceAvailability>>> GetPrices(SmsServiceType type);
    }
}
