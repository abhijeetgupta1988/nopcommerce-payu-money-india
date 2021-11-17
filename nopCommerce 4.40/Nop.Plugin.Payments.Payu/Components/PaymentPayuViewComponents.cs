using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Payments.Payu.Models;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.Payu.Components
{
    [ViewComponent(Name = "PaymentPayU")]

    public class PaymentPayuViewComponents : NopViewComponent
    {

        public IViewComponentResult Invoke()
        {
            var model = new PaymentInfoModel();
            return View("~/Plugins/Payments.Payu/Views/PaymentInfo.cshtml",model);
        }
    }
}
