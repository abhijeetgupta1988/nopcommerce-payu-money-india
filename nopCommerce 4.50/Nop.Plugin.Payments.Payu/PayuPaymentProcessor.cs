using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;

using Nop.Plugin.Payments.Payu.Controllers;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Web.Framework;

namespace Nop.Plugin.Payments.Payu
{
    /// <summary>
    /// Payu payment processor
    /// </summary>
    public class PayuPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields
        private readonly IAddressService _addressService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICountryService _countryService;
        private readonly PayuPaymentSettings _PayuPaymentSettings;
        private readonly ISettingService _settingService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly IWebHelper _webHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        #endregion

        #region Ctor

        public PayuPaymentProcessor(PayuPaymentSettings PayuPaymentSettings, IAddressService addressService,
              IStateProvinceService stateProvinceService,
              ICountryService countryService,
            ISettingService settingService, ICurrencyService currencyService,
              ILocalizationService localizationService, IPaymentService paymentService,
            CurrencySettings currencySettings, IWebHelper webHelper, IHttpContextAccessor httpContextAccessor,
             IOrderTotalCalculationService orderTotalCalculationService)
        {
            _addressService = addressService;
            _stateProvinceService = stateProvinceService;
            _countryService = countryService;
            _paymentService = paymentService;
            this._localizationService = localizationService;
            this._PayuPaymentSettings = PayuPaymentSettings;
            this._settingService = settingService;
            this._currencyService = currencyService;
            this._currencySettings = currencySettings;
            this._webHelper = webHelper;
            _httpContextAccessor = httpContextAccessor;
            _orderTotalCalculationService = orderTotalCalculationService;
        }

        #endregion

        #region Utilities

        #endregion

        #region Methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.NewPaymentStatus = PaymentStatus.Pending;
            return Task.FromResult(result);
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public async Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {

            //choosing correct order address
            var orderAddress = await _addressService.GetAddressByIdAsync(
                (postProcessPaymentRequest.Order.PickupInStore ? postProcessPaymentRequest.Order.PickupAddressId : postProcessPaymentRequest.Order.ShippingAddressId) ?? 0);
            var orderShippingAddress = await _addressService.GetAddressByIdAsync((int)postProcessPaymentRequest.Order.ShippingAddressId);

            var myUtility = new PayuHelper();
            var orderId = postProcessPaymentRequest.Order.Id;

            // var remotePostHelper = new RemotePost();

            var remotePostHelper = new RemotePost(_httpContextAccessor, _webHelper);


            remotePostHelper.FormName = "PayuForm";
            remotePostHelper.Url = _PayuPaymentSettings.PayUri;
            remotePostHelper.Add("key", _PayuPaymentSettings.MerchantId.ToString());
            remotePostHelper.Add("amount", postProcessPaymentRequest.Order.OrderTotal.ToString(new CultureInfo("en-US", false).NumberFormat));
            remotePostHelper.Add("productinfo", "productinfo");
            remotePostHelper.Add("Currency", (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId)).CurrencyCode);
            remotePostHelper.Add("Order_Id", orderId.ToString());
            remotePostHelper.Add("txnid", orderId.ToString());
            remotePostHelper.Add("service_provider", "payu_paisa");
            remotePostHelper.Add("surl", _webHelper.GetStoreLocation(false) + "Plugins/PaymentPayu/Return");
            remotePostHelper.Add("furl", _webHelper.GetStoreLocation(false) + "Plugins/PaymentPayu/Return");
            remotePostHelper.Add("hash", myUtility.getchecksum(_PayuPaymentSettings.MerchantId.ToString(),
                postProcessPaymentRequest.Order.Id.ToString(), postProcessPaymentRequest.Order.OrderTotal.ToString(new CultureInfo("en-US", false).NumberFormat),
                "productinfo", orderAddress?.FirstName,
               orderAddress?.Email.ToString(), _PayuPaymentSettings.Key));


            //Billing details
            remotePostHelper.Add("firstname", orderAddress?.FirstName.ToString());
            remotePostHelper.Add("billing_cust_address", orderAddress?.Address1);
            remotePostHelper.Add("phone", orderAddress?.PhoneNumber);
            remotePostHelper.Add("email", orderAddress?.Email.ToString());
            remotePostHelper.Add("billing_cust_city", orderAddress?.City);
            var billingStateProvince = (await _stateProvinceService.GetStateProvinceByAddressAsync(orderAddress))?.Abbreviation;
            if (billingStateProvince != null)
                remotePostHelper.Add("billing_cust_state", (await _stateProvinceService.GetStateProvinceByAddressAsync(orderAddress))?.Abbreviation);
            else
                remotePostHelper.Add("billing_cust_state", "");
            remotePostHelper.Add("billing_zip_code", orderAddress?.ZipPostalCode);
            var billingCountry = (await _countryService.GetCountryByAddressAsync(orderAddress))?.TwoLetterIsoCode;
            if (billingCountry != null)
                remotePostHelper.Add("billing_cust_country", (await _countryService.GetCountryByAddressAsync(orderAddress))?.TwoLetterIsoCode);
            else
                remotePostHelper.Add("billing_cust_country", "");

            //Delivery details

