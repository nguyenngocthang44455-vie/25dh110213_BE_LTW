using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _25dh110213_BE_LTW.Models.ViewModel
{
    public class CartItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string Category { get; set; }

        // Tính tổng tiền của sản phẩm này (Số lượng x Đơn giá)
        public decimal TotalPrice
        {
            get { return Quantity * UnitPrice; }
        }
    }
}
