using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
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
            // Sử dụng .Include để nạp kèm thông tin khách hàng sở hữu đơn hàng
            var orders = db.Orders.Include(o => o.Customer).OrderByDescending(o => o.OrderDate).ToList();
            return View(orders);
        }
    }
}