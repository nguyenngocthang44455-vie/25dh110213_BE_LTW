using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using _25dh110213_BE_LTW.Models;
using _25dh110213_BE_LTW.Models.ViewModel;

namespace _25dh110213_BE_LTW.Controllers
{
    public class AccountController : Controller
    {
        private My_StoreEntities db = new My_StoreEntities();

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

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

                try
                {
                    db.Users.Add(new User
                    {
                        Username = model.Username,
                        Password = model.Password,
                        UserRole = "C"
                    });

                    db.Customers.Add(new Customer
                    {
                        Username = model.Username,
                        CustomerName = model.CustomerName,
                        CustomerEmail = model.CustomerEmail,
                        CustomerPhone = model.CustomerPhone,
                        CustomerAddress = model.CustomerAddress
                    });

                    db.SaveChanges();
                    return RedirectToAction("Login", "Account");
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    foreach (var validationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            ModelState.AddModelError("", string.Format("Lỗi tại trường '{0}': {1}",
                                validationError.PropertyName, validationError.ErrorMessage));
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi hệ thống: " + ex.Message);
                }
            }
            return View(model);
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

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
                else
                {
                    adminUser.Password = "123456";
                    adminUser.UserRole = "A";
                }

                var customerUser = db.Users.SingleOrDefault(u => u.Username == "khachhang1");
                if (customerUser == null)
                {
                    db.Users.Add(new User { Username = "khachhang1", Password = "123456", UserRole = "C" });
                }
                else
                {
                    customerUser.Password = "123456";
                    customerUser.UserRole = "C";
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

        public ActionResult Logout()
        {
            Session.Clear();
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home", new { area = "" });
        }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(ProfileVM model)
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                string currentUsername = Session["Username"].ToString();
                var customer = db.Customers.SingleOrDefault(c => c.Username == currentUsername);
                if (customer != null)
                {
                    customer.CustomerName = model.CustomerName;
                    customer.CustomerPhone = model.CustomerPhone;
                    customer.CustomerEmail = model.CustomerEmail;
                    customer.CustomerAddress = model.CustomerAddress;

                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Cập nhật thông tin cá nhân thành công!";
                }
                else
                {
                    return HttpNotFound();
                }
            }
            return RedirectToAction("ProfileInfo");
        }

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
                if (user.Password != oldPassword)
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
