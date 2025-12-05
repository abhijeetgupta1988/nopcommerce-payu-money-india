using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.Payu;

public partial class RouteProvider : IRouteProvider
{
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        //Return Endpoint
        endpointRouteBuilder.MapControllerRoute("Plugin.Payments.Payu.Return", "Plugins/PaymentPayu/Return",
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
