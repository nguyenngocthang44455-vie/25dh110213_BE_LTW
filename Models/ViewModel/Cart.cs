using System;
using System.Collections.Generic;
using System.Linq;
using PagedList;

namespace _25dh110213_BE_LTW.Models.ViewModel
{
    public class Cart
    {
        // 1. Danh sách nội bộ dùng để chỉnh sửa thêm/xóa/sửa (Viết chữ thường)
        private List<CartItem> items = new List<CartItem>();

        // 2. Thuộc tính public để View đọc dữ liệu (Dạng IEnumerable chỉ đọc)
        public IEnumerable<CartItem> Items => items;

        // Gom nhóm sản phẩm theo danh mục sản phẩm (Theo đúng slide)
        public List<IGrouping<string, CartItem>> GroupedItems => items.GroupBy(i => i.Category).ToList();

        // Các thuộc tính hỗ trợ phân trang (Theo đúng slide)
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        // Danh sách các sản phẩm cùng danh mục (Theo đúng slide)
        public IPagedList<Product> SimilarProducts { get; set; }

        // CHỨC NĂNG 1: Thêm sản phẩm vào giỏ (Đã sửa đổi để nhận biến category)
        public void AddItem(int productId, string productImage, string productName, decimal unitPrice, int quantity, string category)
        {
            // Kiểm tra trên danh sách nội bộ 'items'
            var existingItem = items.FirstOrDefault(i => i.ProductID == productId);

            if (existingItem == null)
            {
                items.Add(new CartItem
                {
                    ProductID = productId,
                    ProductImage = productImage,
                    ProductName = productName,
                    UnitPrice = unitPrice,
                    Quantity = quantity,
                    Category = category // Gán giá trị category truyền vào
                });
            }
            else
            {
                existingItem.Quantity += quantity;
            }
        }

        // CHỨC NĂNG 2: Xóa sản phẩm khỏi giỏ
        public void RemoveItem(int productId)
        {
            // Thực hiện xóa trên danh sách nội bộ 'items'
            items.RemoveAll(i => i.ProductID == productId);
        }

        // CHỨC NĂNG 3: Tính tổng thành tiền của toàn bộ giỏ hàng
        public decimal TotalValue()
        {
            return items.Sum(i => i.TotalPrice);
        }

        // CHỨC NĂNG 4: Làm trống giỏ hàng
        public void Clear()
        {
            items.Clear();
        }

        // CHỨC NĂNG 5: Cập nhật số lượng của sản phẩm đã chọn
        public void UpdateQuantity(int productId, int quantity)
        {
            var item = items.FirstOrDefault(i => i.ProductID == productId);
            if (item != null)
            {
                if (quantity <= 0)
                {
                    items.Remove(item);
                }
                else
                {
                    item.Quantity = quantity;
                }
            }
        }
    }
}
