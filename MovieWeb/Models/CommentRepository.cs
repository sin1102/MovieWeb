using MovieWeb.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MovieWeb.Models
{
    public class CommentRepository
    {
        DataClasses1DataContext context = new DataClasses1DataContext();
        public IQueryable<Rating> GetAll(int movieId)
        {
            return context.Ratings.Where(x => x.MovieID == movieId).OrderBy(x => x.DateSumitted);
        }

        public commentViewModel AddComment(commentDTO comment, int movieId)
        {
            var _comment = new Rating()
            {
                MovieID = movieId,
                ParentId = comment.ParentId,
                CommentTxt = comment.CommentTxt,
                UserID = comment.UserId,
                DateSumitted = DateTime.Now
            };

            context.Ratings.InsertOnSubmit(_comment);
            context.SubmitChanges();
            return context.Ratings.Where(x => x.RateID == _comment.RateID)
                    .Select(x => new commentViewModel
                    {
                        MovieId = (int)x.MovieID,
                        RateId = x.RateID,
                        CommentTxt = x.CommentTxt,
                        ParentId = (int)x.ParentId,
                        DateSumitted = (DateTime)x.DateSumitted,
                        UserId = (int)x.UserID

                    }).FirstOrDefault();
        }
    }

}