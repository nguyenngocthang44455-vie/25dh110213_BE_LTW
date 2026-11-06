using _25dh110213_BE_LTW.Models;
using _25dh110213_BE_LTW.Models.ViewModel;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace _25dh110213_BE_LTW.Controllers
{
    public class HomeController : Controller
    {
        private My_StoreEntities db = new My_StoreEntities();

        public ActionResult Index(string searchTerm, int? page)
        {
            var model = new HomeProductVM();
            var products = db.Products.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                model.SearchTerm = searchTerm;
                products = products.Where(p => p.ProductName.Contains(searchTerm) ||
                                              p.ProductDescription.Contains(searchTerm) ||
                                              p.Category.CategoryName.Contains(searchTerm));
            }

            model.FeaturedProducts = products.OrderByDescending(p => p.OrderDetails.Sum(d => (int?)d.Quantity) ?? 0).Take(10).ToList();

            int pageNumber = page ?? 1;
            int pageSize = 20;

            model.NewProducts = products.OrderByDescending(p => p.ProductID).ToPagedList(pageNumber, pageSize);

            return View(model);
        }

        // Đã sửa: Đổi tên Action thành ProductDetails để khớp chuẩn với route của Views
        public ActionResult ProductDetails(int? id, int quantity = 1, int? page = 1)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Product pro = db.Products.Find(id);
            if (pro == null)
            {
                return HttpNotFound();
            }

            var products = db.Products.Where(p => p.CategoryID == pro.CategoryID && p.ProductID != pro.ProductID).AsQueryable();

            ProductDetailsVM model = new ProductDetailsVM();

            int pageNumber = page ?? 1;
            int pageSize = 3;

            model.product = pro;
            model.RelatedProducts = products.OrderByDescending(p => p.ProductID).Take(6).ToList();
            model.TopProducts = products.OrderByDescending(p => p.OrderDetails.Sum(d => (int?)d.Quantity) ?? 0).ToPagedList(pageNumber, pageSize);

            model.quantity = quantity;
            model.estimatedValue = quantity * pro.ProductPrice;

            return View(model);
        }

        [ChildActionOnly]
        public ActionResult CategoryMenu()
        {
            var categories = db.Categories.ToList();
            return PartialView("_CategoriesPV", categories);
        }

        public ActionResult ProductList(int? id, int? page)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryName = category.CategoryName;

            var products = db.Products.Where(p => p.CategoryID == id).OrderByDescending(p => p.ProductID);

            int pageNumber = page ?? 1;
            int pageSize = 6;

            return View(products.ToPagedList(pageNumber, pageSize));
        }

        // Action tìm kiếm mở rộng theo tên và khoảng giá sử dụng ProductSearchVM
        public ActionResult SearchProducts(ProductSearchVM model)
        {
            var products = db.Products.AsQueryable();

            if (!string.IsNullOrEmpty(model.SearchTerm))
            {
                products = products.Where(p => p.ProductName.Contains(model.SearchTerm) ||
                                              p.ProductDescription.Contains(model.SearchTerm) ||
                                              p.Category.CategoryName.Contains(model.SearchTerm));
            }

            if (model.MinPrice.HasValue)
            {
                products = products.Where(p => p.ProductPrice >= model.MinPrice.Value);
            }

            if (model.MaxPrice.HasValue)
            {
                products = products.Where(p => p.ProductPrice <= model.MaxPrice.Value);
            }

            switch (model.SortOrder)
            {
                case "price_asc":
                    products = products.OrderBy(p => p.ProductPrice);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.ProductPrice);
                    break;
                default:
                    products = products.OrderByDescending(p => p.ProductID);
                    break;
            }

            int pageNumber = model.PageNumber > 0 ? model.PageNumber : 1;
            int pageSize = model.PageSize > 0 ? model.PageSize : 10;

            model.Products = products.ToPagedList(pageNumber, pageSize);

            return View(model);
        }
    }
}