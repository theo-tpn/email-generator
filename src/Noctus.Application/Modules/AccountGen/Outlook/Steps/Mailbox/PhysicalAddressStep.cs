using Fare;
using FluentResults;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Noctus.Application.PipelineComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Noctus.Infrastructure;

namespace Noctus.Application.Modules.AccountGen.Outlook.Steps.Mailbox
{
    public class PhysicalAddressStep : StepBase<AccountGenExecutionContext>
    {
        public override string Description => "Adding random physical address";

        protected override async Task<Result> ExecuteInner(HttpClient client, AccountGenExecutionContext ctx, CancellationToken cancellationToken)
        {
            var request = await client.GetAsync("https://account.microsoft.com/billing/addresses#/", cancellationToken);
            var requestContent = await request.Content.ReadAsStringAsync(cancellationToken);

            var result = await RequestHelper.PostGeneratedForm(client, requestContent, cancellationToken);

            if (result.IsFailed)
                return result;

            // Get sur la page pour aller chercher le token
            var request2 = await client.GetAsync("https://account.microsoft.com/billing/addresses#/", cancellationToken);
            var requestContent2 = await request2.Content.ReadAsStringAsync(cancellationToken);

            //On vas cherhcer le RequestVerificationToken (input hidden)
            var htmlDocumentParser = new HtmlDocument();
            htmlDocumentParser.LoadHtml(requestContent2);

            var nodes = htmlDocumentParser.DocumentNode
                .SelectNodes("//input[@type='hidden']");

            var formValues =
                (from n in nodes
                 let id = n.GetAttributeValue("name", string.Empty)
                 let value = n.GetAttributeValue("value", string.Empty)
                 select new KeyValuePair<string, string>(id, value)).ToList();

            var token = formValues.SingleOrDefault(x => string.Equals("__RequestVerificationToken", x.Key)).Value;
            if (string.IsNullOrEmpty(token))
                return Result.Fail("Cannot find RequestVerificationToken");


            //GET des states du country (MS construit ses states à sa sauce)
            var msStatesRequest = await client.GetAsync(new Uri($"https://account.microsoft.com/api/country/states?X-Requested-With=XMLHttpRequest&isoCode={ctx.Profile.CountryCode}"), cancellationToken);
            var msStatesList =
                JsonConvert.DeserializeObject<List<MsCountryStates>>(await msStatesRequest.Content.ReadAsStringAsync(cancellationToken));

            //On choisi un state random
            var random = new Random();
            var randomedState = msStatesList[random.Next(0, msStatesList.Count)];

            //On crée une adresse random
            var address = new RandomAddress(CountryCodeSwapper(ctx.Profile.CountryCode), randomedState);

            //POST de l'addresse
            var httpAddressRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://account.microsoft.com/billing/api/shipping/create?X-Requested-With=XMLHttpRequest"),
                Content = new StringContent(BuildBody(ctx, address), Encoding.UTF8, "application/json"),
                Headers =
                {
                    {"__RequestVerificationToken", token}
                }
            };

            await client.SendAsync(httpAddressRequest, cancellationToken);
            return Result.Ok();
        }

        private static string BuildBody(AccountGenExecutionContext ctx, RandomAddress address) =>
            $"{{\"address\":" +
            $"{{\"address\":" +
            $"{{\"countryId\":\"{address.Country}\"," +
            $"\"stateId\":\"{address.State}\"," +
            $"\"line1\":\"{address.BuildingNumber} {address.Street}\"," +
            $"\"zip\":\"{address.Zip}\"," +
            $"\"city\":\"{address.City}\"," +
            $"\"isValid\":true}}," +
            $"\"isAddressRemovalAllowed\":true," +
            $"\"isCountryChangeAllowed\":true," +
            $"\"warnAboutDefaultChange\":false," +
            $"\"isDefaultBillingAddress\":true," +
            $"\"isDefaultShippingAddress\":true," +
            $"\"isDefault\":true," +
            $"\"firstName\":\"{ctx.Profile.FirstName}\"," +
            $"\"lastName\":\"{ctx.Profile.LastName}\"," +
            $"\"errorMessage\":\"\"," +
            $"\"phoneNumber\":\"{ctx.HotValues[OutlookConstants.Keys.PhoneNumber] ?? ""}\"}}}}";

        //Obligé de swap les country code pour les addresses sur certains pays 
        private static string CountryCodeSwapper(string countryCode)
        {
            return countryCode switch
            {
                "UK" => "GB",
                _ => countryCode
            };
        }

        internal class MsCountryStates
        {
            [JsonPropertyName("iso")]
            public string Iso { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }
        }

        internal class RandomAddress
        {
            public string Country { get; set; }
            public string Street { get; set; }
            public string BuildingNumber { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string Zip { get; set; }

            public RandomAddress(string countryCode, MsCountryStates countryState)
            {
                Bogus.Faker faker = new Bogus.Faker();

                Street = faker.Address.StreetName();
                BuildingNumber = faker.Address.BuildingNumber();
                City = faker.Address.City();
                
                Country = countryCode;
                State = countryState.Iso;
                Zip = new Xeger(AddressHelper.CountryZipCodePatterns[countryCode]).Generate();
            }
        }
    }
}
