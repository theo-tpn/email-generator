using Noctus.Domain.Entities;
using Noctus.Domain.Interfaces.Repositories;
using Noctus.Persistence.Contexts;

namespace Noctus.Persistence.Repositories
{
    public class PipelineEventRepository : BaseRepository<PipelineEvent>, IPipelineEventRepository
    {
        public PipelineEventRepository(NoctusDbContext context) : base(context)
        {
        }
    }
}
