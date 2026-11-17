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
                model.TotalAmount = cart.Sum(c => c.TotalPrice);
                return View(model);
            }

            // Kiểm tra tồn kho thực tế và tính lại tổng tiền phía Server (chống sửa giá F12)
            decimal serverTotalAmount = 0;
            foreach (var item in cart)
            {
                var productInDb = db.Products.Find(item.ProductID);
                if (productInDb == null || productInDb.Quantity < item.Quantity)
                {
                    TempData["StockError"] = $"Sản phẩm '{item.ProductName}' không đủ số lượng trong kho.";
                    return RedirectToAction("Index", "Cart");
                }
                serverTotalAmount += productInDb.ProductPrice * item.Quantity;
            }

            string paymentStatus = "Chờ duyệt";

            Order order = new Order();
            order.CustomerID = customer.CustomerID;
            order.OrderDate = DateTime.Now;
            order.TotalAmount = serverTotalAmount;
            order.PaymentStatus = paymentStatus;
            order.PaymentMethod = model.PaymentMethod;
            order.DeliveryMethod = model.ShippingMethod;
            order.ShippingAddress = model.ShippingAddress;
            order.OrderDetails = new List<OrderDetail>();

            foreach (var item in cart)
            {
                var productInDb = db.Products.Find(item.ProductID);

                OrderDetail detail = new OrderDetail();
                detail.ProductID = item.ProductID;
                detail.Quantity = item.Quantity;
                detail.UnitPrice = productInDb.ProductPrice;
                detail.TotalPrice = productInDb.ProductPrice * item.Quantity;
                order.OrderDetails.Add(detail);

                productInDb.Quantity -= item.Quantity;
                if (productInDb.Quantity < 0) productInDb.Quantity = 0;
            }

            db.Orders.Add(order);
            db.SaveChanges();

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

        // Hàm xử lý xem trang chi tiết hóa đơn riêng biệt
        public ActionResult OrderDetail(int id)
        {
            var order = db.Orders.Include("OrderDetails.Product").SingleOrDefault(o => o.OrderID == id);
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
                var user = db.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
                if (user != null)
                {
                    customer = new Customer
                    {
                        Username = user.Username,
                        CustomerName = user.Username == "admin" ? "Quản trị viên (Admin)" : user.Username,
                        CustomerEmail = user.Username + "@mystore.com",
                        CustomerPhone = "0900000000",
                        CustomerAddress = "Chưa cập nhật"
                    };
                    db.Customers.Add(customer);
                    db.SaveChanges();
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }

            // Ẩn hoàn toàn các đơn đã bị hủy ngầm do bấm "Mua lại"
            var orders = db.Orders.Include("OrderDetails")
                                  .Where(o => o.CustomerID == customer.CustomerID
                                           && o.PaymentStatus != "Đã hủy (Mua lại)")
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

            // Ẩn luôn các đơn "Đã hủy (Mua lại)" khỏi Lịch sử mua hàng
            var orders = db.Orders.Where(o => o.CustomerID == customer.CustomerID
                                           && o.PaymentStatus != "Đã hủy (Mua lại)")
                                  .OrderByDescending(o => o.OrderDate)
                                  .ToList();

            return View(orders);
        }

        // GET: Order/BuyAgain/5
        public ActionResult BuyAgain(int id)
        {
            var order = db.Orders.SingleOrDefault(o => o.OrderID == id);
            if (order == null)
            {
                return HttpNotFound();
            }

            var details = db.OrderDetails.Where(d => d.OrderID == id).ToList();
            var cartService = GetCartService();
            var cart = cartService.GetCart();

            // 1. Nếu đơn hàng đang "Chờ duyệt" -> Hủy ngầm & đánh dấu để ẩn khỏi danh sách
            bool isPendingOrder = order.PaymentStatus != null && order.PaymentStatus.Trim() == "Chờ duyệt";
            if (isPendingOrder)
            {
                order.PaymentStatus = "Đã hủy (Mua lại)";
                db.Entry(order).State = EntityState.Modified;
            }

            // 2. Thêm lại từng sản phẩm vào Giỏ hàng với số lượng = 1
            foreach (var item in details)
            {
                var product = db.Products.SingleOrDefault(p => p.ProductID == item.ProductID);
                if (product != null)
                {
                    if (isPendingOrder)
                    {
                        product.Quantity += item.Quantity;
                        db.Entry(product).State = EntityState.Modified;
                    }

                    if (product.Quantity > 0)
                    {
                        string catName = product.Category != null ? product.Category.CategoryName : "Sản phẩm";
                        cart.AddItem(
                            product.ProductID,
                            product.ProductImage,
                            product.ProductName,
                            product.ProductPrice,
                            1,
                            catName
                        );
                    }
                }
            }

            if (isPendingOrder)
            {
                db.SaveChanges();
            }

            return RedirectToAction("Index", "Cart");
        }



       




        // Hủy đơn đổi trạng thái thành "Đã hủy" và tự động hoàn trả số lượng vào kho (Đã bịt lỗ hổng bảo mật)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult CancelOrder(int id)
        {
            var customer = db.Customers.SingleOrDefault(c => c.Username == User.Identity.Name);
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }

            // 1. Chặn hủy đơn hàng của người dùng khác
            if (order.CustomerID != customer.CustomerID)
            {
                return new HttpStatusCodeResult(403, "Bạn không có quyền hủy đơn hàng này.");
            }

            // 2. Chỉ cho phép hủy và hoàn kho khi đơn đang ở trạng thái "Chờ duyệt"
            string currentStatus = order.PaymentStatus != null ? order.PaymentStatus.Trim() : "";
            if (currentStatus == "Chờ duyệt")
            {
                var details = db.OrderDetails.Where(d => d.OrderID == id).ToList();
                foreach (var item in details)
                {
                    var product = db.Products.Find(item.ProductID);
                    if (product != null)
                    {
                        product.Quantity += item.Quantity; // Hoàn lại kho số lượng hàng
                    }
                }

                order.PaymentStatus = "Đã hủy"; // Cập nhật trạng thái
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
            }

            return RedirectToAction("MyOrder");
        }
    }
}
