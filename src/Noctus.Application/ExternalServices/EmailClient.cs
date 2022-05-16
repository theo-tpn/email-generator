#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MailKit.Net.Imap;
using Noctus.Application.Helpers;
using Noctus.Domain.Models.Emails;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using MailKit;
using MailKit.Search;
using MimeKit;
using Noctus.Application.Modules.AccountGen.Outlook;

namespace Noctus.Application.ExternalServices
{
    public class EmailClient : IDisposable
    {
        private readonly ImapClient _client;

        private readonly string _username;
        private readonly string _password;
        private readonly EmailProvider _provider;

        private int _maxRetries = 30;

        public EmailClient(string username, string password, EmailProvider provider)
        {
            _client = new ImapClient();
            _username = username;
            _password = password;
            _provider = provider;
        }

        public async Task Connect(CancellationToken cancelToken)
        {
            await _client.ConnectAsync(EmailHelper.ProvidersHost[_provider], 993, true, cancelToken).ConfigureAwait(false);
            await _client.AuthenticateAsync(_username, _password, cancelToken).ConfigureAwait(false);
        }

        public async Task<Result<string?>> GetOutlookSecurityCode(Regex lookupRegex, string username, CancellationToken cancellationToken)
        {
            var inbox = _client.Inbox;
            if (inbox == null) return Result.Fail("Cannot access inbox folder.");

            var twoLettersUsername = username.Substring(0, 2);
            await inbox.OpenAsync(FolderAccess.ReadWrite, cancellationToken).ConfigureAwait(false);
            IEnumerable<UniqueId> result;
            var retry = 0;
            var code = string.Empty;
            var bodyShouldContains = $"{username.Substring(0, 2)}*****@outlook.com";
            var sentDate = DateTime.Now;
            sentDate = sentDate.AddHours(-7);
            sentDate = sentDate.AddMinutes(-1);
            try
            {
                do
                {
                    if (retry > 0)
                        await Task.Delay(3000, cancellationToken).ConfigureAwait(false);
                    retry++;
                    var query = SearchQuery.And(SearchQuery.FromContains(OutlookConstants.Website.MicrosoftSecurityEmail),
                        SearchQuery.BodyContains(bodyShouldContains));
                    result = (await inbox.SearchAsync(query, cancellationToken)).Reverse().Take(2);

                    foreach (var uniqueId in result)
                    {
                        var message = await inbox.GetMessageAsync(uniqueId, cancellationToken).ConfigureAwait(false);
                        var mailUsernameMatch = OutlookConstants.RegexPatterns.TwoLetterUsernameRecoveryMail.Match(message.TextBody);
                        var codeMatch = lookupRegex.Match(message.TextBody);
                        if (!mailUsernameMatch.Success || !codeMatch.Success) continue;
                        var twoLetterUsernameMatch = mailUsernameMatch.Value.Substring(0, 2);
                        if (!string.Equals(twoLetterUsernameMatch, twoLettersUsername)) continue;
                        code = codeMatch.Value;
                        break;
                    }

                } while (string.IsNullOrEmpty(code) && retry <= _maxRetries);
            }
            catch (Exception e)
            {
                return Result.Fail(e.Message);
            }

            if (string.IsNullOrEmpty(code))
                return Result.Fail("no error code");

            return Result.Ok(code);
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
