using System;
using System.Collections.Generic;

namespace InstaminiWebService.Models
{
    public partial class User
    {
        public User()
        {
            Comments = new HashSet<Comment>();
            Followings = new HashSet<Follow>();
            Followers = new HashSet<Follow>();
            Likes = new HashSet<Like>();
            Posts = new HashSet<Post>();
        }

        public int Id { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset LastUpdate { get; set; }
        public DateTimeOffset? LastLogin { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Follow> Followings { get; set; }
        public virtual ICollection<Follow> Followers { get; set; }
        public virtual ICollection<Like> Likes { get; set; }
        public virtual ICollection<Post> Posts { get; set; }
    }
}
