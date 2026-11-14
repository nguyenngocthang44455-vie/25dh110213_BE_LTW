using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using _25dh110213_BE_LTW.Models;

namespace _25dh110213_BE_LTW.Areas.Admin.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private My_StoreEntities db = new My_StoreEntities();

        // GET: Admin/Users
        public ActionResult Index()
        {
            // Tải danh sách tất cả các tài khoản phân quyền trong hệ thống
            var users = db.Users.ToList();
            return View(users);
        }
    }
}