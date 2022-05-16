using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using FluentResults;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;
using NUnit.Framework;

namespace Noctus.Tests
{
    class NewsletterSubscriptionTests
    {
        [Test]
        public async Task METHOD()
        {
            // Given
            //string email = "jeankekwzor@outlook.com";
            //var client = new NewsletterHttpClient(new HttpClient());

            //// When
            
            //var result = await client.SubscribeToAll(email);

            //// Then

            //var block = new TransformBlock<Tuple<string, string>, Result>(async t =>
            //{
            //    var (mail, password) = t;

            //    try
            //    {
            //        using var imapClient = new ImapClient();
            //        await imapClient.ConnectAsync("outlook.office365.com", 993, true);

            //        await imapClient.AuthenticateAsync(mail, password);
                    
            //        var inbox = imapClient.Inbox;
            //        await inbox.OpenAsync(FolderAccess.ReadWrite);

            //        var query = SearchQuery.NotSeen
            //            .And(SearchQuery.DeliveredAfter(DateTime.Now.AddDays(-5)));

            //        var search = await inbox.SearchAsync(query);

            //        foreach (var uid in search)
            //        {
            //            await inbox.AddFlagsAsync(uid, MessageFlags.Seen, true);
            //        }

            //        await imapClient.DisconnectAsync(true);
            //        return Result.Ok(mail);
            //    }
            //    catch (Exception e)
            //    {
            //        return Result.Fail(new Error("Failed").CausedBy(e));
            //    }
            //}, new ExecutionDataflowBlockOptions
            //{
            //    MaxDegreeOfParallelism = 10
            //});
            

        }
    }
}
