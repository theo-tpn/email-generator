using FluentAssertions;
using Noctus.Application.ExternalServices;
using Noctus.Domain;
using Noctus.Domain.Entities;
using Noctus.Domain.Models.Sms;
using NUnit.Framework;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Noctus.Domain.Models;

namespace Noctus.Tests
{
    class SmsServiceTests
    {
        private TestOptionsMonitor<UserSettings> _options;
        private SmsService _smsService;

        [SetUp]
        public void SetUp()
        {
            _options = new TestOptionsMonitor<UserSettings>(new UserSettings
            {
                ExternalServices = new ExternalServicesSettings
                {
                    SmsActivateRuApiKey = "b9529bA81068A25Ad6ec80bb5f6c2182",
                    SmsActivateRuRefCode = "1034269"
                }
            });

            _smsService = new SmsService(new HttpClient(), _options);
        }

        [Test]
        public async Task Should_use_updated_configuration()
        {
            // Given
            var options = new TestOptionsMonitor<UserSettings>(new UserSettings
            {
                ExternalServices = new ExternalServicesSettings
                {
                    SmsActivateRuApiKey = "",
                    SmsActivateRuRefCode = ""
                }
            });

            var smsService = new SmsService(new HttpClient(), options);

            // When

            var result = await smsService.GetBalance();
            result.Errors.First().Message.Should().Be("Failed to get balance");

            // Then
            options.Set(new UserSettings
            {
                ExternalServices = new ExternalServicesSettings
                {
                    SmsActivateRuApiKey = "b9529bA81068A25Ad6ec80bb5f6c2182",
                    SmsActivateRuRefCode = "1034269"
                }
            });

            result = await smsService.GetBalance();
            result.IsSuccess.Should().BeTrue();
        }

        [TestCase(ActivationStatus.ACTIVATE)]
        [TestCase(ActivationStatus.COMPLETE)]
        [TestCase(ActivationStatus.REPORT_AND_CANCEL)]
        [TestCase(ActivationStatus.REQUEST_OTHER_CODE)]
        public async Task ChangeActivationStatus_should_fail_when_operation_code_is_invalid(ActivationStatus status)
        {
            // Given

            // When
            var result = await _smsService.ChangeActivationStatus("12312312", status);

            // Then
            result.IsFailed.Should().BeTrue();
        }


        [Test]
        public async Task GetAvailablePhones_should_return_dictionary_with_requested_services()
        {
            // Given

            // When
            var result = await _smsService.GetAvailablePhones(SmsCountryCode.FR, new[] { SmsServiceType.Microsoft });

            // Then
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().ContainKey(SmsServiceType.Microsoft.Label);
        }

        [TestCase(SmsCountryCode.FR)]
        [TestCase(SmsCountryCode.UK)]
        public async Task GetPrices_by_country_and_service_should_return_valid_values(SmsCountryCode code)
        {
            // Given

            // When
            var result = await _smsService.GetPrices(code, SmsServiceType.Microsoft);

            // Then
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
        }

        [Test]
        public async Task GetPrices_by_service_should_return_valid_values()
        {
            // Given

            // When
            var result = await _smsService.GetPrices(SmsServiceType.Microsoft);

            // Then
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();

            result.Value.Should().ContainKeys(SmsCountryCode.FR, SmsCountryCode.UK);
        }
    }
}
