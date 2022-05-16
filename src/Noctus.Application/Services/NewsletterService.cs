using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentResults;
using Noctus.Domain.Interfaces.Services;

namespace Noctus.Application.Services
{
    public class NewsletterService : INewsletterService
    {
        private readonly HttpClient _client;

        public NewsletterService(HttpClient client)
        {
            _client = client;
        }

        public async Task<Result> SubscribeToAll(string email)
        {
            var errors = new List<Result>();

            foreach (var kvp in Dictionary)
            {
                var result = await Send(kvp.Value.Invoke(email)).ConfigureAwait(false);

                if (result.IsFailed)
                    errors.Add(Result.Fail(kvp.Key).WithErrors(result.Errors));
            }

            return errors.Any()
                ? Result.Merge(errors.ToArray())
                : Result.Ok();
        }

        private async Task<Result> Send(HttpRequestMessage message)
        {
            try
            {
                var response = await _client.SendAsync(message);

                return response.IsSuccessStatusCode 
                    ? Result.Ok() 
                    : Result.Fail(response.ReasonPhrase);
            }
            catch (Exception e)
            {
                return Result.Fail(new ExceptionalError(e));
            }
        }

        private Dictionary<string, Func<string, HttpRequestMessage>> Dictionary =
            new Dictionary<string, Func<string, HttpRequestMessage>>
            {
                {"www.declikdeco.com", DeclikDecoNewsletters},
                {"www.leslipfrancais.fr", SlipFrancais},
                {"www.theatlantic.com", TheAtlantic},
                {"www.francetelevisions.fr", FranceTv},
                {"www.courrierinternational.com", CourrierInternational},
                {"www.voltex.fr", Voltex},
                {"www.lespetitsraffineurs.com", LesPetitsRaffineurs},
                {"www.zalando.fr", Zalando},
                {"www.qz.com", Quartz}
            };

        private static HttpRequestMessage DeclikDecoNewsletters(string email)
        {
            var formData = new MultipartFormDataContent {{new StringContent(email), "email"}};

            return new HttpRequestMessage
            {
                RequestUri = new Uri("https://www.declikdeco.com/newsletter/register"),
                Content = formData,
                Method = HttpMethod.Post
            };
        }

        private static HttpRequestMessage SlipFrancais(string email)
        {
            var formData = new MultipartFormDataContent
            {
                {new StringContent(email), "email"},
                {new StringContent("blocknewsletter"), "module"},
                {new StringContent("module"), "fc"},
                {new StringContent("verification"), "controller"},
                {new StringContent("1"), "ajax"},
                {new StringContent("footer"), "origin"},
                {new StringContent("0"), "action"}
            };

            return new HttpRequestMessage
            {
                RequestUri = new Uri("https://www.leslipfrancais.fr/"),
                Content = formData,
                Method = HttpMethod.Post
            };
        }

        private static HttpRequestMessage TheAtlantic(string email)
        {
            var formData = new MultipartFormDataContent
            {
                {new StringContent(email), "email"}, 
                {new StringContent("7581216"), "newsletters"}
            };

            return new HttpRequestMessage
            {
                RequestUri = new Uri("https://www.theatlantic.com/newsletters/api/sign-up/"),
                Content = formData,
                Method = HttpMethod.Post
            };
        }

        private static HttpRequestMessage FranceTv(string email)
        {
            var formData = new MultipartFormDataContent
            {
                {new StringContent(email), "email"},
                {new StringContent("optin"), "ftvOptInfFtv7H30,ftvOptInfFtvAlr,ftvOptInfFtvVideo,ftvOptInFtvBoSem,ftvOptInFtvJt20h,ftvOptOnVousRepond,ftvOpt1OutMer,ftvOpt1HebNouCal,ftvOptSpoQuo,ftvAlertSport,ftvOptCulQuo,ftvOptAlloDocQuo,ftvOptBestOfFtv,ftvOptIntegrale,ftvOptOkoo,ftvMetaMedia,ftvOptPBLV,ftvOptClu,ftvOptCulturePrime,ftvOptUnSiGrandSoleil,ftvHebCulFtv,ftvOptFtv"}
            };

            return new HttpRequestMessage
            {
                RequestUri = new Uri("https://www.francetelevisions.fr/abonnements/php/abonnement/abonnement.php"),
                Content = formData,
                Method = HttpMethod.Post
            };
        }

