using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;


namespace Nop.Plugin.Payments.Payu
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {

            /*
            routeBuilder.MapRoute("Plugin.Payments.Payu.Configure",
                 "Plugins/PaymentPayu/Configure",
                 new { controller = "PaymentPayu", action = "Configure" });

            //routeBuilder.MapRoute("Plugin.Payments.Payu.Configure", "Plugins/Payments.Payu/Views/Configure",
            //     new { controller = "PaymentPayu", action = "Configure" });

            routeBuilder.MapRoute("Plugin.Payments.Payu.PaymentInfo",
                 "Plugins/PaymentPayu/PaymentInfo",
                 new { controller = "PaymentPayu", action = "PaymentInfo" });
            */
            //Return
            routeBuilder.MapRoute("Plugin.Payments.Payu.Return",
                 "Plugins/PaymentPayu/Return",
                 new { controller = "PaymentPayu", action = "Return" });
            
            

    
        }
        public int Priority
        {
            get
            {
                return -1;
            }
        }
    }
}
