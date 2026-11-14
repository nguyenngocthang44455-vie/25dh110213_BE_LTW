using _25dh110213_BE_LTW.Models;
using _25dh110213_BE_LTW.Models.ViewModel;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace _25dh110213_BE_LTW.Controllers
{
    public class CartController : Controller
    {
        private My_StoreEntities db = new My_StoreEntities();

        // 1. Hàm bổ trợ: Lấy dịch vụ giỏ hàng (GetCartService)
        private CartService GetCartService()
        {
            return new CartService(Session);
        }

        // GET: Cart
        public ActionResult Index(int? page)
        {
            var cartService = GetCartService();
            var cart = cartService.GetCart();

            var products = db.Products.ToList();
            var similarProducts = new List<Product>();

            if (cart.Items != null && cart.Items.Any())
            {
                similarProducts = products.Where(p =>
                    cart.Items.Any(ci => ci.Category == p.Category.CategoryName) &&
                    !cart.Items.Any(ci => ci.ProductID == p.ProductID)
                ).ToList();
            }

            // Đoạn code liên quan tới phân trang
            int pageNumber = page ?? 1;
            int pageSize = 4; // Số sản phẩm mỗi trang

            cart.PageNumber = pageNumber;
            cart.PageSize = pageSize;

            cart.SimilarProducts = similarProducts.OrderBy(p => p.ProductID).ToPagedList(pageNumber, pageSize);
            return View(cart);
        }

        // 2. Action: Thêm sản phẩm vào giỏ (ĐÃ THÊM LỚP BẢO VỆ CHẶN VƯỢT QUÁ TỒN KHO)
        public ActionResult AddToCart(int id, int quantity = 1)
        {
            var product = db.Products.FirstOrDefault(p => p.ProductID == id);
            if (product == null)
            {
                return HttpNotFound("Sản phẩm không tồn tại.");
            }

            var cartService = GetCartService();
            var cart = cartService.GetCart();

            // 1. Kiểm tra xem sản phẩm này đã có trong giỏ hàng từ trước chưa
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductID == id);
            int currentQtyInCart = existingItem != null ? existingItem.Quantity : 0;

            // 2. Tổng số lượng yêu cầu đặt mua (mới thêm + đã có trong giỏ)
            int totalRequestedQuantity = currentQtyInCart + quantity;

            // 3. Nếu tổng số lượng muốn mua lớn hơn số lượng tồn kho thực tế thì chặn lại
            if (totalRequestedQuantity > product.Quantity)
            {
                // Gửi thông báo lỗi chi tiết ra giao diện thông qua TempData
                TempData["StockError"] = $"Sản phẩm '{product.ProductName}' không đủ số lượng cung ứng. Trong kho chỉ còn {product.Quantity} sản phẩm, bạn đang có {currentQtyInCart} sản phẩm trong giỏ hàng.";

                // Điều hướng ngược lại trang chi tiết sản phẩm để hiển thị thông báo lỗi
                return RedirectToAction("ProductDetails", "Home", new { id = id });
            }

            // 4. Nếu hợp lệ, tiến hành thêm sản phẩm vào giỏ hàng như bình thường
            cart.AddItem(
                product.ProductID,
                product.ProductImage,
                product.ProductName,
                (decimal)product.ProductPrice,
                quantity,
                product.Category != null ? product.Category.CategoryName : "Khác"
            );

            // Quay trở lại trang hiển thị danh sách giỏ hàng công khai
            return RedirectToAction("Index");
        }


        // 3. Action: Xóa sản phẩm khỏi giỏ (RemoveFromCart)
        public ActionResult RemoveFromCart(int id)
        {
            var cartService = GetCartService();
            var cart = cartService.GetCart();

            // Xóa sản phẩm theo ID
            cart.RemoveItem(id);

            return RedirectToAction("Index");
        }

        // 4. Action: Làm trống giỏ hàng (ClearCart)
        public ActionResult ClearCart()
        {
            var cartService = GetCartService();
            cartService.ClearCart();

            return RedirectToAction("Index");
        }

        // 5. Action: Cập nhật số lượng sản phẩm trong giỏ hàng (UpdateQuantity)
        public ActionResult UpdateQuantity(int id, int quantity)
        {
            var cartService = GetCartService();
            var cart = cartService.GetCart();

            // Cập nhật lại số lượng sản phẩm dựa trên ID sản phẩm và số lượng mới từ View gửi lên
            cart.UpdateQuantity(id, quantity);

            return RedirectToAction("Index");
        }
    }
}