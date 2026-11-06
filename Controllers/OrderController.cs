using _25dh110213_BE_LTW.Models;
using _25dh110213_BE_LTW.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace _25dh110213_BE_LTW.Controllers
{
    public class OrderController : Controller
    {
        private My_StoreEntities db = new My_StoreEntities();

        private CartService GetCartService()
        {
            return new CartService(Session);
        }

        // GET: Order
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Checkout()
        {
            // SỬA LỖI: Lấy giỏ hàng thông qua CartService thay vì ép kiểu trực tiếp List<CartItem>
            var cartService = GetCartService();
            var cartObj = cartService.GetCart();
            var cart = cartObj.Items?.ToList();

            if (cart == null || cart.Count == 0)
            {
                return RedirectToAction("Index", "Home");
            }

            if (User.Identity.IsAuthenticated == false)
            {
                return RedirectToAction("Login", "Account");
            }

            var customer = db.Customers.SingleOrDefault(c => c.Username == User.Identity.Name);
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new CheckoutVM();
            model.CartItems = cart;
            model.TotalAmount = cart.Sum(c => c.TotalPrice);
            model.CustomerName = customer.CustomerName;
            model.ShippingAddress = customer.CustomerAddress;
            model.CustomerPhone = customer.CustomerPhone;
            model.Username = customer.Username;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Checkout(CheckoutVM model)
        {
            var cartService = GetCartService();
            var cartObj = cartService.GetCart();
            var cart = cartObj.Items?.ToList();

            if (cart == null || cart.Count == 0)
            {
                return RedirectToAction("Index", "Home");
            }

            var user = db.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var customer = db.Customers.SingleOrDefault(c => c.Username == user.Username);
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid == false)
            {
                model.CartItems = cart;
                return View(model);
            }

            string paymentStatus = "Chưa thanh toán";
            switch (model.PaymentMethod)
            {
                case "Paypal": paymentStatus = "Thanh toán qua Paypal"; break;
                case "VNPAY": paymentStatus = "Thanh toán qua VNPAY"; break;
                case "COD": paymentStatus = "Thanh toán khi giao hàng"; break;
            }

            Order order = new Order();
            order.CustomerID = customer.CustomerID;
            order.OrderDate = DateTime.Now;
            order.TotalAmount = model.TotalAmount;
            order.PaymentStatus = paymentStatus;
            order.PaymentMethod = model.PaymentMethod;

            // SỬA LỖI: Gán bổ sung phương thức vận chuyển DeliveryMethod từ ViewModel
            order.DeliveryMethod = model.ShippingMethod;

            order.ShippingAddress = model.ShippingAddress;
            order.OrderDetails = new List<OrderDetail>();

            foreach (var item in cart)
            {
                OrderDetail detail = new OrderDetail();
                detail.ProductID = item.ProductID;
                detail.Quantity = item.Quantity;
                detail.UnitPrice = item.UnitPrice;
                detail.TotalPrice = item.TotalPrice;
                order.OrderDetails.Add(detail);
            }

            db.Orders.Add(order);
            db.SaveChanges();

            // Làm trống giỏ hàng sau khi đặt hàng thành công
            cartService.ClearCart();

            return RedirectToAction("OrderSuccess", "Order", new { id = order.OrderID });
        }

        public ActionResult OrderSuccess(int id)
        {
            var order = db.Orders.Include("OrderDetails").SingleOrDefault(o => o.OrderID == id);
            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        // GET: Order/MyOrder
        public ActionResult MyOrder(string searchTerm, string status)
        {
            if (User.Identity.IsAuthenticated == false)
            {
                return RedirectToAction("Login", "Account");
            }

            var customer = db.Customers.SingleOrDefault(c => c.Username == User.Identity.Name);
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = db.Orders.Include("OrderDetails")
                                  .Where(o => o.CustomerID == customer.CustomerID)
                                  .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                orders = orders.Where(o => o.OrderID.ToString().Contains(searchTerm) ||
                                           o.OrderDetails.Any(d => d.Product.ProductName.Contains(searchTerm)));
            }

            if (!string.IsNullOrEmpty(status) && status != "Tất cả")
            {
                orders = orders.Where(o => o.PaymentStatus == status);
            }

            ViewBag.CurrentSearch = searchTerm;
            ViewBag.CurrentStatus = string.IsNullOrEmpty(status) ? "Tất cả" : status;

            var result = orders.OrderByDescending(o => o.OrderDate).ToList();
            return View(result);
        }

        // GET: Order/OrderHistory
        public ActionResult OrderHistory()
        {
            if (User.Identity.IsAuthenticated == false)
            {
                return RedirectToAction("Login", "Account");
            }

            var customer = db.Customers.SingleOrDefault(c => c.Username == User.Identity.Name);
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = db.Orders.Where(o => o.CustomerID == customer.CustomerID)
                                  .OrderByDescending(o => o.OrderDate)
                                  .ToList();

            return View(orders);
        }
    }
}