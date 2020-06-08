using System;
using System.Collections.Generic;

namespace InstaminiWebService.Models
{
    public partial class Follow
    {
        public int UserId { get; set; }
        public int FollowerId { get; set; }
        public short IsActive { get; set; }

        public virtual User Follower { get; set; }
        public virtual User User { get; set; }
    }
}
