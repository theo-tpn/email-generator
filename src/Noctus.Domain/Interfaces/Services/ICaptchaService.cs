using System.Threading.Tasks;
using FluentResults;
using Noctus.Domain.Entities;

namespace Noctus.Domain.Interfaces.Services
{
    public interface ICaptchaService
    {
        Task<Result<string>> SolveFunCaptcha(string siteKey, string url, string userAgent);
        Task<Result<double>> GetBalance();
        Task<ApiKeyStatus> ApiKeyHealthCheck(string key);
    }
}
