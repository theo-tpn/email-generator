using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CsvHelper.Configuration;
using FluentValidation;
using Newtonsoft.Json;
using Noctus.Domain.Models;
using NUnit.Framework;

namespace Noctus.Tests
{
    public class AddAccountTests
    {
        [Test]
        public async Task Try_read_accounts()
        {
            // Given
            var values = "Username,Password,RecoveryCode,RecoveryEmail,MasterForward\r\nmehmedcattelain1932@outlook.com,VCjH0ShW21,L2G2J-RF6VY-L6EBG-JSGEH-D3DGV,,jeankekwzor@outlook.com,\r\ncorettaVAN-CANNEYT253@outlook.com,VCjH0ShW21,L2G2J-RF6VY-L6EBG-JSGEH-D3DGV,,jeankekwzor@outlook.com,mehmedcattelain1932@outlook.com\r\nholmiRORTAIS10@outlook.com,VCjH0ShW21,L2G2J-RF6VY-L6EBG-JSGEH-D3DGV,,jeankekwzor@outlook.com,mehmedcattelain1932@outlook.com";

            var sr = new StringReader(values.Trim());
            
            var reader = new CsvHelper.CsvReader(sr, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                IgnoreBlankLines = true,
                MissingFieldFound = null
            });

            var validator = new AccountValidator();
            
            await foreach (var record in reader.GetRecordsAsync<dynamic>())
            {
                var a = ((IDictionary<string, Object>)record)["Username"];

                var result = await validator.ValidateAsync(record);
            }

            // When

            // Then

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
