using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _25dh110213_BE_LTW.Models.ViewModel
{
    public class CartService
    {
        private readonly HttpSessionStateBase session;
        private const string CartSessionKey = "Cart";

        // Khởi tạo Service kết nối với Session của hệ thống
        public CartService(HttpSessionStateBase session)
        {
            this.session = session;
        }

        // Lấy giỏ hàng hiện tại ra, nếu chưa có thì tạo mới giỏ trống
        public Cart GetCart()
        {
            var cart = session[CartSessionKey] as Cart;
            if (cart == null)
            {
                cart = new Cart();
                session[CartSessionKey] = cart;
            }
            return cart;
        }

        // Xóa hoàn toàn giỏ hàng khỏi Session
        public void ClearCart()
        {
            session[CartSessionKey] = null;
        }
    }
}