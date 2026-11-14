using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList.Mvc;

namespace _25dh110213_BE_LTW.Models.ViewModel
{
    public class ProductDetailsVM
    {
        public Product product { get; set; }
        public int quantity { get; set; } = 1; // Lưu giá trị tạm thời
        public decimal estimatedValue { get; set; } // = quantity * product.ProductPrice

        // Các thuộc tính hỗ trợ phân trang
        public int PageNumber { get; set; } // Trang hiện tại
        public int PageSize { get; set; } // Số sản phẩm mỗi trang

        // Danh sách sản phẩm cùng danh mục
        // public PagedList.IPagedList<Product> RelatedProducts { get; set; }
        public List<Product> RelatedProducts { get; set; }

        // Danh sách sản phẩm bán chạy nhất cùng danh mục
        public PagedList.IPagedList<Product> TopProducts { get; set; }
    }
}