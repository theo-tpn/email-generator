using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Noctus.Application.PipelineComponents;
using Noctus.Domain.Interfaces.Services;

namespace Noctus.Application.Modules.AccountGen.Outlook.Steps.Registration
{
    public class SolveCaptchaEnforcement : StepBase<AccountGenExecutionContext>
    {
        private readonly ICaptchaService _captchaService;

        public override string Description => "Resolve captcha";

        public SolveCaptchaEnforcement(ICaptchaService captchaService)
        {
            _captchaService = captchaService;
        }

        protected override async Task<Result> ExecuteInner(HttpClient client, AccountGenExecutionContext ctx,
            CancellationToken cancellationToken)
        {
            var result = await _captchaService.SolveFunCaptcha(OutlookConstants.Website.CaptchaKey,
                OutlookConstants.Website.SignUpUrl, ctx.UserAgent).ConfigureAwait(false);

            if (result.IsFailed)
                return result;

            ctx.HotValues[OutlookConstants.Keys.HipCaptchaSolution] = result.Value;
            return Result.Ok();
        }
    }
}