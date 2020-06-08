using System;
using System.Collections.Generic;

namespace InstaminiWebService.Models
{
    public partial class Photo
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public int PostId { get; set; }

        public virtual Post Post { get; set; }
    }
}
