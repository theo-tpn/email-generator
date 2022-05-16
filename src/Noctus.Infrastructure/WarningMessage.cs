using FluentResults;

namespace Noctus.Infrastructure
{
    public class WarningMessage : Success
    {
        public WarningMessage(string message) : base(message)
        {
        }
    }
}