using _25dh110213_BE_LTW.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList.Mvc;

namespace _25dh110213_BE_LTW.Models.ViewModel
{
    public class HomeProductVM
    {

        // Tiêu chí để search theo tên, mô tả sp
        // hoặc loại sản phẩm
        public string SearchTerm { get; set; }

        // Các thuộc tính hỗ trợ phân trang
        public int PageNumber { get; set; } // trang hiện tại
        public int PageSize { get; set; } // số sản phẩm mỗi trang

        // Danh sách sản phẩm nổi bật
        public List<Product> FeaturedProducts { get; set; }

        // Danh sách sản phẩm mới có phân trang
        public PagedList.IPagedList<Product> NewProducts { get; set; }

    }
}
