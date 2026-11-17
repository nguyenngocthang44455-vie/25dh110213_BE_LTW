using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace _25dh110213_BE_LTW.Filters
{
    public class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var username = httpContext.Session["Username"];
            var role = httpContext.Session["UserRole"] != null
                       ? httpContext.Session["UserRole"].ToString().Trim().ToUpper()
                       : "";

            return username != null && role == "A";
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary(new
                {
                    area = "",
                    controller = "Account",
                    action = "Login"
                })
            );
        }
    }
}
