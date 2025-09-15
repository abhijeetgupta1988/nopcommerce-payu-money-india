using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.Payu
{
    public class PayuPaymentSettings : ISettings
    {
        public string MerchantId { get; set; }
        public string Key { get; set; }
        public string PayUri { get; set; }
        public decimal AdditionalFee { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to "additional fee" is specified as percentage. true - percentage, false - fixed value.
        /// </summary>
        public bool AdditionalFeePercentage { get; set; }
    }
}
