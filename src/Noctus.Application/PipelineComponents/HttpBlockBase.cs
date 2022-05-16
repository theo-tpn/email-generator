using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Noctus.Domain.Interfaces;
using Noctus.Domain.Interfaces.Pipeline;
using Noctus.Domain.Models;
using Noctus.Infrastructure;

namespace Noctus.Application.PipelineComponents
{
    public abstract class HttpBlockBase<TContext> : IBlock<TContext> where TContext : IHttpExecutionContext
    {
        protected abstract string Id { get; }
        protected abstract string Name { get; }

        public BlockStatus Status { get; protected set; } = BlockStatus.WAITING;

        public BlockExecution BlockExecutionLog { get; }
        
        public event EventHandler RaiseErrorEvent;

        protected HttpBlockBase()
        {
            BlockExecutionLog = new BlockExecution(Id, Name);
        }

        protected virtual void RaiseError()
        {
            RaiseErrorEvent?.Invoke(this, EventArgs.Empty);
        }

        protected abstract IEnumerable<IStep> Steps(TContext ctx);

        public async Task<Result> Execute(TContext ctx, CancellationToken cancellationToken)
        {
            Status = BlockStatus.PROCESSING;

            BlockExecutionLog.StartedAt = BlockExecutionLog.StartedAt == DateTime.MinValue
                ? DateTime.Now
                : BlockExecutionLog.StartedAt;

            using var client = CreateHttpClient(ctx.ProxyInfos, ctx.UserAgent, ctx.CookieContainer);

            foreach (var step in Steps(ctx))
            {
                var stepExecution = new StepExecution { StepName = step.Description };
                BlockExecutionLog.StepExecution.Add(stepExecution);

                var result = await step.Execute(client, ctx, cancellationToken).ConfigureAwait(false);
                stepExecution.TimeTakenMs = step.TimeTakenInMs;
                stepExecution.Result = result;

                if (!result.IsFailed) continue;

                Status = BlockStatus.FAILED;

                if (result.HasError<SmsServiceBalanceError>())
                {
                    BlockExecutionLog.EndedAt = DateTime.Now;
                    RaiseError();
                    return result;
                }

                BlockExecutionLog.RetryCount++;
                return result;
            }

            Status = BlockStatus.SUCCEEDED;
            BlockExecutionLog.EndedAt = DateTime.Now;

            return Result.Ok();
        }

        private static HttpClient CreateHttpClient(Proxy proxyInfo, string userAgent, CookieContainer cookieContainer)
        {
            HttpClientHandler handler;

            if (proxyInfo.UseLocalhost)
            {
                handler = new HttpClientHandler();
            }
            else
            {
                var proxy = new WebProxy
                {
                    Address = new Uri($"http://{proxyInfo.Ip}:{proxyInfo.Port}"),
                    BypassProxyOnLocal = false,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(userName: proxyInfo.Username, password: proxyInfo.Password)
                };

                handler = new HttpClientHandler
                {
                    Proxy = proxy,
                    UseCookies = true,
                    CookieContainer = cookieContainer
                };
            }

            var client = new HttpClient(handler);

            if (!string.IsNullOrEmpty(userAgent))
            {
                client.DefaultRequestHeaders.Add("User-Agent", userAgent);
            }

            return client;
        }
    }
}