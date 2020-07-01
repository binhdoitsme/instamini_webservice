using InstaminiWebService.Database;
using Microsoft.Extensions.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InstaminiWebService.Repositories
{
    public abstract class Repository<T> where T : class
    {
        protected readonly InstaminiContext DbContext;
        protected readonly IConfiguration Configuration;

        public Repository(InstaminiContext dbContext, IConfiguration configuration)
        {
            DbContext = dbContext;
            Configuration = configuration;
        }

        public abstract Task<T> FindByIdAsync(int id);
        public abstract Task<IEnumerable<T>> FindAllAsync();
        public abstract Task<T> InsertAsync(T item);
        public abstract Task<T> UpdateAsync(T oldItem, T newItem);
        public abstract Task DeleteAsync(T item);
        public abstract Task<bool> Exists(T item);
    }
}
