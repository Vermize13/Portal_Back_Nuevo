using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Repository
{
    public interface IUnitOfWork
    {
        BugMgrDbContext Context { get; }
        Task<int> SaveChangesAsync();
    }

    public class UnitOfWork : IUnitOfWork
    {
        public BugMgrDbContext Context { get; }

        public UnitOfWork(BugMgrDbContext context)
        {
            Context = context;
        }

        public Task<int> SaveChangesAsync()
        {
            return Context.SaveChangesAsync();
        }
    }
}
