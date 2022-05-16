using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Noctus.Application.Modules.AccountGen;
using Noctus.Application.Modules.AccountGen.Outlook.Steps.Registration;
using NUnit.Framework;
using Noctus.Domain.Models;

namespace Noctus.Tests.Steps
{
    public class FindAvailableUsernameTests
    {
        [Test]
        public async Task Execute_should_fail_when_cancellation_token_is_cancelled()
        {
            // Given
            var client = new HttpClient();

            var ctx = new AccountGenExecutionContext
            {
                Profile = new Account
                {
                    FirstName = "Toto",
                    LastName = "Tata"
                }
            };

            var cts = new CancellationTokenSource();
            var sut = new FindAvailableUsernameStep();

            // When
            cts.Cancel();
            var result = await sut.Execute(client, ctx, cts.Token);

            // Then
            result.IsFailed.Should().BeTrue();
        }
    }
}
