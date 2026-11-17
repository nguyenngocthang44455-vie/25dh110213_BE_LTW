using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using _25dh110213_BE_LTW.Models;
using _25dh110213_BE_LTW.Filters;

namespace _25dh110213_BE_LTW.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class OrdersController : Controller
    {
        private My_StoreEntities db = new My_StoreEntities();

        // GET: Admin/Orders
        public ActionResult Index()
        {
            var orders = db.Orders.Include(o => o.Customer).OrderByDescending(o => o.OrderDate).ToList();
            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string status)
        {
            var order = db.Orders.Find(id);
            if (order != null)
            {
                if (status == "Đã hủy" && order.PaymentStatus != "Đã hủy")
                {
                    var details = db.OrderDetails.Where(d => d.OrderID == id).ToList();
                    foreach (var item in details)
                    {
                        var product = db.Products.Find(item.ProductID);
                        if (product != null)
                        {
                            product.Quantity += item.Quantity;
                        }
                    }
                }

                order.PaymentStatus = status;
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
            }
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
