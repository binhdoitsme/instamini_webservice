using InstaminiWebService.Models;
using InstaminiWebService.ResponseModels.Base;
using System;

namespace InstaminiWebService.ResponseModels
{
    public class CommentResponse : IResponseModel<Comment>
    {
        public int Id { get; private set; }
        public string Content { get; private set; }
        public DateTimeOffset Timestamp { get; private set; }
        public string Username { get; private set; }
        public string UserLink { get; private set; }
        public string UserAvatar { get; private set; }
        public string Link { get; private set; }

        public CommentResponse(Comment comment)
        {
            Id = comment.Id;
            Content = comment.Content;
            Timestamp = comment.Timestamp;
            var user = comment.User;
            Username = user.Username;
            UserLink = $"/users/{user.Username}";
            UserAvatar = $"/avatars/{user.AvatarPhoto.FileName}";
            Link = $"/comments/{Id}";
        }
    }
}