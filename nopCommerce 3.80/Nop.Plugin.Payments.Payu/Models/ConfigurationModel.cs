using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Payments.Payu.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Payu.MerchantId")]
        public string MerchantId { get; set; }
        public bool MerchantId_OverrideForStore { get; set; }


        [NopResourceDisplayName("Plugins.Payments.Payu.Key")]
        public string Key { get; set; }
        public bool Key_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Payu.MerchantParam")]
        public string MerchantParam { get; set; }
        public bool MerchantParam_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Payu.PayUri")]
        public string PayUri { get; set; }
        public bool PayUri_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Payu.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }
    }
}