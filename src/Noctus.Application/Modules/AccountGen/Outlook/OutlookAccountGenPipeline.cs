using System.Collections.Generic;
using Noctus.Application.Modules.AccountGen.Outlook.Blocks;
using Noctus.Application.Modules.Shared;
using Noctus.Application.PipelineComponents;
using Noctus.Domain.Interfaces.Repositories;
using Noctus.Domain.Interfaces.Services;

namespace Noctus.Application.Modules.AccountGen.Outlook
{
    public class OutlookAccountGenPipeline : AccountGenPipeline, IOutlookAccountGenPipeline
    {
        private readonly IRecoveryEmailService _recoveryEmailService;

        public OutlookAccountGenPipeline( 
            ISmsService smsService,
            ICaptchaService captchaService, 
            INewsletterService newsletterService,
            IRecoveryEmailService recoveryEmailService,
            INoctusService noctusService,
            IProxyService proxyService,
            IAccountSetRepository repository,
            IHarvestedCookiesRepository harvestedCookiesRepository)
            : base(smsService, captchaService, newsletterService, recoveryEmailService, noctusService, proxyService, repository, harvestedCookiesRepository)
        {
            _recoveryEmailService = recoveryEmailService;
        }

        protected override List<IBlock<AccountGenExecutionContext>> DefineJobBlocks(
            AccountGenSettings accountGenSettings)
        {
            var list = new List<IBlock<AccountGenExecutionContext>>
            {
                new OutlookRegistrationBlock(SmsService, CaptchaService, accountGenSettings.EnablePhoneVerification),
                new OutlookElevatedBlock(SmsService, _recoveryEmailService, accountGenSettings.EnablePhoneVerification, accountGenSettings.EnableEmailRecoveryVerification),
                new OutlookMailboxSettingsBlock()
            };

            if (accountGenSettings.RegisterToNewsletter)
            {
                list.Add(new RegisterNewsletterBlock(NewsletterService));
            }

            return list;
        }
    }
}
