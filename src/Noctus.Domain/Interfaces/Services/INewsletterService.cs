using System.Threading.Tasks;
using FluentResults;

namespace Noctus.Domain.Interfaces.Services
{
    public interface INewsletterService
    {
        Task<Result> SubscribeToAll(string email);
    }
}