using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using _25dh110213_BE_LTW.Models;

namespace _25dh110213_BE_LTW.Areas.Admin.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private My_StoreEntities db = new My_StoreEntities();

        // GET: Admin/Orders
        public ActionResult Index()
        {
            // Sử dụng .Include để nạp kèm thông tin khách hàng và sắp xếp đơn mới lên đầu
            var orders = db.Orders.Include(o => o.Customer).OrderByDescending(o => o.OrderDate).ToList();
            return View(orders);
        }

        // ĐÃ CẬP NHẬT: Hàm xử lý thay đổi trạng thái đa năng và tự động hoàn trả kho khi Admin hủy đơn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string status)
        {
            var order = db.Orders.Find(id);
            if (order != null)
            {
                // Nếu Admin chủ động bấm nút Hủy đơn hộ khách -> Hệ thống tự kích hoạt lệnh hoàn kho
                if (status == "Đã hủy")
                {
                    var details = db.OrderDetails.Where(d => d.OrderID == id).ToList();
                    foreach (var item in details)
                    {
                        var product = db.Products.Find(item.ProductID);
                        if (product != null)
                        {
                            product.Quantity += item.Quantity; // Cộng hoàn lại số lượng vào kho hàng
                        }
                    }
                }

                order.PaymentStatus = status; // Gán trạng thái mới (Đã thanh toán / Đang giao hàng / Đã hủy)
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
            }
            // Tải lại giao diện bảng quản trị sau khi xử lý thành công
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
