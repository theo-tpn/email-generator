using System.Collections.Generic;
using Noctus.Application.Modules.AccountGen.Outlook.Steps.Elevated;
using Noctus.Application.PipelineComponents;
using Noctus.Domain.Interfaces.Pipeline;
using Noctus.Domain.Interfaces.Services;

namespace Noctus.Application.Modules.AccountGen.Outlook.Blocks
{
    public class OutlookElevatedBlock : HttpBlockBase<AccountGenExecutionContext>
    {
        private readonly ISmsService _smsService;
        private readonly IRecoveryEmailService _recoveryEmailService;

        private readonly bool _enablePhoneVerification;
        private readonly bool _enableRecoveryEmailVerification;

        public OutlookElevatedBlock(ISmsService smsService, IRecoveryEmailService recoveryEmailService, bool enablePhoneVerif, bool enableRecoveryMailVerif) 
        {
            _smsService = smsService;
            _recoveryEmailService = recoveryEmailService;

            _enablePhoneVerification = enablePhoneVerif;
            _enableRecoveryEmailVerification = enableRecoveryMailVerif;
        }

        protected override string Id => "Outlook_Pva";
        protected override string Name => "Elevated Actions";

        protected override IEnumerable<IStep> Steps(AccountGenExecutionContext ctx)
        {
            if (!_enableRecoveryEmailVerification && !_enablePhoneVerification) yield break;

            if (ctx.HotValues[OutlookConstants.Keys.ChallengeType] == OutlookConstants.Website.CaptchaEnforcement && _enableRecoveryEmailVerification)
            {
                yield return new LinkEmailToAccountStep(_recoveryEmailService);
                yield return new EmailVerificationStep(_recoveryEmailService);
            }
            else
            {
                if(!ctx.HotValues.ContainsKey(OutlookConstants.Keys.PhoneNumber))
                {
                    yield return new RequestPhoneNumberStep(_smsService);
                    yield return new LinkPhoneNumberToAccountStep(_smsService);
                }
                yield return new PhoneVerificationStep(_smsService);
            }

            yield return new GetRecoveryCodeStep();
            yield return new AddAliasesStep();

            yield return new EndStep();
        }
    }
}