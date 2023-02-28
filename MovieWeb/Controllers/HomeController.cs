using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MovieWeb.Models;
using MovieWeb.Models.DTO;
using PagedList;
using PagedList.Mvc;

namespace MovieWeb.Controllers
{

    public class MovieDetail
    {
        public int MovieID = 1;
        public String MovieName;
        public String Description;
        public String MovieImage;
        public String GenreName;
        public String Type;
        public DateTime ReleaseDate;
        public String Country;
        public String ActorName;
    }

    public class MovieComment
    {
        public String UserName;
        public String Comment;
        public String DateSumitted;
    }

    public class UpNext
    {
        public int MovieID;
        public String MovieName;
        public String ActorName;
        public String MoviePicture;
    }

    public class TopMovie
    {
        public String Genre;
    }

    public class HomeController : Controller
    {
        DataClasses1DataContext data = new DataClasses1DataContext();

        public ActionResult Index()
        {
            return View();
        }

        private List<Movie> LayPhimNoiBat(int count)
        {
            return data.Movies.OrderByDescending(a => a.MovieName).Take(count).ToList();
        }

        public ActionResult FeaturedMovies()
        {
            var featuredmovies = LayPhimNoiBat(18);
            return PartialView(featuredmovies);
        }

        private List<Movie> LayPhimMoi(int count)
        {
            return data.Movies.OrderByDescending(a => a.ReleaseDate).Take(count).ToList();
        }

        public ActionResult NewMovieSlide()
        {
            var newmovieslide = LayPhimMoi(6);
            return PartialView(newmovieslide);
        }

        private List<Movie> LayTopPhim()
        {
            var temp = from m in data.Movies 
                       join r in data.Ratings 
                       on m.MovieID equals r.MovieID into hay 
                       from h in hay 
                       where h.CommentTxt == "Hay"
                       select m;
            return temp.Distinct().ToList();
        }

        public ActionResult TopMovieSlide()
        {
            var newmovieslide = LayTopPhim();
            return PartialView(newmovieslide);
        }

        private List<MovieDetail> LayPhimHay()
        {
            var topmovie = (from m in data.Movies
                         join mg in data.Movie_Genres
                         on m.MovieID equals mg.MovieID
                         join gr in data.Genres
                         on mg.GenreID equals gr.GenreID
                         join r in data.Ratings
                         on m.MovieID equals r.MovieID
                         where r.CommentTxt == "Hay"
                         select new MovieDetail()
                         {
                             MovieID = m.MovieID,
                             MovieName = m.MovieName,
                             Description = m.Description,
                             MovieImage = m.MovieImage,
                             GenreName = gr.GenreName,
                             Type = m.Type,
                             ReleaseDate = (DateTime)m.ReleaseDate
                         }).ToList();
            return topmovie;
        }

        public ActionResult MostPopularMovie()
        {
            var mostpopularmovie = LayPhimHay().Take(8);
            return PartialView(mostpopularmovie);
        }

        public ActionResult GenreCategory()
        {
            var genrecategory = from gc in data.Genres select gc;
            return PartialView(genrecategory);
        }

        public ActionResult CountryCategory()
        {
            var countrycategory = from ct in data.Countries select ct;
            return PartialView(countrycategory);
        }

        public ActionResult MovieTheaters(int ? page)
        {
            int pageSize = 18;
            int pageNum = (page ?? 1);
            var movietheaters = (from m in data.Movies
                                 where m.Type == "Theaters"
                                 select m).ToList();
            return View(movietheaters.ToPagedList(pageNum,pageSize));
        }

        public ActionResult MovieSeries(int? page)
        {
            int pageSize = 18;
            int pageNum = (page ?? 1);
            var movieseries = (from m in data.Movies
                                 where m.Type == "Series"
                                 select m).ToList();
            return View(movieseries.ToPagedList(pageNum, pageSize));
        }

        public ActionResult PhimTheoTheLoai(int id,int? page)
        {
            int pageSize = 18;
            int pageNum = (page ?? 1);
            var genre = data.Genres.SingleOrDefault(s => s.GenreID == id);
            ViewBag.GenreName = genre.GenreName;
            var moviegenre = (from m in data.Movies 
                             join mg in data.Movie_Genres 
                             on m.MovieID equals mg.MovieID into gen
                             from g in gen
                             where g.GenreID == id
                             select m).Distinct();
            return View(moviegenre.ToPagedList(pageNum, pageSize));
        }

        public ActionResult PhimTheoQuocGia(int id,int? page)
        {
            int pageSize = 18;
            int pageNum = (page ?? 1);
            var country = data.Countries.SingleOrDefault(s => s.CountryID == id);
            ViewBag.CountryName = country.CountryName;
            var moviecountry = from s in data.Movies where s.CountryID == id select s;
            return View(moviecountry.ToPagedList(pageNum, pageSize));
        }

