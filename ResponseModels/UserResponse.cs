using InstaminiWebService.Models;
using InstaminiWebService.ResponseModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InstaminiWebService.ResponseModels
{
    public class UserResponse : IResponseModel<User>
    {
        public int Id { get; private set; }
        public string Username { get; private set; }
        public string DisplayName { get; private set; }
        public string AvatarLink { get; private set; }
        public DateTimeOffset Created { get; private set; }
        public int FollowerCount { get; private set; }
        public int FollowingCount { get; private set; }
        public IEnumerable<object> Followers { get; private set; }
        public IEnumerable<object> Followings { get; private set; }
        public IEnumerable<object> Posts { get; private set; }
        public string Link { get; private set; }

        public UserResponse(User user)
        {
            Id = user.Id;
            Username = user.Username;
            Link = $"/users/{Username}";
            DisplayName = user.DisplayName;
            AvatarLink = $"/avatars/{user.AvatarPhoto.FileName}";
            Created = user.Created;
            Followers = user.Followers.Where(f => f.IsActive.Value).Select(f => new
            {
                f.FollowerId,
                f.Follower.Username,
                f.Follower.DisplayName,
                AvatarLink = $"/avatars/{f.Follower.AvatarPhoto.FileName}"
            });
            Followings = user.Followings.Where(f => f.IsActive.Value).Select(f => new
            {
                f.UserId,
                f.User.Username,
                f.User.DisplayName,
                AvatarLink = $"/avatars/{f.User.AvatarPhoto.FileName}"
            });
            FollowerCount = Followers.Count();
            FollowingCount = Followings.Count();
            Posts = user.Posts.OrderByDescending(p => p.Created).Select(p => new
            {
                p.Id,
                Link = $"/posts/{p.Id}",
                LikeCount = p.Likes.Count(l => l.IsActive.Value),
                CommentCount = p.Comments.Count,
                Thumbnail = $"/photos/{p.Photos.FirstOrDefault().FileName}"
            });
            
        }
    }
}
