using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MovieWeb.Models
{
    public partial class Comment
    {
        public int ParentId { get; set; }
        public string CommentTxt { get; set; }
        public int UserID { get; set; }
        public System.DateTime DateSumitted { get; set; }
        public int MovieID { get; set; }
    }
}