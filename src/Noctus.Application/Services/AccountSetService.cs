using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using FluentResults;
using FluentValidation;
using MailKit.Net.Imap;
using Noctus.Domain.Interfaces.Repositories;
using Noctus.Domain.Interfaces.Services;
using Noctus.Domain.Models;
using Noctus.Infrastructure;
using Stl.Collections;
using Stl.Fusion;

namespace Noctus.Application.Services
{
    [ComputeService]
    public class AccountSetService : IAccountSetService
    {
        private readonly IAccountSetRepository _accountSetRepository;
        private readonly INewsletterService _newsletterService;

        public AccountSetService(
            IAccountSetRepository accountSetRepository,
            INewsletterService newsletterService)
        {
            _accountSetRepository = accountSetRepository;
            _newsletterService = newsletterService;
        }

        [ComputeMethod(AutoInvalidateTime = 30)]
        public virtual async Task<int> GetGeneratedAccountsCount()
        {
            var sets = await _accountSetRepository.Get();
            var result = sets.Sum(x => x.Accounts.Count);
            return result;
        }

        public async Task RunVerification(int setId)
        {
            var set = await _accountSetRepository.Find(setId);
            set.SetState(AccountSetState.CLIP_TESTING);
            _accountSetRepository.Update(set);

            var process = new ActionBlock<Account>(async account =>
            {
                try
                {
                    using var client = new ImapClient();
                    await client.ConnectAsync("outlook.office365.com", 993, true);
                    await client.AuthenticateAsync(account.Username, account.Password);
                    await client.DisconnectAsync(true);
                    account.ClipStatus = ClipStatus.VALID;
                }
                catch (Exception)
                {
                    account.ClipStatus = ClipStatus.CLIPPED;
                }

                account.LastClipVerification = DateTime.Now;
            }, new ExecutionDataflowBlockOptions { EnsureOrdered = false, MaxDegreeOfParallelism = 10 });

            foreach (var account in set.Accounts)
            {
                await process.SendAsync(account);
            }

            process.Complete();

            await process.Completion;

            set.SetState(AccountSetState.NONE);
            _accountSetRepository.Update(set);
        }

        public async Task NewsletterSubscription(int setId)
        {
            var set = await _accountSetRepository.Find(setId);
            set.SetState(AccountSetState.NEWSLETTER_SUBSCRIPTION);
            _accountSetRepository.Update(set);

            var process = new ActionBlock<Account>(async account =>
            {
                try
                {
                    await _newsletterService.SubscribeToAll(account.Username);
                }
                catch (Exception)
                {

                }
            }, new ExecutionDataflowBlockOptions {EnsureOrdered = false, MaxDegreeOfParallelism = 10});

            foreach (var account in set.Accounts)
            {
                await process.SendAsync(account);
            }

            process.Complete();

            await process.Completion;

            set.SetState(AccountSetState.NONE);
            _accountSetRepository.Update(set);
        }

        public async Task<Result<int>> ImportAccounts(int setId, Dictionary<AccountColumnType, string> columns,
            IList<dynamic> records)
        {
            var parseResult = await Parse(columns, records);

            if (parseResult.IsFailed)
                return parseResult.ToResult();

            var set = await _accountSetRepository.Find(setId);
            set.Accounts.AddRange(parseResult.Value);
            _accountSetRepository.Update(set);

            return parseResult.ToResult(list => list.Count);
        }

        public async Task<Result<int>> ImportAccounts(Dictionary<AccountColumnType, string> columns,
            IList<dynamic> records)
        {
            var parseResult = await Parse(columns, records);

            if (parseResult.IsFailed)
                return parseResult.ToResult();

            return _accountSetRepository.Insert(null, parseResult.Value);
        }

        private static async Task<Result<List<Account>>> Parse(Dictionary<AccountColumnType, string> columns, IList<dynamic> records)
        {
            if (string.IsNullOrEmpty(columns[AccountColumnType.Username]) ||
                string.IsNullOrEmpty(columns[AccountColumnType.Password]))
                return Result.Fail("Username and Password columns must be bind");

            if (!records.Any())
                return Result.Fail("No records to import");

            var accounts = new List<Account>();
            var validator = new AccountValidator();

            var importResult = Result.Ok(0);

            foreach (var record in records.Select((r, i) => new {value = r, index = i}))
            {
                var rec = (IDictionary<string, object>) record.value;

                try
                {
                    var account = new Account
                    {
                        Username = rec[columns[AccountColumnType.Username]] as string,
                        Password = rec[columns[AccountColumnType.Password]] as string,
                        RecoveryCode = !string.IsNullOrEmpty(columns[AccountColumnType.RecoveryCode])
                            ? rec[columns[AccountColumnType.RecoveryCode]] as string
                            : string.Empty,
                        RecoveryEmail = !string.IsNullOrEmpty(columns[AccountColumnType.RecoveryEmail])
                            ? rec[columns[AccountColumnType.RecoveryEmail]] as string
                            : string.Empty,
                        MasterForward = !string.IsNullOrEmpty(columns[AccountColumnType.MasterForwarding])
                            ? rec[columns[AccountColumnType.MasterForwarding]] as string
                            : string.Empty
                    };

                    var result = await validator.ValidateAsync(account);

                    if (result.IsValid)
                    {
                        accounts.Add(account);
                    }
                    else
                    {
                        importResult.WithReason(new WarningMessage($"Validation failed for line {record.index}"));
                    }
                }
                catch (Exception _)
                {
                    importResult.WithReason(new WarningMessage($"Error parsing line {record.index}"));
                }
            }

            if (!accounts.Any())
                return Result.Fail("All accounts failed to pass validation");

            return Result.Ok(accounts);
        }
    }

    public class AccountValidator : AbstractValidator<Account>
    {
        public AccountValidator()
        {
            RuleFor(x => x.Username)
                .EmailAddress();

            RuleFor(x => x.Username)
                .Must(s => s.Split("@")[1].Contains("outlook"));

            RuleFor(x => x.Password)
                .NotNull()
                .NotEmpty();

            RuleFor(x => x.RecoveryEmail)
                .EmailAddress()
                .Unless(x => string.IsNullOrEmpty(x.RecoveryEmail));
        }
    }
}
