using System;
using System.Collections.Generic;

namespace InstaminiWebService.Models
{
    public partial class Post
    {
        public Post()
        {
            Comments = new HashSet<Comment>();
            Likes = new HashSet<Like>();
            Photos = new HashSet<Photo>();
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public string Caption { get; set; }
        public DateTimeOffset? Created { get; set; }

        public virtual User User { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Like> Likes { get; set; }
        public virtual ICollection<Photo> Photos { get; set; }
    }
}
