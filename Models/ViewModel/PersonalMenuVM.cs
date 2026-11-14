using System;
using System.Collections.Generic;

namespace _25dh110213_BE_LTW.Models.ViewModel
{
    public class PersonalMenuVM
    {
        public bool IsLoggedIn { get; set; }     // Trạng thái đã đăng nhập hay chưa
        public string Username { get; set; }     // Tên tài khoản hiển thị
        public int CartCount { get; set; }       // Số lượng sản phẩm hiển thị trên icon giỏ hàng
    }
}
