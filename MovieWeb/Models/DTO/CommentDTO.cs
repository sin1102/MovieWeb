using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MovieWeb.Models.DTO
{
    public class commentDTO
    {

        public int ParentId { get; set; }
        public string CommentTxt { get; set; }
        public int UserId { get; set; }
    }

    public class commentViewModel : commentDTO
    {
        public int MovieId { get; set; }
        public int RateId { get; set; }
        public DateTime DateSumitted { get; set; }
        public string strDateSumitted { get {; return this.DateSumitted.ToString("dd-MM-yyyy"); } }
    }
}