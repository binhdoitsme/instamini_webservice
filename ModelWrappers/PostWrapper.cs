using InstaminiWebService.Models;
using InstaminiWebService.ModelWrappers.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InstaminiWebService.ModelWrappers
{
    public class PostWrapper : IModelWrapper<Post>
    {
        public int Id { get; private set; }
        public string Caption { get; private set; }
        public string Username { get; private set; }
        public string UserAvatar { get; private set; }
        public string UserLink { get; private set; }
        public DateTimeOffset Created { get; private set; }
        public int CommentCount { get; private set; }
        public int LikeCount { get; private set; }
        public IEnumerable<CommentWrapper> Comments { get; private set; }
        public IEnumerable<string> LikedBy { get; private set; }
        public IEnumerable<PhotoWrapper> Photos { get; private set; }
        public string Link { get; private set; }

        public PostWrapper(Post post)
        {
            Id = post.Id;
            Caption = post.Caption;
            Username = post.User.Username;
            Link = $"/posts/{post.Id}";
            UserAvatar = $"/avatars/{post.User.AvatarPhoto.FileName}";
            UserLink = $"/users/{post.UserId}";
            Created = post.Created.Value;
            Comments = post.Comments.Select(c => new CommentWrapper(c));
            CommentCount = Comments.Count();
            LikedBy = post.Likes.Select(l => l.User.Username);
            LikeCount = LikedBy.Count();
            Photos = post.Photos.Select(p => new PhotoWrapper(p));
        }
    }
}
