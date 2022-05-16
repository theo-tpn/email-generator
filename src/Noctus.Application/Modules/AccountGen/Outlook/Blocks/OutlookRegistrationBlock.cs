using Noctus.Application.Modules.AccountGen.Outlook.Steps.Registration;
using Noctus.Application.PipelineComponents;
using Noctus.Domain.Interfaces.Pipeline;
using Noctus.Domain.Interfaces.Services;
using System.Collections.Generic;
using Noctus.Application.Modules.AccountGen.Outlook.Steps.Elevated;

namespace Noctus.Application.Modules.AccountGen.Outlook.Blocks
{
    public class OutlookRegistrationBlock : HttpBlockBase<AccountGenExecutionContext>
    {
        private readonly ISmsService _smsService;
        private readonly ICaptchaService _captchaService;

        private readonly bool _enablePhoneVerification;

        protected override string Id => "Outlook_Registration";
        protected override string Name => "Register";

        public OutlookRegistrationBlock(ISmsService smsService, ICaptchaService captchaService, bool enablePhoneVerification)
        {
            _smsService = smsService;
            _captchaService = captchaService;

            _enablePhoneVerification = enablePhoneVerification;
        }

        protected override IEnumerable<IStep> Steps(AccountGenExecutionContext ctx)
        {
            yield return new InitialStep();
            yield return new FindAvailableUsernameStep();
            yield return new PostAccountStep();

            switch (ctx.HotValues[OutlookConstants.Keys.ChallengeType])
            {
                case OutlookConstants.Website.CaptchaEnforcement:
                    yield return new SolveCaptchaEnforcement(_captchaService);
                    break;
                case OutlookConstants.Website.HipPhoneEnforcement:
                    if (!_enablePhoneVerification) RaiseError();
                    yield return new RequestPhoneNumberStep(_smsService);
                    yield return new SolveHipPhoneEnforcementStep(_smsService);
                    break;
            }

            yield return new FinalizeAccountStep();
        }
    }
}
