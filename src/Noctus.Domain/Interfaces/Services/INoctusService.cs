#nullable enable
using FluentResults;
using Noctus.Domain.Entities;
using Noctus.Domain.Models.Dto;
using System.Threading.Tasks;
using Noctus.Domain.Models;

namespace Noctus.Domain.Interfaces.Services
{
    public interface INoctusService
    {
        Task<bool> Login(string licenseKey, string mbSerialNumber, KeyEvent keyEvent, string? discordId);
        Task Logout(string licenseKey, string mbSerialNumber, string discordId);
        Task AddLicenseKeyEventAsync(KeyStatus keyStatus, KeyStatusReason statusReason, string description);
        Task<Result<int>> CreatePipeline(PipelineRunDto pipeline);
        Task CreatePipelineEvent(int pipelineId, PipelineEventType eventType);
        Task SendFinishJob(int pipelineId);
        Task<AccountGenBucket> GetLicenseBucket(string licenseKey);
    }
}
