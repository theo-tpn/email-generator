using FluentAssertions;
using Noctus.Application.ExternalServices;
using Noctus.Domain;
using Noctus.Domain.Entities;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using Noctus.Domain.Models;

namespace Noctus.Tests
{
    public class CaptchaServiceTests
    {
        [Test]
        public async Task Should_use_updated_configuration()
        {
            // Given
            var options = new TestOptionsMonitor<UserSettings>(new UserSettings
            {
                ExternalServices = new ExternalServicesSettings
                {
                    TwoCaptchaApiKey = ""
                }
            });

            var captchaService = new CaptchaService(options);

            // When

            var result = await captchaService.GetBalance();
            result.Errors.First().Message.Should().Be("ERROR_WRONG_USER_KEY");

            // Then
            options.Set(new UserSettings
            {
                ExternalServices = new ExternalServicesSettings
                {
                    TwoCaptchaApiKey = "8b4ffdcc073d7b36d265e6016980c50a"
                }
            });

            result = await captchaService.GetBalance();
            result.IsSuccess.Should().BeTrue();
        }
    }
}
