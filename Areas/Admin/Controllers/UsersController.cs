using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using _25dh110213_BE_LTW.Models;
using _25dh110213_BE_LTW.Filters;

namespace _25dh110213_BE_LTW.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class UsersController : Controller
    {
        private My_StoreEntities db = new My_StoreEntities();

        // GET: Admin/Users
        public ActionResult Index()
        {
            var users = db.Users.ToList();
            return View(users);
        }
    }
}
