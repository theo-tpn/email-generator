using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;

namespace Noctus.Domain.Interfaces.Pipeline
{
    public interface IStep
    {
        string Description { get; }
        long TimeTakenInMs { get; }
        Task<Result> Execute(HttpClient client, IExecutionContext ctx, CancellationToken cancellationToken);
    }
}