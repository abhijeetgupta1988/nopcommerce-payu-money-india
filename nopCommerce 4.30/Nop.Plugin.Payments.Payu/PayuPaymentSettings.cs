using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.Payu
{
    public class PayuPaymentSettings : ISettings
    {
        public string MerchantId { get; set; }
        public string Key { get; set; }
        public string MerchantParam { get; set; }
        public string PayUri { get; set; }
        public decimal AdditionalFee { get; set; }
    }
}
