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

        private CartService GetCartService()
        {
            return new CartService(Session);
        }

        // GET: Cart/Index
        public ActionResult Index()
        {
            var cartService = GetCartService();
            var cart = cartService.GetCart();

            // Truyền danh sách số lượng tồn kho của các sản phẩm trong giỏ sang View để khóa nếu nhập lố
            var stockDict = new Dictionary<int, int>();
            if (cart != null && cart.Items != null)
            {
                var productIds = cart.Items.Select(i => i.ProductID).ToList();
                stockDict = db.Products
                    .Where(p => productIds.Contains(p.ProductID))
                    .ToDictionary(p => p.ProductID, p => p.Quantity);
            }
            ViewBag.StockDict = stockDict;

            return View(cart);
        }

        // 2. Action: Thêm sản phẩm vào giỏ hàng
        public ActionResult AddToCart(int id, int quantity = 1)
        {
            var product = db.Products.FirstOrDefault(p => p.ProductID == id);

            if (product != null)
            {
                var cartService = GetCartService();
                var cart = cartService.GetCart();

                var existingItem = cart.Items.FirstOrDefault(i => i.ProductID == id);
                int currentQtyInCart = existingItem != null ? existingItem.Quantity : 0;

                if (currentQtyInCart + quantity > product.Quantity)
                {
                    TempData["StockError"] = $"Số lượng yêu cầu vượt quá số lượng hàng có sẵn! (Kho chỉ còn {product.Quantity} sản phẩm).";
                    return RedirectToAction("ProductDetails", "Home", new { id = id });
                }

                cart.AddItem(
                    product.ProductID,
                    product.ProductImage,
                    product.ProductName,
                    product.ProductPrice,
                    quantity,
                    product.Category != null ? product.Category.CategoryName : "Chưa phân loại"
                );
            }
            return RedirectToAction("Index");
        }

        // 3. Action: Xóa sản phẩm khỏi giỏ hàng
        public ActionResult RemoveFromCart(int id)
        {
            var cartService = GetCartService();
            var cart = cartService.GetCart();

            cart.RemoveItem(id);

            return RedirectToAction("Index");
        }

        // 4. Action: Làm trống toàn bộ giỏ hàng
        public ActionResult ClearCart()
        {
            GetCartService().ClearCart();
            return RedirectToAction("Index");
        }

        // 5. Action: Cập nhật số lượng sản phẩm trong giỏ hàng
        public ActionResult UpdateQuantity(int id, int quantity)
        {
            var product = db.Products.FirstOrDefault(p => p.ProductID == id);
            if (product == null)
            {
                return HttpNotFound("Sản phẩm không tồn tại.");
            }

            if (quantity <= 0)
            {
                quantity = 1;
            }

            if (quantity > product.Quantity)
            {
                quantity = product.Quantity;
                TempData["StockError"] = $"Sản phẩm '{product.ProductName}' chỉ còn tối đa {product.Quantity} sản phẩm trong kho.";
            }

            var cartService = GetCartService();
            var cart = cartService.GetCart();

            cart.UpdateQuantity(id, quantity);

            return RedirectToAction("Index");
        }
    }
}
