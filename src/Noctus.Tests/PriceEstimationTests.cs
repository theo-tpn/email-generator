using System;
using FluentAssertions;
using NUnit.Framework;

namespace Noctus.Tests
{
   public class PriceEstimationTests
    {
        [Test]
        public void Test()
        {
            var (captchaCounter, smsCounter) = ComputeTasksPerService(1, false, 0);

            captchaCounter.Should().Be(0);
            smsCounter.Should().Be(1);
        }

        [Test]
        public void Test2()
        {
            var (captchaCounter, smsCounter) = ComputeTasksPerService(10, false, 0);

            captchaCounter.Should().Be(2);
            smsCounter.Should().Be(8);
        }

        [Test]
        public void Test3()
        {
            var (captchaCounter, smsCounter) = ComputeTasksPerService(1, true, 1);

            captchaCounter.Should().Be(1);
            smsCounter.Should().Be(0);
        }


        [Test]
        public void Test4()
        {
            var (captchaCounter, smsCounter) = ComputeTasksPerService(10, true, 10);

            captchaCounter.Should().Be(9);
            smsCounter.Should().Be(1);
        }

        [Test]
        public void Test5()
        {
            var (captchaCounter, smsCounter) = ComputeTasksPerService(10, true, 5);

            captchaCounter.Should().Be(4);
            smsCounter.Should().Be(6);
        }

        [Test]
        public void Test6()
        {
            var (captchaCounter, smsCounter) = ComputeTasksPerService(10, true, 15);

            captchaCounter.Should().Be(9);
            smsCounter.Should().Be(1);
        }

        private (int captchaCounter, int smsCounter) ComputeTasksPerService(int tasks, bool useHarvestCookies, int availableCookies)
        {
            int captchaCounter;
            int smsCounter;

            if (useHarvestCookies)
            {
                const double smsEnforcementRate = 0.1;

                var harvestTasks = availableCookies - tasks > 0
                    ? tasks
                    : availableCookies;

                captchaCounter = Convert.ToInt32(harvestTasks - harvestTasks * smsEnforcementRate);
                smsCounter = tasks - captchaCounter;
            }
            else
            {
                const double smsEnforcementRate = 0.8;

                captchaCounter = Convert.ToInt32(Math.Round(tasks - tasks * smsEnforcementRate));
                smsCounter = tasks - captchaCounter;
            }

            return (captchaCounter, smsCounter);
        }
    }
}
