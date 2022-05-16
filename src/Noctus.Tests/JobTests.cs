using System;
using System.Threading.Tasks;
using FluentAssertions;
using Noctus.Application.Modules.AccountGen;
using Noctus.Application.PipelineComponents;
using NUnit.Framework;

namespace Noctus.Tests
{
    class JobTests
    {
        [Test]
        public async Task Job_should_cancel_when_timeout()
        {
            // Given
            var job = new Job<AccountGenExecutionContext>(new AccountGenExecutionContext(), TimeSpan.FromMilliseconds(1000));
            job.StartProcessing();

            // When
            await Task.Delay(1500);

            // Then
            job.State.Should().Be(JobState.CANCELLED);
        }

        [Test]
        public async Task Job_should_not_be_cancel_by_timeout_when_job_finish_before()
        {
            // Given
            var job = new Job<AccountGenExecutionContext>(new AccountGenExecutionContext(), TimeSpan.FromMilliseconds(1000));
            job.StartProcessing();

            // When
            await Task.Delay(500);
            job.Finish();

            await Task.Delay(1000);

            // Then
            job.State.Should().Be(JobState.FINISHED);
        }
    }
}
