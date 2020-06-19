﻿using InstaminiWebService.Models;
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
        public string Link { get; private set; }

        public UserResponse(User user)
        {
            Id = user.Id;
            Username = user.Username;
            Link = $"/users/{Username}";
            DisplayName = user.DisplayName;
            AvatarLink = $"/avatars/{user.AvatarPhoto.FileName}";
            Created = user.Created;
            Followers = user.Followers.Select(f => new
            {
                f.UserId,
                f.User.DisplayName
            });
            Followings = user.Followings.Select(f => new
            {
                f.UserId,
                f.User.DisplayName
            });
            FollowerCount = Followers.Count();
            FollowingCount = Followings.Count();
        }
    }
}