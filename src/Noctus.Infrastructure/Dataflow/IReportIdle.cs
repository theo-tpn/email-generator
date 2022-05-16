using System.Text;

namespace Noctus.Infrastructure.Dataflow
{
    public interface IReportIdle
    {
        bool IsIdle { get; }
    }
}
