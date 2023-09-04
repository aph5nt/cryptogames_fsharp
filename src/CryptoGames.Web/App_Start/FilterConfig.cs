using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CryptoGames.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }

    public class UrlAuthorize : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var isAuthorized = base.AuthorizeCore(httpContext);
            if (!isAuthorized)
            {
                return false;
            }

            // true is exists??
            return true;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var x = filterContext.RequestContext.RouteData;


            base.OnAuthorization(filterContext);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // this will create new user!
            filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary(
                            new
                            {
                                controller = "Error",
                                action = "Unauthorised"
                            })
                        );
        }
    }
}
