using System.Collections.Generic;
using System.Threading.Tasks;
using InstaminiWebService.Database;
using InstaminiWebService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace InstaminiWebService.Repositories
{
    public class CommentRepository : Repository<Comment>
    {
        private readonly DbSet<Comment> CommentDatabase;
        public CommentRepository(InstaminiContext dbContext, IConfiguration configuration) : base(dbContext, configuration)
        {
            CommentDatabase = dbContext.Comments;
        }

        public override Task DeleteAsync(Comment item)
        {
            DbContext.Remove(item);
            return DbContext.SaveChangesAsync();
        }

        public override Task<bool> Exists(Comment item)
        {
            throw new System.NotImplementedException();
        }

        public override Task<IEnumerable<Comment>> FindAllAsync()
        {
            throw new System.NotImplementedException();
        }

        public override Task<Comment> FindByIdAsync(int id)
        {
            return CommentDatabase.Include(c => c.User)
                                .ThenInclude(u => u.AvatarPhoto)
                                .AsTracking()
                                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public override Task<Comment> InsertAsync(Comment item)
        {
            throw new System.NotImplementedException();
        }

        public override async Task<Comment> UpdateAsync(Comment oldItem, Comment newItem)
        {
            DbContext.Entry(oldItem).CurrentValues.SetValues(new { newItem.Content });
            await DbContext.SaveChangesAsync();
            return oldItem;
        }
    }
}