        public ActionResult Details(int id)
        {
            var movie = (from m in data.Movies
                         where m.MovieID == id
                         select new MovieDetail()
                         {
                             MovieID = id,
                             MovieName = m.MovieName,
                             Description = m.Description,
                             MovieImage = m.MovieImage,
                         }).FirstOrDefault();
            ViewBag.MovieId = id;
            return View(movie);
        }
        [HttpGet]
        public ActionResult Comment(int id)
        { 
            var comment = (from m in data.Movies
                           join r in data.Ratings
                           on m.MovieID equals r.MovieID
                           join u in data.Users
                           on r.UserID equals u.UserID
                           where m.MovieID == id
                           select new MovieComment()
                           {
                               UserName = u.UserName,
                               Comment = r.CommentTxt,
                               DateSumitted = r.DateSumitted.ToString(),
                           }).ToList();
            
            return PartialView(comment);
        }

        [HttpPost]
        public ActionResult PostComment (string txtComment)
        {
            return RedirectToAction("Details", "Home");
        }
        public ActionResult UpNext()
        {
            var upnext = (from m in data.Movies
                          join ma in data.Movie_Actors
                          on m.MovieID equals ma.MovieID
                          join a in data.Actors
                          on ma.ActorID equals a.ActorID
                          orderby m.ReleaseDate descending
                          select new UpNext
                          {
                              MovieID = m.MovieID,
                              ActorName = a.ActorName,
                              MovieName = m.MovieName,
                              MoviePicture = m.MoviePicture,       
                          }).Take(6).ToList();
            return PartialView(upnext);
        }

        public ActionResult ListMovie()
        {
            
            return View();
        }

        public ActionResult AzList(string x)
        {
            if (x.Equals("1"))
            {
                var list = (from m in data.Movies
                              join mg in data.Movie_Genres
                              on m.MovieID equals mg.MovieID
                              join gr in data.Genres
                              on mg.GenreID equals gr.GenreID
                              join c in data.Countries
                              on m.CountryID equals c.CountryID
                              orderby m.MovieName ascending
                              select new MovieDetail()
                              {
                                  MovieName = m.MovieName,
                                  MovieImage = m.MoviePicture,
                                  ReleaseDate = (DateTime)m.ReleaseDate,
                                  GenreName = gr.GenreName,
                                  Country = c.CountryName,
                              }).ToList();
                return PartialView(list);
            } 
            else
            {
                var list = (from m in data.Movies
                            join mg in data.Movie_Genres
                            on m.MovieID equals mg.MovieID
                            join gr in data.Genres
                            on mg.GenreID equals gr.GenreID
                            join c in data.Countries
                            on m.CountryID equals c.CountryID
                            where m.MovieName.StartsWith(x)
                            orderby m.MovieName descending
                            select new MovieDetail()
                            {
                                MovieName = m.MovieName,
                                MovieImage = m.MoviePicture,
                                ReleaseDate = (DateTime)m.ReleaseDate,
                                GenreName = gr.GenreName,
                                Country = c.CountryName,
                            }).ToList();
                return PartialView(list);
            }         
        }

        public ActionResult LiveTagSearch(string search)
        {
            if (search == "")
            {
               var res = (
               from t in data.Movies
               where t.MovieName.Contains("lasjsdfafasfkjsdhf")
               select t
               ).ToList();
                return PartialView(res);
            }
            else
            {
                var res = (
                from t in data.Movies
                where t.MovieName.Contains(search)
                select t
                ).ToList();
                return PartialView(res);
            }
        }

        public ActionResult SearchResult(string search, int? page)
        {
            int pageSize = 18;
            int pageNum = (page ?? 1);

            ViewBag.SearchResult = search;

            var res = (
               from t in data.Movies
               where t.MovieName.Contains(search)
               select t
               ).ToList();

            return View(res.ToPagedList(pageNum, pageSize));
        }
        public ActionResult ContactUs()
        {
            return View();
        }
        public ActionResult FAQ()
        {
            return View();
        }
        public ActionResult Privacy()
        {
            return View();
        }
        public ActionResult WatchMovie(int id)
        {
            var watchmovie = data.Movies.SingleOrDefault(s => s.MovieID == id);
            return View(watchmovie);
        }
        CommentRepository repo = new CommentRepository();
        public PartialViewResult CommentPartial(int movieId)
        {
            var comments = repo.GetAll(movieId);
            return PartialView("_CommentPartial", comments);
        }

        public JsonResult addNewComment(commentDTO comment, int movieId)
        {
            try
            {
                var model = repo.AddComment(comment, movieId);

                return Json(new { error = false, result = model }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                //Handle Error here..
            }

            return Json(new { error = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Recommendation(int id)
        {
            var genreID = data.Movie_Genres.FirstOrDefault(m => m.MovieID == id);
            var recommentdation = (from m in data.Movies
                          join mg in data.Movie_Genres
                          on m.MovieID equals mg.MovieID
                          where mg.GenreID == genreID.GenreID && m.MovieID != id
                          select new UpNext
                          {
                              MovieID = m.MovieID,
                              MovieName = m.MovieName,
                              MoviePicture = m.MoviePicture,
                          }).Take(5).ToList();
            return PartialView(recommentdation);
        }
    }
}