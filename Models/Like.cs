using System;
using System.Collections.Generic;

namespace InstaminiWebService.Models
{
    public partial class Like
    {
        public int UserId { get; set; }
        public int LikedPost { get; set; }
        public bool? IsActive { get; set; }

        public virtual Post LikedPostNavigation { get; set; }
        public virtual User User { get; set; }
    }
}
