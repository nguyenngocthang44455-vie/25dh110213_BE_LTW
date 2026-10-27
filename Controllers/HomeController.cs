using _25dh110213_BE_LTW.Models.ViewModel;
using _25dh110213_BE_LTW.Models;
using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;

namespace _25dh110213_BE_LTW.Controllers
{
    public class HomeController : Controller
    {
        private My_StoreEntities db = new My_StoreEntities();

        // Chỉnh sửa action Index: truyền tham số searchTerm và page
        public ActionResult Index(string searchTerm, int? page)
        {
            var model = new HomeProductVM();
            var products = db.Products.AsQueryable();

            // Tìm kiếm sản phẩm dựa trên từ khóa tìm kiếm
            if (!string.IsNullOrEmpty(searchTerm))
            {
                model.SearchTerm = searchTerm;
                products = products.Where(p => p.ProductName.Contains(searchTerm) ||
                                              p.ProductDescription.Contains(searchTerm) ||
                                              p.Category.CategoryName.Contains(searchTerm));
            }

            // Đoạn mã lấy danh sách sản phẩm nổi bật
            // Lấy 10 sản phẩm bán chạy nhất dựa trên số lượng trong Chi tiết đơn hàng
            model.FeaturedProducts = products.OrderByDescending(p => p.OrderDetails.Sum(d => d.Quantity)).Take(10).ToList();

            // Đoạn mã lấy danh sách sản phẩm mới có phân trang
            int pageNumber = page ?? 1;
            int pageSize = 20; // 20 sản phẩm mỗi trang

            model.NewProducts = products.OrderByDescending(p => p.ProductID).ToPagedList(pageNumber, pageSize);

            return View(model);
        }

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

            // Lấy tất cả các sản phẩm cùng danh mục
            var products = db.Products.Where(p => p.CategoryID == pro.CategoryID && p.ProductID != pro.ProductID).AsQueryable();

            ProductDetailsVM model = new ProductDetailsVM();

            // Đoạn code liên quan tới phân trang
            // Lấy số trang hiện tại (mặc định là trang 1 nếu không có giá trị)
            int pageNumber = page ?? 1;
            int pageSize = 3; // 3 sản phẩm mỗi trang

            model.product = pro;
            model.RelatedProducts = products.OrderByDescending(p => p.ProductID).Take(6).ToList();
            model.TopProducts = products.OrderByDescending(p => p.OrderDetails.Sum(d => d.Quantity)).ToPagedList(pageNumber, pageSize);

            // Cập nhật quantity
            model.quantity = quantity;
            model.estimatedValue = quantity * pro.ProductPrice;

            return View(model);
        }

    }
}