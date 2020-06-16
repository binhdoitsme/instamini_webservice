using System;
using System.Collections.Generic;

namespace InstaminiWebService.Models
{
    public partial class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public int UserId { get; set; }
        public int PostId { get; set; }

        public virtual Post Post { get; set; }
        public virtual User User { get; set; }
    }
}
