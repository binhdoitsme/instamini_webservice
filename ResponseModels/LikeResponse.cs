using InstaminiWebService.Models;
using InstaminiWebService.ResponseModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InstaminiWebService.ResponseModels
{
    public class LikeResponse : IResponseModel<Like>
    {
        public string Username { get; private set; }
        public string UserAvatar { get; private set; }
        public string UserLink { get; private set; }

        public LikeResponse(Like like)
        {
            Username = like.User.Username;
            UserAvatar = $"/avatars/{like.User.AvatarPhoto.FileName}";
            UserLink = $"/users/{Username}";
        }
    }
}
