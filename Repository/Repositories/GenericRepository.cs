using Infrastructure;
using Domain;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Repository.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetAsync(object id);
        Task<IEnumerable<T>> GetAllAsync();
        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);
    }

    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly BugMgrDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(BugMgrDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public Task<T?> GetAsync(object id)
        {
            return _dbSet.FindAsync(id).AsTask();
        }

        public Task<IEnumerable<T>> GetAllAsync()
        {
            return Task.FromResult(_dbSet.AsEnumerable());
        }

        public Task AddAsync(T entity)
        {
            _dbSet.Add(entity);
            return Task.CompletedTask;
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }
    }
}
