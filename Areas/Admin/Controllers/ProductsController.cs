using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using _25dh110213_BE_LTW.Models;
using _25dh110213_BE_LTW.Models.ViewModel;
using PagedList;

namespace _25dh110213_BE_LTW.Areas.Admin.Controllers
{
    // ĐÃ FIX BẢO MẬT: Ép giữ Session đăng nhập mới cho phép quản lý thêm sửa xóa kho sản phẩm
    [Authorize]
    public class ProductsController : Controller
    {
        private My_StoreEntities db = new My_StoreEntities();

        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const string ImageFolder = "/Content/images/";

        private string SaveImage(HttpPostedFileBase file, out string error)
        {
            error = null;
            string extension = System.IO.Path.GetExtension(file.FileName).ToLower();
            if (!AllowedExtensions.Contains(extension))
            {
                error = "Chỉ chấp nhận các định dạng ảnh (.jpg, .jpeg, .png, .gif, .webp)";
                return null;
            }

            string rawFileName = System.IO.Path.GetFileNameWithoutExtension(file.FileName).Replace(" ", "_");
            string uniqueFileName = rawFileName + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + extension;

            string serverPath = Server.MapPath("~" + ImageFolder);
            if (!System.IO.Directory.Exists(serverPath))
            {
                System.IO.Directory.CreateDirectory(serverPath);
            }
            file.SaveAs(System.IO.Path.Combine(serverPath, uniqueFileName));

            return ImageFolder + uniqueFileName;
        }

        // GET: Admin/Products
        public ActionResult Index(string searchTerm, decimal? minPrice,
            decimal? maxPrice, string sortOrder, int? page)
        {
            var model = new ProductSearchVM();
            var products = db.Products.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                model.SearchTerm = searchTerm;
                products = products.Where(p => p.ProductName.Contains(searchTerm) ||
                                               p.ProductDescription.Contains(searchTerm) ||
                                               p.Category.CategoryName.Contains(searchTerm));
            }

            if (minPrice.HasValue)
            {
                model.MinPrice = minPrice.Value;
                products = products.Where(p => p.ProductPrice >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                model.MaxPrice = maxPrice.Value;
                products = products.Where(p => p.ProductPrice <= maxPrice.Value);
            }

            switch (sortOrder)
            {
                case "name_asc":
                    products = products.OrderBy(p => p.ProductName);
                    break;
                case "name_desc":
                    products = products.OrderByDescending(p => p.ProductName);
                    break;
                case "price_asc":
                    products = products.OrderBy(p => p.ProductPrice);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.ProductPrice);
                    break;
                default:
                    products = products.OrderBy(p => p.ProductName);
                    break;
            }

            model.SortOrder = sortOrder;

            int pageNumber = page ?? 1;
            int pageSize = 2;

            model.Products = products.ToPagedList(pageNumber, pageSize);

            return View(model);
        }

        // GET: Admin/Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // GET: Admin/Products/Create
        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
            return View();
        }

        // POST: Admin/Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductID,CategoryID,ProductName,ProductDescription,ProductPrice,Quantity")] Product product)
        {
            ModelState.Remove("ProductImage");

            if (ModelState.IsValid)
            {
                HttpPostedFileBase file = Request.Files["UploadImg"];

                if (file == null || file.ContentLength == 0)
                {
                    ModelState.AddModelError("", "Vui lòng chọn hình ảnh minh họa cho sản phẩm.");
                    ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
                    return View(product);
                }

                string error;
                string imagePath = SaveImage(file, out error);
                if (imagePath == null)
                {
                    ModelState.AddModelError("", error);
                    ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
                    return View(product);
                }

                product.ProductImage = imagePath;
                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // GET: Admin/Products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // POST: Admin/Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductID,CategoryID,ProductName,ProductDescription,ProductPrice,Quantity")] Product product)
        {
            ModelState.Remove("ProductImage");

            if (ModelState.IsValid)
            {
                HttpPostedFileBase file = Request.Files["UploadImg"];

                if (file != null && file.ContentLength > 0)
                {
                    string error;
                    string imagePath = SaveImage(file, out error);
                    if (imagePath == null)
                    {
                        ModelState.AddModelError("", error);
                        ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
                        return View(product);
                    }
                    product.ProductImage = imagePath;
                }
                else
                {
                    db.Configuration.ValidateOnSaveEnabled = false;
                    var oldProduct = db.Products.AsNoTracking().FirstOrDefault(p => p.ProductID == product.ProductID);
                    if (oldProduct != null)
                    {
                        product.ProductImage = oldProduct.ProductImage;
                    }
                }

                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // GET: Admin/Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            try
            {
                db.Products.Remove(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (DbUpdateException)
            {
                db.Entry(product).State = EntityState.Unchanged;
                ViewBag.Error = "Không thể xóa sản phẩm này vì đã có trong đơn hàng.";
                return View("Delete", product);
            }
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
