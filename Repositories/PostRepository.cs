using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InstaminiWebService.Database;
using InstaminiWebService.Models;
using InstaminiWebService.ResponseModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace InstaminiWebService.Repositories
{
    public class PostRepository : Repository<Post>
    {
        private readonly DbSet<Post> PostDatabase;

        public PostRepository(InstaminiContext context, IConfiguration configuration) : base(context, configuration)
        {
            PostDatabase = context.Posts;
        }

        public override Task DeleteAsync(Post item)
        {
            DbContext.Remove(item);
            return DbContext.SaveChangesAsync();
        }

        public override Task<bool> Exists(Post item)
        {
            throw new System.NotImplementedException();
        }

        public override Task<IEnumerable<Post>> FindAllAsync()
        {
            throw new System.NotImplementedException();
        }

        public override Task<Post> FindByIdAsync(int id)
        {
            return PostDatabase.Include(p => p.User)
                                    .ThenInclude(u => u.AvatarPhoto)
                                .Include(p => p.Likes)
                                    .ThenInclude(l => l.User)
                                        .ThenInclude(u => u.AvatarPhoto)
                                .Include(p => p.Photos)
                                .Include(p => p.Comments)
                                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public override Task<Post> InsertAsync(Post item)
        {
            throw new System.NotImplementedException();
        }

        public override async Task<Post> UpdateAsync(Post oldItem, Post newItem)
        {
            DbContext.Entry(oldItem).CurrentValues.SetValues(new { newItem.Caption });
            await DbContext.SaveChangesAsync();
            return oldItem;
        }

        public async Task<IEnumerable<Post>> FindByUserAsync(string username)
        {
            return await PostDatabase.Include(p => p.Photos)
                        .Include(p => p.Likes).ThenInclude(l => l.User).ThenInclude(u => u.AvatarPhoto)
                        .Include(p => p.User).ThenInclude(u => u.AvatarPhoto)
                        .Include(p => p.Comments).ThenInclude(c => c.User).ThenInclude(u => u.AvatarPhoto)
                        .Where(p => p.User.Username == username)
                        .OrderByDescending(p => p.Created)
                        .AsNoTracking()
                        .ToListAsync();
        }

        public async Task<IEnumerable<Post>> FindRelatedPostsAsync(string username, IEnumerable<string> relatedUsers) {
            return await DbContext.Posts
                                .Include(p => p.Photos)
                                .Include(p => p.Comments)
                                    .ThenInclude(c => c.User)
                                        .ThenInclude(u => u.AvatarPhoto)
                                .Include(p => p.Likes)
                                    .ThenInclude(l => l.User)
                                        .ThenInclude(u => u.AvatarPhoto)
                                .Include(p => p.User)
                                    .ThenInclude(u => u.Followings)
                                .Include(p => p.User)
                                    .ThenInclude(u => u.AvatarPhoto)
                                .Where(p => p.User.Username == username || relatedUsers.Contains(p.User.Username))
                                .OrderByDescending(p => p.Created)
                                .AsNoTracking()
                                .ToListAsync();
        }
    }
}