            if (postProcessPaymentRequest.Order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            {
                remotePostHelper.Add("delivery_cust_name", orderShippingAddress.FirstName);
                remotePostHelper.Add("delivery_cust_address", orderShippingAddress.Address1);
                remotePostHelper.Add("delivery_cust_notes", string.Empty);
                remotePostHelper.Add("delivery_cust_tel", orderShippingAddress.PhoneNumber);
                remotePostHelper.Add("delivery_cust_city", orderShippingAddress.City);
                var deliveryStateProvince = await _stateProvinceService.GetStateProvinceByAddressAsync(orderShippingAddress);
                if (deliveryStateProvince != null)
                    remotePostHelper.Add("delivery_cust_state", deliveryStateProvince.Abbreviation);
                else
                    remotePostHelper.Add("delivery_cust_state", "");
                remotePostHelper.Add("delivery_zip_code", orderShippingAddress.ZipPostalCode);
                var deliveryCountry = await _countryService.GetCountryByAddressAsync(orderShippingAddress); // postProcessPaymentRequest.Order.ShippingAddress.Country;
                if (deliveryCountry != null)
                    remotePostHelper.Add("delivery_cust_country", deliveryCountry.ThreeLetterIsoCode);
                else
                    remotePostHelper.Add("delivery_cust_country", "");
            }

            //  remotePostHelper.Add("Merchant_Param", _PayuPaymentSettings.MerchantParam);
            remotePostHelper.Post();
        }



        //Hide payment begins

        public Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return Task.FromResult(false);
        }

        //hide payment ends

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
           /*
            return await _orderTotalCalculationService.CalculateAdditionalFeeAsync(cart,
               _PayuPaymentSettings.AdditionalFee, _PayuPaymentSettings.AdditionalFeePercentage);
           */
            return await _orderTotalCalculationService.CalculatePaymentAdditionalFeeAsync(cart,
               _PayuPaymentSettings.AdditionalFee, _PayuPaymentSettings.AdditionalFeePercentage);
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();
            result.AddError("Capture method not supported");


            return Task.FromResult(result);
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            result.AddError("Refund method not supported");
            return Task.FromResult(result);
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            result.AddError("Void method not supported");
            return Task.FromResult(result);
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>

        public Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AddError("Recurring payment not supported");
            return Task.FromResult(result);
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError("Recurring payment not supported");
            return Task.FromResult(result);
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public Task<bool> CanRePostProcessPaymentAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            //Payu is the redirection payment method
            //It also validates whether order is also paid (after redirection) so customers will not be able to pay twice

            //payment status should be Pending
            if (order.PaymentStatus != PaymentStatus.Pending)
                return Task.FromResult(false);

            //let's ensure that at least 1 minute passed after order is placed
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalMinutes < 1)
                return Task.FromResult(false);

            return Task.FromResult(true);
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentPayU/Configure";
        }

        public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            var warnings = new List<string>();
            return Task.FromResult<IList<string>>(new List<string>());

        }

        public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            return Task.FromResult(new ProcessPaymentRequest());
        }


        public string GetPublicViewComponentName()
        {
            return "PaymentPayU";
        }

        /*
       
        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "PaymentPayu";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.Payu.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Gets a route for payment info
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "PaymentPayu";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.Payu.Controllers" }, { "area", null } };
        }


        */
        public Type GetControllerType()
        {
            return typeof(PaymentPayuController);
        }

        public override async Task InstallAsync()
        {
            var settings = new PayuPaymentSettings()
            {
                MerchantId = "",
                Key = "",
                MerchantParam = "",
                PayUri = "https://sandboxsecure.payu.in/_payment",
                AdditionalFee = 0,
            };
            await _settingService.SaveSettingAsync(settings);

            //locales
            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Payments.Payu.RedirectionTip"] = "You will be redirected to Payu site to complete the order.",
                ["Plugins.Payments.Payu.Fields.MerchantId"] = "Key",
                ["Plugins.Payments.Payu.Fields.MerchantId.Hint"] = "Enter Key.",
                ["Plugins.Payments.Payu.Fields.Key"] = "Salt",
                ["Plugins.Payments.Payu.Fields.Key.Hint"] = "Enter salt.",
                ["Plugins.Payments.Payu.Fields.MerchantParam"] = "Merchant Param",
                ["Plugins.Payments.Payu.Fields.MerchantParam.Hint"] = "Enter merchant param.",
                ["Plugins.Payments.Payu.Fields.PayUri"] = "Pay URI",
                ["Plugins.Payments.Payu.Fields.PayUri.Hint"] = "Enter Pay URI.",
                ["Plugins.Payments.Payu.Fields.AdditionalFee"] = "Additional fee",
                ["Plugins.Payments.Payu.Fields.AdditionalFee.Hint"] = "Enter additional fee to charge your customers.",
                ["Plugins.Payments.Payu.Fields.AdditionalFeePercentage"] = "Additional fee.Use percentage",
                ["Plugins.Payments.Payu.Fields.AdditionalFeePercentage.Hint"] = "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.",


            });


            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<PayuPaymentSettings>();

            //locales
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.Payments.Payu");

            await base.UninstallAsync();
        }
        #endregion

        #region Properies

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture => false;


        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund => false;


        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund => false;


        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid => false;


        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;


        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;



        public bool SkipPaymentInfo => false;




        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            //return description of this payment method to be display on "payment method" checkout step. good practice is to make it localizable
            //for example, for a redirection payment method, description may be like this: "You will be redirected to PayU site to complete the payment"
            return await _localizationService.GetResourceAsync("Plugins.Payments.PayU.RedirectionTip");
        }

        /*
        public void GetPublicViewComponent(out string viewComponentName)
        {
            viewComponentName = "PaymentPayU";
        }

        public IList<string> ValidatePaymentForm(IFormCollection form)
        {
            var warnings = new List<string>();
            return warnings;
        }

        public ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            return paymentInfo;
        }

    */
        #endregion
    }
}
