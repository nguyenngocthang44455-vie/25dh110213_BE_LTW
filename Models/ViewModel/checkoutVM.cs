using _25dh110213_BE_LTW.Models;
using _25dh110213_BE_LTW.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace _25dh110213_BE_LTW.Models.ViewModel
{
    public class CheckoutVM
    {
        public List<CartItem> CartItems { get; set; } // Danh sách sản phẩm trong giỏ
        public int CustomerID { get; set; }

        [Display(Name = "Họ tên khách hàng")]
        public string CustomerName { get; set; }

        [Display(Name = "Số điện thoại")]
        public string CustomerPhone { get; set; }

        [Display(Name = "Ngày đặt hàng")]
        public DateTime OrderDate { get; set; }

        [Display(Name = "Tổng giá trị")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Trạng thái thanh toán")]
        public string PaymentStatus { get; set; }

        [Display(Name = "Phương thức thanh toán")]
        public string PaymentMethod { get; set; }

        [Display(Name = "Phương thức giao hàng")]
        public string ShippingMethod { get; set; }

        [Display(Name = "Địa chỉ giao hàng")]
        public string ShippingAddress { get; set; }

        public string Username { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }
    }
}
