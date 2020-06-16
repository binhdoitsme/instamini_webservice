using InstaminiWebService.Models;
using InstaminiWebService.ModelWrappers.Base;
using System;

namespace InstaminiWebService.ModelWrappers
{
    public class CommentWrapper : IModelWrapper<Comment>
    {
        public int Id { get; private set; }
        public int Content { get; private set; }
        public DateTimeOffset Timestamp { get; private set; }
        public string Username { get; private set; }
        public string UserLink { get; private set; }
        public string UserAvatar { get; private set; }

        public CommentWrapper(Comment comment)
        {
            Id = comment.Id;
            Content = comment.Content;
            Timestamp = comment.Timestamp;
            var user = comment.User;
            Username = user.Username;
            UserLink = $"/users/{user.Id}";
            UserAvatar = $"/avatars/{user.AvatarPhoto}";
        }
    }
}