using _25dh110213_BE_LTW.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace _25dh110213_BE_LTW.Areas.Admin.Controllers
{
    public class CustomersController : Controller
    {
        private My_StoreEntities db = new My_StoreEntities();

        // GET: Admin/Customers
        public ActionResult Index()
        {
            var customers = db.Customers.ToList();
            return View(customers);
        }
    }
}