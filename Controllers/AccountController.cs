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
                    var user = new User
                    {
                        Username = model.Username,
                        Password = model.Password,
                        UserRole = "C"
                    };
                    db.Users.Add(user);

                    var customer = new Customer
                    {
                        Username = model.Username,
                        CustomerName = model.CustomerName,
                        CustomerEmail = model.CustomerEmail,
                        CustomerPhone = model.CustomerPhone,
                        CustomerAddress = model.CustomerAddress
                    };
                    db.Customers.Add(customer);

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
                var user = db.Users.SingleOrDefault(u => u.Username == model.Username && u.Password == model.Password && u.UserRole == "C");
                if (user != null)
                {
                    Session["Username"] = user.Username;
                    Session["UserRole"] = user.UserRole;

                    FormsAuthentication.SetAuthCookie(user.Username, false);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                }
            }
            return View(model);
        }

        public ActionResult Logout()
        {
            Session.Clear();
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
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
            // SỬA LỖI: Kiểm tra Session trực tiếp thay vì phụ thuộc thuộc tính [Authorize]
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
            // SỬA LỖI: Kiểm tra Session trực tiếp thay vì phụ thuộc thuộc tính [Authorize]
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