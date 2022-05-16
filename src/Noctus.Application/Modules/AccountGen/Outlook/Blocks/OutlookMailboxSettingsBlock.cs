using System.Collections.Generic;
using Noctus.Application.Modules.AccountGen.Outlook.Steps.Mailbox;
using Noctus.Application.PipelineComponents;
using Noctus.Domain.Interfaces.Pipeline;

namespace Noctus.Application.Modules.AccountGen.Outlook.Blocks
{
    public class OutlookMailboxSettingsBlock : HttpBlockBase<AccountGenExecutionContext>
    {
        protected override string Id => "Outlook_MailSettings";
        protected override string Name => "Configure";

        protected override IEnumerable<IStep> Steps(AccountGenExecutionContext ctx)
        {
            yield return new LoginStep();
            yield return new SetForwardingStep();
            yield return new SetSafeSendersStep();
            //yield return new PhysicalAddressStep();
        }
    }
}