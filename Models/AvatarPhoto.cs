using System;
using System.Collections.Generic;

namespace InstaminiWebService.Models
{
    public partial class AvatarPhoto
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public int UserId { get; set; }

        public virtual User User { get; set; }
    }
}
