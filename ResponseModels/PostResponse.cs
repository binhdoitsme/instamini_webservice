using InstaminiWebService.Models;
using InstaminiWebService.ResponseModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InstaminiWebService.ResponseModels
{
    public class PostResponse : IResponseModel<Post>
    {
        public int Id { get; private set; }
        public string Caption { get; private set; }
        public string Username { get; private set; }
        public string UserAvatar { get; private set; }
        public string UserLink { get; private set; }
        public DateTimeOffset Created { get; private set; }
        public int CommentCount { get; private set; }
        public int LikeCount { get; private set; }
        public IEnumerable<CommentResponse> Comments { get; private set; }
        public IEnumerable<LikeResponse> LikedBy { get; private set; }
        public IEnumerable<PhotoResponse> Photos { get; private set; }
        public string Link { get; private set; }

        public PostResponse(Post post)
        {
            Id = post.Id;
            Caption = post.Caption;
            Username = post.User.Username;
            Link = $"/posts/{post.Id}";
            UserAvatar = $"/avatars/{post.User.AvatarPhoto.FileName}";
            UserLink = $"/users/{post.User.Username}";
            Created = post.Created.Value;
            Comments = post.Comments.Select(c => new CommentResponse(c));
            CommentCount = Comments.Count();
            LikedBy = post.Likes.Where(l => l.IsActive.Value).Select(l => new LikeResponse(l));
            LikeCount = LikedBy.Count();
            Photos = post.Photos.Select(p => new PhotoResponse(p));
        }
    }
}
