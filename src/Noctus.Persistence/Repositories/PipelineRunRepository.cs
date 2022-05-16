using Noctus.Domain.Entities;
using Noctus.Domain.Interfaces.Repositories;
using Noctus.Persistence.Contexts;

namespace Noctus.Persistence.Repositories
{
    public class PipelineRunRepository : BaseRepository<PipelineRun>, IPipelineRunRepository
    {
        public PipelineRunRepository(NoctusDbContext context) : base(context)
        {
        }
    }
}
