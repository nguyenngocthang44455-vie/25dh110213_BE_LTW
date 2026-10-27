using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using _25dh110213_BE_LTW.Models;
using _25dh110213_BE_LTW.Models.ViewModel;
using PagedList;

namespace _25dh110213_BE_LTW.Areas.Admin.Controllers
{
    public class ProductsController : Controller
    {
        private My_StoreEntities db = new My_StoreEntities();

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
        public ActionResult Create([Bind(Include = "ProductID,CategoryID,ProductName,ProductDescription,ProductPrice")] Product product)
        {
            // CỐ ĐỊNH: Loại bỏ hoàn toàn ProductImage khỏi Validation vì ảnh sẽ xử lý thủ công dưới đây
            ModelState.Remove("ProductImage");

            if (ModelState.IsValid)
            {
                // 1. Đọc file từ Request theo thuộc tính 'name="UploadImg"' của thẻ input
                HttpPostedFileBase file = Request.Files["UploadImg"];

                if (file != null && file.ContentLength > 0)
                {
                    // 2. Kiểm tra định dạng đuôi file để đảm bảo chỉ chấp nhận tệp ảnh công nghệ
                    string extension = System.IO.Path.GetExtension(file.FileName).ToLower();
                    string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("", "Chỉ chấp nhận các định dạng ảnh (.jpg, .jpeg, .png, .gif, .webp)");
                        ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
                        return View(product);
                    }

                    // 3. Tối ưu tên file: Đổi tên file để tránh trùng lặp bằng cách đính kèm chuỗi thời gian (Timestamp)
                    string rawFileName = System.IO.Path.GetFileNameWithoutExtension(file.FileName);
                    string uniqueFileName = rawFileName + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + extension;

                    // 4. Thiết lập đường dẫn lưu trữ
                    string folderPath = "/Content/images/"; // Bỏ dấu '~' để hiển thị mượt mà trên thẻ <img src> ngoài View
                    string serverPath = Server.MapPath("~" + folderPath);

                    // Tự động tạo thư mục nếu chưa tồn tại trên Server
                    if (!System.IO.Directory.Exists(serverPath))
                    {
                        System.IO.Directory.CreateDirectory(serverPath);
                    }

                    // 5. Tiến hành lưu file vật lý vào ổ cứng server và cập nhật thuộc tính Model
                    string physicalPath = System.IO.Path.Combine(serverPath, uniqueFileName);
                    file.SaveAs(physicalPath);

                    product.ProductImage = folderPath + uniqueFileName; // Lưu chuỗi /Content/images/ten_anh_2026...jpg vào DB
                }
                else
                {
                    // Báo lỗi nếu việc chọn hình ảnh là bắt buộc đối với sản phẩm mới
                    ModelState.AddModelError("", "Vui lòng chọn hình ảnh minh họa cho sản phẩm.");
                    ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
                    return View(product);
                }

                // 6. Thực hiện lưu thông tin sản phẩm vào Cơ sở dữ liệu
                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // Nếu ModelState không hợp lệ, nạp lại DropdownList danh mục cho View hiển thị lỗi
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
        public ActionResult Edit([Bind(Include = "ProductID,CategoryID,ProductName,ProductDescription,ProductPrice,ProductImage")] Product product)
        {
            // BỎ QUA bắt lỗi của trường dữ liệu chuỗi ProductImage gốc để tránh nghẽn Form
            ModelState.Remove("ProductImage");

            if (ModelState.IsValid)
            {
                // Đọc file trực tiếp từ Request gửi lên từ trình duyệt để chống bị rỗng nhận diện
                HttpPostedFileBase file = Request.Files["UploadImg"];

                if (file != null && file.ContentLength > 0)
                {
                    string filename = System.IO.Path.GetFileName(file.FileName);
                    string savePath = "~/Content/images/";
                    product.ProductImage = savePath + filename;

                    string serverPath = Server.MapPath(savePath);
                    if (!System.IO.Directory.Exists(serverPath))
                    {
                        System.IO.Directory.CreateDirectory(serverPath);
                    }
                    file.SaveAs(System.IO.Path.Combine(serverPath, filename));
                }
                else
                {
                    // Nếu không tải ảnh mới, tìm lại đường dẫn cũ trong DB để giữ lại dữ liệu
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
            db.Products.Remove(product);
            db.SaveChanges();
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
