using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using FluentResults;
using Microsoft.AspNetCore.Components.Forms;
using Noctus.Domain;
using Noctus.Domain.Models;

namespace Noctus.GenWave.Desktop.App.Services
{
    public class CsvExportService
    {
        public async Task<Result<string>> ExportAccounts(string fileName, IEnumerable<Account> accounts)
        {
            try
            {
                var list = new List<CsvAccountRecord>();
                foreach (var account in accounts)
                {
                    var record = new CsvAccountRecord(account.Username, account.FirstName, account.LastName, account.Password, account.MasterForward,
                        account.RecoveryCode, account.RecoveryEmail);
                    list.Add(record);
                    var aliasRecords =
                        account.Aliases.Select(s => record with {Username = s, MainMail = record.Username});
                    list.AddRange(aliasRecords);
                }

                fileName ??= DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture);
                var path = @$"{ResourcesHelper.DefaultAccountsFolderPath}/{fileName}.csv";
                var fileExists = File.Exists(path);

                await using var streamWriter = new StreamWriter(path, fileExists);
                await using var csvWriter = new CsvWriter(streamWriter,
                    new CsvConfiguration(CultureInfo.InvariantCulture) {HasHeaderRecord = false});

                csvWriter.Context.RegisterClassMap<CsvAccountRecordMap>();

                csvWriter.WriteHeader<CsvAccountRecord>();
                await csvWriter.NextRecordAsync();

                await csvWriter.WriteRecordsAsync(list);
                await csvWriter.FlushAsync();
                await streamWriter.FlushAsync();

                return Result.Ok(path);
            }
            catch (Exception e)
            {
                return Result.Fail(new ExceptionalError(e));
            }
        }

        private static readonly HashSet<string> AllowedExtensions = new() {"csv", "xls", "xlsx"};

        public async Task<Result<(List<string> headers, List<dynamic> records)>> ImportAccounts(IBrowserFile browserFile)
        {
            var regex = new Regex("\\.([^\\.]+)$");
            var match = regex.Match(browserFile.Name);
            var extension = match.Groups[1].Value;

            if (!AllowedExtensions.Contains(extension))
                return Result.Fail($"File extension {extension} not supported");

            try
            {
                await using var stream = browserFile.OpenReadStream();
                using var streamReader = new StreamReader(stream);
                using var reader = new CsvReader(streamReader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    IgnoreBlankLines = true,
                    HasHeaderRecord = true
                });

                await reader.ReadAsync();
                reader.ReadHeader();
                var headers = reader.HeaderRecord.ToList();
                var records = await reader.GetRecordsAsync<dynamic>().ToListAsync();

                if (!records.Any())
                    return Result.Fail("File does not contain any records, please select another file");

                return Result.Ok((headers, records));
            }
            catch (IOException)
            {
                return Result.Fail("File is too big. Max allowed size is 500kb");
            }
            catch (Exception _)
            {
                return Result.Fail("Ensure that your file contains CSV headers");
            }
        }
    }

    public record CsvAccountRecord(string Username, string firstName, string lastName, /*int birthday, int birthmonth,
            int birthyear, string countryCode, string phoneCountryCode,*/ string Password, string MasterForward,
        string RecoveryCode, string RecoveryEmail, string MainMail = "");
    //{
    //    public string Username { get; }
    //    //public string FirstName { get; }
    //    //public string LastName { get; }
    //    //public int Birthday { get; }
    //    //public int Birthmonth { get; }
    //    //public int Birthyear { get; }
    //    //public string CountryCode { get; }
    //    //public string PhoneCountryCode { get; }
    //    public string Password { get; }
    //    public string MasterForward { get; }
    //    public string RecoveryCode { get; }
    //    public string RecoveryEmail { get; }
    //    public string MainMail { get; }

    //    public CsvAccountRecord(string username, /*string firstName, string lastName, int birthday, int birthmonth,
    //        int birthyear, string countryCode, string phoneCountryCode,*/ string password, string masterForward,
    //        string recoveryCode, string recoveryEmail, string mainMail)
    //    {
    //        Username = username;
    //        //FirstName = firstName;
    //        //LastName = lastName;
    //        //Birthday = birthday;
    //        //Birthmonth = birthmonth;
    //        //Birthyear = birthyear;
    //        //CountryCode = countryCode;
    //        //PhoneCountryCode = phoneCountryCode;
    //        Password = password;
    //        MasterForward = masterForward;
    //        RecoveryCode = recoveryCode;
    //        RecoveryEmail = recoveryEmail;
    //        MainMail = mainMail;
    //    }
    //}

    public sealed class CsvAccountRecordMap : ClassMap<CsvAccountRecord>
    {
        public CsvAccountRecordMap()
        {
            Map(m => m.Username).Name("Username");
            Map(m => m.Password).Name("Password");
            Map(m => m.RecoveryCode).Name("RecoveryCode");
            Map(m => m.RecoveryEmail).Name("RecoveryEmail").Optional();
            Map(m => m.MasterForward).Name("MasterForward").Optional();
            Map(m => m.MainMail).Name("MainMail").Optional();
            //Map(m => m.CountryCode).Name("AccountCountryCode").Optional();
            //Map(m => m.PhoneCountryCode).Name("PhoneCountryCode").Optional();
            Map(m => m.firstName).Name("FirstName");
            Map(m => m.lastName).Name("LastName");
            //Map(m => m.Birthday).Name("Birthday");
            //Map(m => m.Birthmonth).Name("Birthmonth");
            //Map(m => m.Birthyear).Name("Birthyear");
        }
    }
}
