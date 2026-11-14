using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;

namespace _25dh110213_BE_LTW.Models
{
    public partial class Product
    {
        [NotMapped]
        // Bổ sung kiểm tra định dạng: Chỉ chấp nhận các đuôi ảnh phổ biến
        [RegularExpression(@"^([a-zA-Z0-9\s_\\.\-:])+(.png|.jpg|.gif|.PNG|.JPG|.GIF)$",
            ErrorMessage = "Chỉ chấp nhận các định dạng ảnh PNG, JPG và GIF")]
        public HttpPostedFileBase UploadImg { get; set; }
    }
}
