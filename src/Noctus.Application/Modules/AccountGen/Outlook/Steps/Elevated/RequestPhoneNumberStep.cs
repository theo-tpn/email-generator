using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Noctus.Application.ExternalServices;
using Noctus.Application.PipelineComponents;
using Noctus.Domain.Interfaces.Services;
using Noctus.Domain.Models.Sms;

namespace Noctus.Application.Modules.AccountGen.Outlook.Steps.Elevated
{
    public class RequestPhoneNumberStep : StepBase<AccountGenExecutionContext>
    {
        public override string Description => "Requesting phone number";

        private readonly ISmsService _smsService;

        public RequestPhoneNumberStep(ISmsService smsService)
        {
            _smsService = smsService;
        }

        protected override async Task<Result> ExecuteInner(HttpClient client, AccountGenExecutionContext ctx,
            CancellationToken cancellationToken)
        {
            if (ctx.HotValues.ContainsKey(OutlookConstants.Keys.PhoneNumber)) 
                return Result.Ok("Phone number already requested");
            
            var result =
                await _smsService.OrderPhoneNumber(CountryCode.SmsCountryCodeLookup[ctx.Profile.PhoneCountryCode], SmsServiceType.Microsoft, cancellationToken).ConfigureAwait(false);
            
            if (result.IsFailed)
                return result;

            ctx.HotValues[OutlookConstants.Keys.PhoneNumber] = result.Value.PhoneNumber;
            ctx.HotValues[OutlookConstants.Keys.PhoneNumberCountryCode] = result.Value.PhoneCountryCode;
            ctx.HotValues[OutlookConstants.Keys.PhoneNumberLastFourDigits] = result.Value.PhoneNumber[^4..];
            ctx.HotValues[OutlookConstants.Keys.PhoneOperationCode] = result.Value.OperationCode;

            return Result.Ok();
        }
    }
}
