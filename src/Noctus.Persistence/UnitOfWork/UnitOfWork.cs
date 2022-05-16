using System;
using System.Threading.Tasks;
using Noctus.Domain.Interfaces.UnitOfWork;
using Noctus.Persistence.Contexts;

namespace Noctus.Persistence.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private NoctusDbContext _context;
        public UnitOfWork(NoctusDbContext context)
        {
            _context = context;
        }

        public async Task Commit()
        {
            await _context.SaveChangesAsync();
        }
    }
}
