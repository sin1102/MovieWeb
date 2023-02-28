using MovieWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList.Mvc;
using PagedList;
using System.IO;
using System.Threading;
using Firebase.Storage;
using System.Threading.Tasks;

namespace MovieWeb.Controllers
{
    public class AdminController : Controller
    {
        private static string ApiKey = "AIzaSyCo0xUItvNUqEEDXy_rduouV8aON9twq98";
        private static string Bucket = "movieweb-1e4b2.appspot.com";

        DataClasses1DataContext data = new DataClasses1DataContext();
        public ActionResult Index()
        {
            if (Session["AdminAccount"] == null)
            {
                return RedirectToAction("Login", "Admin");
            }
            return View();
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(FormCollection collection)
        {
            var username = collection["UserName"];
            var password = collection["Password"];
            if (String.IsNullOrEmpty(username))
            {
                ViewData["Loi1"] = "Please enter your Username";
            }
            else if (String.IsNullOrEmpty(password))
            {
                ViewData["Loi2"] = "Please enter your Password";
            }
            else
            {
                var md5_password = MD5Encryption.GetMD5(password);
                Admin ad = data.Admins.SingleOrDefault(n => n.AdminName == username && n.Password == md5_password);
                if (ad != null)
                {
                    Session["AdminAccount"] = ad;
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    ViewBag.ThongBao = "The Username or Password is Incorrect";
                }
            }
            return View();
        }

        public ActionResult Movie(int ?page)
        {
            int pageNumber = (page ?? 1);
            int pageSize = 5;

            return View(data.Movies.ToList().OrderBy(n => n.MovieID).ToPagedList(pageNumber, pageSize));
        }

        [HttpGet]
        public ActionResult CreateMovie()
        {
            ViewBag.CountryID = new SelectList(data.Countries.ToList().OrderBy(n => n.CountryName), "CountryID", "CountryName");
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> CreateMovie(Movie movie, HttpPostedFileBase fileUpload, HttpPostedFileBase fileUpload2, HttpPostedFileBase fileUpload3)
        {
            ViewBag.CountryID = new SelectList(data.Countries.ToList().OrderBy(n => n.CountryName), "CountryID", "CountryName");
            
            if (fileUpload == null && fileUpload2 == null && fileUpload3 == null)
            {
                ViewBag.NullFile = "Please choose your file";
                return View();
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var filePicture = Path.GetFileName(fileUpload.FileName);
                    var pathPicture = Path.Combine(Server.MapPath("~/images"), filePicture);
                    fileUpload.SaveAs(pathPicture);
                    var stream = new FileStream(Path.Combine(pathPicture), FileMode.Open);

                    var fileImage = Path.GetFileName(fileUpload2.FileName);
                    var pathImage = Path.Combine(Server.MapPath("~/images"), fileImage);
                    fileUpload2.SaveAs(pathImage);
                    var stream2 = new FileStream(Path.Combine(pathImage), FileMode.Open);


                    var fileVideo = Path.GetFileName(fileUpload3.FileName);
                    var pathVideo = Path.Combine(Server.MapPath("~/images"), fileVideo);
                    fileUpload3.SaveAs(pathVideo);
                    var stream3 = new FileStream(Path.Combine(pathVideo), FileMode.Open);

                    var cancellation = new CancellationTokenSource();

                    var task1 = new FirebaseStorage(
                        Bucket,
                        new FirebaseStorageOptions
                        {
                            ThrowOnCancel = true // when you cancel the upload, exception is thrown. By default no exception is thrown
                })
                        .Child("Image")
                        .Child(filePicture)
                        .PutAsync(stream, cancellation.Token);

                    movie.MoviePicture = await task1;

                    var task2 = new FirebaseStorage(
                       Bucket,
                       new FirebaseStorageOptions
                       {
                           ThrowOnCancel = true // when you cancel the upload, exception is thrown. By default no exception is thrown
               })
                       .Child("Image")
                       .Child(fileImage)
                       .PutAsync(stream2, cancellation.Token);

                    movie.MovieImage = await task2;

                    var task3 = new FirebaseStorage(
                       Bucket,
                       new FirebaseStorageOptions
                       {
                           ThrowOnCancel = true // when you cancel the upload, exception is thrown. By default no exception is thrown
               })
                       .Child("Movie")
                       .Child(fileVideo)
                       .PutAsync(stream3, cancellation.Token);

                    task3.Progress.ProgressChanged += (s, e) => Console.WriteLine($"Progress: {e.Percentage} %");
                    movie.Url = await task3;

                    data.Movies.InsertOnSubmit(movie);
                    data.SubmitChanges();
                }
            }
            return RedirectToAction("Movie", "Admin");
        }

        [HttpGet]
        public ActionResult DeleteMovie(int id)
        {
            Movie movie = data.Movies.SingleOrDefault(n => n.MovieID == id);
            ViewBag.Movie = movie.MovieID;
            if(movie == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(movie);
        }
        [HttpPost, ActionName("DeleteMovie")]
        public ActionResult ConfirmDelete(int id)
        {
            Movie movie = data.Movies.SingleOrDefault(n => n.MovieID == id);
            ViewBag.Movie = movie.MovieID;
            if(movie == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            data.Movies.DeleteOnSubmit(movie);
            data.SubmitChanges();
            return RedirectToAction("Movie");
        }
        [HttpGet]
        public ActionResult EditMovie(int id)
        {
            ViewBag.CountryID = new SelectList(data.Countries.ToList().OrderBy(n => n.CountryName), "CountryID", "CountryName");
            Movie movie = data.Movies.SingleOrDefault(n => n.MovieID == id);
            if(movie == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(movie);
        }
        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> EditMovie(Movie movie, HttpPostedFileBase fileUpload, HttpPostedFileBase fileUpload2, HttpPostedFileBase fileUpload3)
        {
            ViewBag.CountryID = new SelectList(data.Countries.ToList().OrderBy(n => n.CountryName), "CountryID", "CountryName");
            if (fileUpload == null && fileUpload2 == null && fileUpload3 == null)
            {
                ViewBag.NullFile = "Please choose your file";
                return View();
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var filePicture = Path.GetFileName(fileUpload.FileName);
                    var pathPicture = Path.Combine(Server.MapPath("~/images"), filePicture);
                    fileUpload.SaveAs(pathPicture);
                    var stream = new FileStream(Path.Combine(pathPicture), FileMode.Open);

                    var fileImage = Path.GetFileName(fileUpload2.FileName);
                    var pathImage = Path.Combine(Server.MapPath("~/images"), fileImage);
                    fileUpload2.SaveAs(pathImage);
                    var stream2 = new FileStream(Path.Combine(pathImage), FileMode.Open);


                    var fileVideo = Path.GetFileName(fileUpload3.FileName);
                    var pathVideo = Path.Combine(Server.MapPath("~/images"), fileVideo);
                    fileUpload3.SaveAs(pathVideo);
                    var stream3 = new FileStream(Path.Combine(pathVideo), FileMode.Open);

                    var cancellation = new CancellationTokenSource();

                    var task1 = new FirebaseStorage(
                        Bucket,
                        new FirebaseStorageOptions
                        {
                            ThrowOnCancel = true // when you cancel the upload, exception is thrown. By default no exception is thrown
                        })
                        .Child("Image")
                        .Child(filePicture)
                        .PutAsync(stream, cancellation.Token);

                    movie.MoviePicture = await task1;

                    var task2 = new FirebaseStorage(
                       Bucket,
                       new FirebaseStorageOptions
                       {
                           ThrowOnCancel = true // when you cancel the upload, exception is thrown. By default no exception is thrown
                       })
                       .Child("Image")
                       .Child(fileImage)
                       .PutAsync(stream2, cancellation.Token);

                    movie.MovieImage = await task2;

                    var task3 = new FirebaseStorage(
                       Bucket,
                       new FirebaseStorageOptions
                       {
                           ThrowOnCancel = true // when you cancel the upload, exception is thrown. By default no exception is thrown
                       })
                       .Child("Movie")
                       .Child(fileVideo)
                       .PutAsync(stream3, cancellation.Token);

                    task3.Progress.ProgressChanged += (s, e) => Console.WriteLine($"Progress: {e.Percentage} %");
                    movie.Url = await task3;

                    UpdateModel(movie);
                    data.SubmitChanges();
                }
            }
            return RedirectToAction("Movie");
        }

        public ActionResult Actor(int ?page)
        {
            int pageNumber = (page ?? 1);
            int pageSize = 10;
            return View(data.Actors.ToList().OrderBy(n => n.ActorID).ToPagedList(pageNumber, pageSize));
        }
        [HttpGet]
        public ActionResult CreateActor()
        {
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateActor(FormCollection collection, Actor actor)
        {
            var actorName = collection["ActorName"];
            var actorBirthDay = collection["ActorBirthDay"];

            if (String.IsNullOrEmpty(actorName))
            {
                ViewBag.NullActorName = "Please insert actor's name";
            }else if (data.Actors.SingleOrDefault(n => n.ActorName == actorName) != null)
            {
                ViewBag.ExistedName = "Actor has existed";
            }
            else
            {
                actor.ActorName = actorName;
                actor.NamSinh = Convert.ToInt32(actorBirthDay);
                data.Actors.InsertOnSubmit(actor);
                data.SubmitChanges();
                return RedirectToAction("Actor");
            }
            return View();
        }
        [HttpGet]
        public ActionResult EditActor(int id)
        {
            Actor actor = data.Actors.SingleOrDefault(n => n.ActorID == id);
            if (actor == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(actor);
        }
        [HttpPost, ActionName("EditActor")]
        public ActionResult ConfirmEditActor(FormCollection collection, int id)
        {
            Actor actor = data.Actors.SingleOrDefault(n => n.ActorID == id);
            var actorName = collection["ActorName"];
            var actorBirthDay = collection["ActorBirthDay"];
            if (String.IsNullOrEmpty(actorName))
            {
                ViewBag.NullActorName = "Please insert actor's name";
            }
            else
            {
                actor.ActorName = actorName;
                actor.NamSinh = Convert.ToInt32(actorBirthDay);
                UpdateModel(actor);
                data.SubmitChanges();
                return RedirectToAction("Actor");
            }
            return View();
        }
        [HttpGet]
        public ActionResult DeleteActor(int id)
        {
            Actor actor = data.Actors.SingleOrDefault(n => n.ActorID == id);
            ViewBag.Actor = actor.ActorID;
            if (actor == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(actor);
        }
        [HttpPost, ActionName("DeleteActor")]
        public ActionResult ConfirmDeleteActor(int id)
        {
            Actor actor = data.Actors.SingleOrDefault(n => n.ActorID == id);
            ViewBag.Actor = actor.ActorID;
            if (actor == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            data.Actors.DeleteOnSubmit(actor);
            data.SubmitChanges();
            return RedirectToAction("Actor");
        }
        public ActionResult Country(int ?page)
        {
            int pageNumber = (page ?? 1);
            int pageSize = 10;
            return View(data.Countries.ToList().OrderBy(n => n.CountryID).ToPagedList(pageNumber, pageSize));
        }
        [HttpGet]
        public ActionResult CreateCountry()
        {
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateCountry(FormCollection collection, Country country)
        {
            var countryName = collection["CountryName"];

            if (String.IsNullOrEmpty(countryName))
            {
                ViewBag.NullCountry = "Please insert Country's Name";
            }else if(data.Countries.SingleOrDefault(c => c.CountryName == countryName) != null)
            {
                ViewBag.ExistedCountry = "Country has existed";
            }
            else
            {
                country.CountryName = countryName;
                data.Countries.InsertOnSubmit(country);
                data.SubmitChanges();
                return RedirectToAction("Country");
            }
            return View();
        }
        [HttpGet]
        public ActionResult EditCountry(int id)
        {
            Country country = data.Countries.SingleOrDefault(n => n.CountryID == id);

            if(country == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(country);
        }
        [HttpPost, ActionName("EditCountry")]
        public ActionResult ConfirmEditCountry(FormCollection collection, int id)
        {
            Country country = data.Countries.SingleOrDefault(n => n.CountryID == id);
            var countryName = collection["CountryName"];

            if (String.IsNullOrEmpty(countryName))
            {
                ViewBag.NullCountry = "Please insert Country's Name";
            }
            else
            {
                country.CountryName = countryName;
                UpdateModel(country);
                data.SubmitChanges();
                return RedirectToAction("Country");
            }
            return View();
        }
        [HttpGet]
        public ActionResult DeleteCountry(int id)
        {
            Country country = data.Countries.SingleOrDefault(n => n.CountryID == id);
            ViewBag.Country = country.CountryID;
            if (country == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(country);
        }
        [HttpPost, ActionName("DeleteCountry")]
        public ActionResult ConfirmDeleteCountry(int id)
        {
            Country country = data.Countries.SingleOrDefault(n => n.CountryID == id);
            ViewBag.Country = country.CountryID;
            if (country == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            data.Countries.DeleteOnSubmit(country);
            data.SubmitChanges();
            return RedirectToAction("Country");
        }

        //Genre Page
        public ActionResult Genre(int ?page)
        {
            int pageNumber = (page ?? 1);
            int pageSize = 10;
            return View(data.Genres.ToList().OrderBy(n => n.GenreID).ToPagedList(pageNumber, pageSize));
        }

        //Create Genre
        [HttpGet]
        public ActionResult CreateGenre()
        {
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateGenre(FormCollection collection, Genre genre)
        {
            var genreName = collection["GenreName"];

            if (String.IsNullOrEmpty(genreName))
            {
                ViewBag.NullGenre = "Please insert Genre name";
            }
            else if (data.Genres.SingleOrDefault(c => c.GenreName == genreName) != null)
            {
                ViewBag.ExistedGenre = "Genre has existed";
            }
            else
            {
                genre.GenreName = genreName;
                data.Genres.InsertOnSubmit(genre);
                data.SubmitChanges();
                return RedirectToAction("Genre");
            }
            return View();
        }

        //Edit Genre
        [HttpGet]
        public ActionResult EditGenre(int id)
        {
            Genre genre = data.Genres.SingleOrDefault(n => n.GenreID == id);

            if (genre == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(genre);
        }
        [HttpPost, ActionName("EditGenre")]
        public ActionResult ConfirmEditGenre(FormCollection collection, int id)
        {
            Genre genre = data.Genres.SingleOrDefault(n => n.GenreID == id);
            var genreName = collection["GenreName"];

            if (String.IsNullOrEmpty(genreName))
            {
                ViewBag.NullGenre = "Please insert Genre name";
            }
            else
            {
                genre.GenreName = genreName;
                UpdateModel(genre);
                data.SubmitChanges();
                return RedirectToAction("Genre");
            }
            return View();
        }

        //Delete Genre
        [HttpGet]
        public ActionResult DeleteGenre(int id)
        {
            Genre genre = data.Genres.SingleOrDefault(n => n.GenreID == id);
            ViewBag.Genre = genre.GenreID;
            if (genre == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(genre);
        }
        [HttpPost, ActionName("DeleteGenre")]
        public ActionResult ConfirmDeleteGenre(int id)
        {
            Genre genre = data.Genres.SingleOrDefault(n => n.GenreID == id);
            ViewBag.Genre = genre.GenreID;
            if (genre == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            data.Genres.DeleteOnSubmit(genre);
            data.SubmitChanges();
            return RedirectToAction("Genre");
        }

        //Account Page
        public ActionResult Account(int ?page)
        {
            int pageNumber = (page ?? 1);
            int pageSize = 100;
            return View(data.Users.ToList().OrderBy(n => n.UserID).ToPagedList(pageNumber, pageSize));
        }

        //Create Account
        [HttpGet]
        public ActionResult CreateAccount()
        {
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateAccount(FormCollection collection, User user)
        {
            var userName = collection["UserName"];
            var userPassword = collection["Password"];
            var userEmail = collection["Email"];
            var confirmPassword = collection["ConfirmPassword"];

            if (String.IsNullOrEmpty(userName))
            {
                ViewBag.NullUserName = "Please insert username";
            }else if (String.IsNullOrEmpty(userPassword))
            {
                ViewBag.NullPassword = "Please insert password";
            }else if (userPassword.Length < 6)
            {
                ViewBag.Length = "Password must larger or equal than 6 characters";
            }
            else if (String.IsNullOrEmpty(userEmail))
            {
                ViewBag.NullEmail = "Please insert Email";
            }else if (data.Users.SingleOrDefault(n => n.Email == userEmail) != null)
            {
                ViewBag.ExistedEmail = "Email has existed";
            }
            else if (data.Users.SingleOrDefault(u => u.UserName == userName) != null)
            {
                ViewBag.ExistedUserName = "Username has existed";
            }
            else
            {
                var password_md5 = MD5Encryption.GetMD5(userPassword);

                user.UserName = userName;
                user.Email = userEmail;
                user.Password = password_md5;
                data.Users.InsertOnSubmit(user);
                data.SubmitChanges();
                return RedirectToAction("Account");
            }
            return View();
        }

        //Edit Account
        [HttpGet]
        public ActionResult EditAccount(int id)
        {
            User user = data.Users.SingleOrDefault(n => n.UserID == id);

            if (user == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(user);
        }
        [HttpPost, ActionName("EditAccount")]
        public ActionResult ConfirmEditAccount(FormCollection collection, int id)
        {
            User user = data.Users.SingleOrDefault(n => n.UserID == id);
            var userName = collection["UserName"];
            var userEmail = collection["Email"];

            if (String.IsNullOrEmpty(userName))
            {
                ViewBag.NullUserName = "Please insert username";
            }
            else if (String.IsNullOrEmpty(userEmail))
            {
                ViewBag.NullEmail = "Please insert Email";
            }            
            else
            {
                user.UserName = userName;
                UpdateModel(user);
                data.SubmitChanges();
                return RedirectToAction("Account");
            }
            return View();
        }
    }
}