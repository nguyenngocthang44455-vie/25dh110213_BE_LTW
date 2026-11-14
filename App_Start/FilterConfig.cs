using System;
using System.Web;
using System.Web.Mvc;

namespace _25dh110213_BE_LTW
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());

            // THÊM DÒNG NÀY: Đăng ký bộ lọc chống lưu bộ nhớ đệm (Cache) trên toàn hệ thống
            filters.Add(new NoCacheGlobalFilter());
        }
    }

    // THÊM LỚP NÀY: Định nghĩa bộ lọc ép trình duyệt luôn tải lại trang mới khi quay lui (Back)
    public class NoCacheGlobalFilter : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            var response = filterContext.HttpContext.Response;
            response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
            response.Cache.SetValidUntilExpires(false);
            response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            response.Cache.SetCacheability(HttpCacheability.NoCache);
            response.Cache.SetNoStore();

            base.OnResultExecuting(filterContext);
        }
    }
}
