using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MovieWeb.Models;
using System.Web.Security;

namespace MovieWeb.Controllers
{
    public class UserController : Controller
    {

        DataClasses1DataContext data = new DataClasses1DataContext();

        // GET: User
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult SignUp()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SignUp(FormCollection collection, User user)
        {
            var email = collection["Email"];
            var username = collection["UserName"];
            var password = collection["Password"];
            var confirmpassword = collection["ConfirmPassword"];

            if (data.Users.SingleOrDefault(u => u.Email == email) !=  null)
            {
                ViewBag.ThongBaoEmail = "Email already exists";
            }
            else if (data.Users.SingleOrDefault(u => u.UserName == username) != null)
            {
                ViewBag.ThongBaoUsername = "Username already exists";
            }
            else if (!password.Equals(confirmpassword))
            {
                ViewBag.ThongBaoPassword = "Password and Confirm Password does not match";
            }
            else if (String.IsNullOrEmpty(email))
            {
                ViewBag.ThongBaoEmailEmpty = "Please enter your Email";
            }
            else if (String.IsNullOrEmpty(username))
            {
                ViewBag.ThongBaoUsernameEmpty = "Please enter your Username";
            }
            else if (String.IsNullOrEmpty(password))
            {
                ViewBag.ThongBaoPasswordEmpty = "Please enter your Password";
            }
            else if (String.IsNullOrEmpty(confirmpassword))
            {
                ViewBag.ThongBaoConfirmPasswordEmpty = "Please confirm your Password";
            }
            else
            {
                var md5_password = MD5Encryption.GetMD5(password);
                user.Email = email;
                user.UserName = username;
                user.Password = md5_password;

                data.Users.InsertOnSubmit(user);
                data.SubmitChanges();

                return RedirectToAction("SignIn");
            }
            return this.SignUp();
        }      
        public ActionResult SignIn(FormCollection collection)
        {
            var username = collection["UserName"];
            var userPassword = collection["Password"];
            
            if (String.IsNullOrEmpty(username))
            {
                ViewData["Loi1"] = "Please enter your Username";
            }
            else if (String.IsNullOrEmpty(userPassword))
            {
                ViewData["Loi2"] = "Please enter your Password";
            }
            else
            {
                var md5_password = MD5Encryption.GetMD5(userPassword);
                User user = data.Users.SingleOrDefault(s => s.UserName == username && s.Password == md5_password);
                if(user != null)
                {
                    Session["Account"] = user;
                    return RedirectToAction("Index", "Home");
                }else
                {
                    ViewBag.Invalid = "Username or Password is incorrect";
                }
                
            }
            return View();
        }
        public ActionResult ForgotPassword(FormCollection collection)
        {
            var email = collection["Email"];
            var password = collection["Password"];

            User user = data.Users.SingleOrDefault(s => s.Email == email);
            if (user != null)
            {
                return RedirectToAction("ChangePassword","User");
            }
            else
            {
                ViewBag.ThongBaoForgotPassword = "Wrong Email";
            }
            return View();
        }

        public ActionResult ChangePassword(FormCollection collection)
        {
            //var email = collection["Email"];
            var password = collection["NewPassword"];
            var confirmPassword = collection["ConfirmPassword"];

            if (String.IsNullOrEmpty(password))
            {
                ViewBag.ThongBaoNullPassword = "Please enter your password";
            }
            else if (String.IsNullOrEmpty(confirmPassword))
            {
                ViewBag.ThongBaoNullConfirmPassword = "Please enter confirm password";
            }
            else if(password.Length < 6)
            {
                ViewBag.ThongBaoPasswordCharacter = "Password must be larger or equal 6 characters";
            }
            else if (confirmPassword != password)
            {
                ViewBag.ThongBaoConfirmPassword = "Password doesn't match";
            }
            else
            {
                //data.Users.Where(u => u.Email == email).FirstOrDefault();
                //data.Users.SingleOrDefault(p => p.Password == password);
                //data.SubmitChanges();

                return RedirectToAction("SignIn");
            }
            return View();
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }
    }   
}