using _25dh110213_BE_LTW.Models;
using _25dh110213_BE_LTW.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace _25dh110213_BE_LTW.Controllers
{
    public class AccountController : Controller
    {
        private My_StoreEntities db = new My_StoreEntities();

        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = db.Users.SingleOrDefault(u => u.Username == model.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại!");
                    return View(model);
                }

                var user = new User
                {
                    Username = model.Username,
                    Password = model.Password,
                    UserRole = "C"
                };
                db.Users.Add(user);

                var customer = new Customer
                {
                    CustomerName = model.CustomerName,
                    CustomerEmail = model.CustomerEmail,
                    CustomerPhone = model.CustomerPhone,
                    CustomerAddress = model.CustomerAddress,
                    Username = model.Username,
                };
                db.Customers.Add(customer);

                db.SaveChanges();

                return RedirectToAction("Login", "Account");
            }

            return View(model);
        }

        // GET: Account/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                EnsureDefaultAccounts();

                var user = db.Users.SingleOrDefault(u => u.Username == model.Username && u.Password == model.Password);
                if (user != null)
                {
                    Session["Username"] = user.Username;
                    Session["UserRole"] = user.UserRole;

                    FormsAuthentication.SetAuthCookie(user.Username, false);

                    // Khi đăng nhập thành công (kể cả Admin), luôn đưa ra Trang chủ. Muốn vào trang quản trị thì bấm nút 'Quản trị' trên thanh menu.
                    return RedirectToAction("Index", "Home", new { area = "" });
                }
                else
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                }
            }
            return View(model);
        }

        private void EnsureDefaultAccounts()
        {
            try
            {
                var adminUser = db.Users.SingleOrDefault(u => u.Username == "admin");
                if (adminUser == null)
                {
                    db.Users.Add(new User { Username = "admin", Password = "123456", UserRole = "A" });
                }

                // THÊM ĐOẠN NÀY: Tạo hồ sơ Customer cho admin nếu chưa có
                var adminProfile = db.Customers.SingleOrDefault(c => c.Username == "admin");
                if (adminProfile == null)
                {
                    db.Customers.Add(new Customer
                    {
                        Username = "admin",
                        CustomerName = "Quản trị viên (Admin)",
                        CustomerEmail = "admin@mystore.com",
                        CustomerPhone = "0900000000",
                        CustomerAddress = "Hệ thống quản trị"
                    });
                }

                var customerUser = db.Users.SingleOrDefault(u => u.Username == "khachhang1");
                if (customerUser == null)
                {
                    db.Users.Add(new User { Username = "khachhang1", Password = "123456", UserRole = "C" });
                }

                var customerProfile = db.Customers.SingleOrDefault(c => c.Username == "khachhang1");
                if (customerProfile == null)
                {
                    db.Customers.Add(new Customer
                    {
                        Username = "khachhang1",
                        CustomerName = "Khách Hàng 1",
                        CustomerEmail = "khachhang1@gmail.com",
                        CustomerPhone = "0901234567",
                        CustomerAddress = "TP. Hồ Chí Minh"
                    });
                }

                db.SaveChanges();
            }
            catch
            {
            }
        }


        // GET: Account/Logout
        public ActionResult Logout()
        {
            // 1. Hủy vé đăng nhập FormsAuthentication (xóa User.Identity.IsAuthenticated)
            FormsAuthentication.SignOut();

            // 2. Xóa sạch toàn bộ Session
            Session.Clear();
            Session.Abandon();

            // 3. Xóa triệt để cookie xác thực trên trình duyệt
            if (Request.Cookies[FormsAuthentication.FormsCookieName] != null)
            {
                var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, "")
                {
                    Expires = DateTime.Now.AddYears(-1)
                };
                Response.Cookies.Add(authCookie);
            }

            // 4. Xóa session cookie
            if (Request.Cookies["ASP.NET_SessionId"] != null)
            {
                var sessionCookie = new HttpCookie("ASP.NET_SessionId", "")
                {
                    Expires = DateTime.Now.AddYears(-1)
                };
                Response.Cookies.Add(sessionCookie);
            }

            return RedirectToAction("Login", "Account");
        }

        // GET: Account/ProfileInfo
        public ActionResult ProfileInfo()
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string currentUsername = Session["Username"].ToString();
            var customer = db.Customers.SingleOrDefault(c => c.Username == currentUsername);

            if (customer == null)
            {
                var user = db.Users.SingleOrDefault(u => u.Username == currentUsername);
                if (user != null && user.UserRole != null && user.UserRole.Trim().ToUpper() == "A")
                {
                    var adminModel = new ProfileVM
                    {
                        Username = user.Username,
                        CustomerName = "Quản trị viên (Admin)",
                        CustomerPhone = "Đang cập nhật",
                        CustomerEmail = "admin@mystore.com",
                        CustomerAddress = "Hệ thống quản trị"
                    };
                    return View(adminModel);
                }

                return HttpNotFound();
            }

            var model = new ProfileVM
            {
                Username = customer.Username,
                CustomerName = customer.CustomerName,
                CustomerPhone = customer.CustomerPhone,
                CustomerEmail = customer.CustomerEmail,
                CustomerAddress = customer.CustomerAddress
            };

            return View(model);
        }

        // POST: Account/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(ProfileVM model)
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string currentUsername = Session["Username"].ToString();
            var customer = db.Customers.SingleOrDefault(c => c.Username == currentUsername);

            if (customer == null)
            {
                var user = db.Users.SingleOrDefault(u => u.Username == currentUsername);
                if (user != null && user.UserRole != null && user.UserRole.Trim().ToUpper() == "A")
                {
                    customer = new Customer
                    {
                        Username = currentUsername,
                        CustomerName = model.CustomerName,
                        CustomerPhone = model.CustomerPhone,
                        CustomerEmail = model.CustomerEmail,
                        CustomerAddress = model.CustomerAddress
                    };
                    db.Customers.Add(customer);
                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Cập nhật thông tin hồ sơ quản trị viên thành công!";
                    return RedirectToAction("ProfileInfo");
                }

                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                customer.CustomerName = model.CustomerName;
                customer.CustomerPhone = model.CustomerPhone;
                customer.CustomerEmail = model.CustomerEmail;
                customer.CustomerAddress = model.CustomerAddress;

                db.SaveChanges();
                TempData["SuccessMessage"] = "Cập nhật thông tin tài khoản thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Vui lòng kiểm tra lại thông tin nhập vào!";
            }

            return RedirectToAction("ProfileInfo");
        }

        // POST: Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(string oldPassword, string newPassword, string confirmNewPassword)
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string currentUsername = Session["Username"].ToString();

            if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ mật khẩu cũ và mật khẩu mới!";
                return RedirectToAction("ProfileInfo");
            }

            if (newPassword != confirmNewPassword)
            {
                TempData["ErrorMessage"] = "Mật khẩu mới và xác nhận mật khẩu không khớp nhau!";
                return RedirectToAction("ProfileInfo");
            }

            var user = db.Users.SingleOrDefault(u => u.Username == currentUsername);
            if (user != null)
            {
                if (user.Password.Trim() != oldPassword.Trim())
                {
                    TempData["ErrorMessage"] = "Mật khẩu cũ nhập vào không chính xác!";
                    return RedirectToAction("ProfileInfo");
                }

                user.Password = newPassword;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Đổi mật khẩu tài khoản thành công!";
            }
            else
            {
                return HttpNotFound();
            }

            return RedirectToAction("ProfileInfo");
        }
    }
}
