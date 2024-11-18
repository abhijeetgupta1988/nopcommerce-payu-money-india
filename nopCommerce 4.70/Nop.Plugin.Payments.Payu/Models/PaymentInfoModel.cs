using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.Payu.Models
{
    public record PaymentInfoModel : BaseNopModel
    {
        public PaymentInfoModel() { }
    }
}