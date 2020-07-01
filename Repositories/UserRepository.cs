using InstaminiWebService.Database;
using InstaminiWebService.Models;
using InstaminiWebService.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InstaminiWebService.Repositories
{
    public class UserRepository : Repository<User>
    {
        private readonly DbSet<User> UserDatabase;
        public UserRepository(InstaminiContext context, IConfiguration configuration) : base(context, configuration)
        {
            UserDatabase = context.Users;
        }

        public override async Task DeleteAsync(User user)
        {
            UserDatabase.Remove(user);
            await DbContext.SaveChangesAsync();
        }

        public override Task<IEnumerable<User>> FindAllAsync()
        {
            throw new NotImplementedException();
        }

        public override Task<User> FindByIdAsync(int id)
        {
            return UserDatabase.Include(u => u.AvatarPhoto)
                            .Include(u => u.Posts).ThenInclude(p => p.Comments)
                            .Include(u => u.Posts).ThenInclude(p => p.Likes)
                            .Include(u => u.Posts).ThenInclude(p => p.Photos)
                            .Include(u => u.Followers).ThenInclude(u => u.Follower).ThenInclude(f => f.AvatarPhoto)
                            .Include(u => u.Followings).ThenInclude(u => u.User).ThenInclude(f => f.AvatarPhoto)
                            .FirstOrDefaultAsync(u => u.Id == id);
        }

        public override async Task<User> InsertAsync(User user)
        {
            var defaultAvatarPath = Configuration.GetValue<string>("DefaultAvatar");

            string originalPass = user.Password;
            string salt = PasswordUtils.GenerateSalt();
            string hashedPass = PasswordUtils.HashPasswordWithSalt(originalPass, salt);
            var now = DateTimeOffset.UtcNow;

            user.Username = user.Username.Trim();
            user.DisplayName = user.DisplayName.Trim();
            user.Password = hashedPass;
            user.Salt = salt;
            user.Created = now;
            user.LastUpdate = now;

            var transaction = DbContext.Database.BeginTransaction();

            await UserDatabase.AddAsync(user);
            await DbContext.SaveChangesAsync();

            // create new avatar record
            var photo = new AvatarPhoto
            {
                UserId = user.Id,
                FileName = defaultAvatarPath
            };
            DbContext.AvatarPhotos.Add(photo);
            await DbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            // format output
            user.AvatarPhoto = photo;
            return user;
        }

        public override async Task<User> UpdateAsync(User retrievedUser, User user)
        {
            if (!string.IsNullOrEmpty(user.Password))
            {
                string salt = PasswordUtils.GenerateSalt();
                string hashedPassword = PasswordUtils.HashPasswordWithSalt(user.Password, salt);
                user.Salt = salt;
                user.Password = hashedPassword;
            }
            else
            {
                user.Password = retrievedUser.Password;
                user.Salt = retrievedUser.Salt;
            }

            if (user.Created == DateTimeOffset.MinValue)
            {
                user.Created = retrievedUser.Created;
            }

            if (string.IsNullOrEmpty(user.Username))
            {
                user.Username = retrievedUser.Username;
            }
            if (string.IsNullOrEmpty(user.DisplayName))
            {
                user.DisplayName = retrievedUser.DisplayName;
            }

            var now = DateTime.UtcNow;
            user.LastUpdate = now;

            // After password update
            DbContext.Entry(retrievedUser).CurrentValues.SetValues(new
            {
                user.Username,
                user.Password,
                user.DisplayName,
                user.Salt
            });
            await DbContext.SaveChangesAsync();
            return retrievedUser;
        }

        public override Task<bool> Exists(User user)
        {
            return UserDatabase.AnyAsync(u => u.Username == user.Username);
        }

        public Task<User> FindByUsernameAsync(String username)
        {
            return UserDatabase.Include(u => u.AvatarPhoto)
                            .Include(u => u.Posts).ThenInclude(p => p.Comments)
                            .Include(u => u.Posts).ThenInclude(p => p.Likes)
                            .Include(u => u.Posts).ThenInclude(p => p.Photos)
                            .Include(u => u.Followers).ThenInclude(u => u.Follower).ThenInclude(f => f.AvatarPhoto)
                            .Include(u => u.Followings).ThenInclude(u => u.User).ThenInclude(f => f.AvatarPhoto)
                            .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<IEnumerable<User>> FindByQueryAsync(String query)
        {
            return await UserDatabase.Include(u => u.AvatarPhoto)
                                .Include(u => u.Followers)
                                    .ThenInclude(f => f.Follower)
                                        .ThenInclude(f => f.AvatarPhoto)
                                .Include(u => u.Followings)
                                    .ThenInclude(f => f.User)
                                        .ThenInclude(f => f.AvatarPhoto)
                                .Where(u => u.Username.Contains(query))
                                .ToListAsync();
        }

        public async Task<IEnumerable<string>> FindRelatedByUsernameAsync(string username)
        {
            return await UserDatabase.Include(u => u.Followings).ThenInclude(f => f.User)
                                    .Where(u => u.Username == username)
                                    .SelectMany(u => u.Followings)
                                    .Where(f => f.IsActive.Value)
                                    .Select(f => f.User.Username)
                                    .AsNoTracking()
                                    .ToListAsync();
        }
    }
}
