using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
namespace Nop.Plugin.Payments.Payu.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Payu.Fields.MerchantId")]
        public string MerchantId { get; set; }
        public bool MerchantId_OverrideForStore { get; set; }


        [NopResourceDisplayName("Plugins.Payments.Payu.Fields.Key")]
        public string Key { get; set; }
        public bool Key_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Payu.Fields.MerchantParam")]
        public string MerchantParam { get; set; }
        public bool MerchantParam_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Payu.Fields.PayUri")]
        public string PayUri { get; set; }
        public bool PayUri_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Payu.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Payu.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }
    }
}