        private static HttpRequestMessage CourrierInternational(string email)
        {
            var formData = new MultipartFormDataContent
            {
                {new StringContent(email), "email"},
                {new StringContent("reveil"), "1"},
                {new StringContent("daily"), "1"},
                {new StringContent("magazine"), "1"},
                {new StringContent("cartoons"), "1"},
                {new StringContent("best"), "1"},
                {new StringContent("subscribers_expat"), "1"},
                {new StringContent("expatpratique"), "1"},
                {new StringContent("educ"), "1"},
                {new StringContent("exclusive_ci"), "1"},
                {new StringContent("exclusive_partners"), "1"},
                {new StringContent("subscribe"), "Je+m'inscris"},
                {new StringContent("form_build_id"), "form-vTBC31sQ6lZr_6Zd4-SfNLWgDQkR3GAQU3nsyV5S8UI"},
                {new StringContent("form_id"), "ci_newsletters_subscribe_form"}
            };

            return new HttpRequestMessage
            {
                RequestUri = new Uri("https://www.courrierinternational.com/page/newsletters"),
                Content = formData,
                Method = HttpMethod.Post
            };
        }

        private static HttpRequestMessage Voltex(string email)
        {
            var formData = new MultipartFormDataContent
            {
                {new StringContent(email), "email"}
            };

            return new HttpRequestMessage
            {
                RequestUri = new Uri("https://www.voltex.fr/newsletter/subscriber/new/"),
                Content = formData,
                Method = HttpMethod.Post
            };
        }

        private static HttpRequestMessage LesPetitsRaffineurs(string email)
        {
            var formData = new MultipartFormDataContent
            {
                {new StringContent(email), "email"},
                {new StringContent("submitNewsletter"), "Je+m'abonne"},
                {new StringContent("blockHookName"), "displayHome"},
                {new StringContent("action"), "0"}
            };

            return new HttpRequestMessage
            {
                RequestUri = new Uri("https://www.lespetitsraffineurs.com/"),
                Content = formData,
                Method = HttpMethod.Post
            };
        }

        private static HttpRequestMessage Zalando(string email)
        {
            return new HttpRequestMessage
            {
                RequestUri = new Uri("https://www.zalando.fr/api/graphql/"),
                Content = new StringContent($"[{{\"id\":\"f321f59294a4ffd369951dc5d8f92b801cb7c3c7302de9e5118b3569416c844f\",\"variables\":{{\"input\":{{\"email\":\"{email}\",\"preference\":{{\"category\":\"MEN\",\"topics\":[{{\"id\":\"item_alerts\",\"isEnabled\":true}},{{\"id\":\"survey\",\"isEnabled\":true}},{{\"id\":\"recommendations\",\"isEnabled\":true}},{{\"id\":\"fashion_fix\",\"isEnabled\":true}},{{\"id\":\"follow_brand\",\"isEnabled\":true}},{{\"id\":\"subscription_confirmations\",\"isEnabled\":true}},{{\"id\":\"offers_sales\",\"isEnabled\":true}}]}},\"referrer\":\"nl_subscription_page\",\"clientMutationId\":\"1615029830151\"}}}}}}]"),
                Method = HttpMethod.Post
            };
        }

        private static HttpRequestMessage Quartz(string email)
        {
            return new HttpRequestMessage
            {
                RequestUri = new Uri("https://www.sportstrategies.com/wp-admin/admin-ajax.php"),
                Method = HttpMethod.Post,
                Content = new StringContent(
                    $"{{\"email\":\"{email}\",\"list_names\":[\"daily-brief\"],\"custom_fields\":{{}}}}", Encoding.UTF8,
                    "application/json")
            };
        }
    }
}
