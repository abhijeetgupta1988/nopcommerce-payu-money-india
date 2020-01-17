using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.Payu.Controllers;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Web.Framework;

namespace Nop.Plugin.Payments.Payu
{
    /// <summary>
    /// Payu payment processor
    /// </summary>
    public class PayuPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly PayuPaymentSettings _PayuPaymentSettings;
        private readonly ISettingService _settingService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public PayuPaymentProcessor(PayuPaymentSettings PayuPaymentSettings,
            ISettingService settingService, ICurrencyService currencyService,
            CurrencySettings currencySettings, IWebHelper webHelper)
        {
            this._PayuPaymentSettings = PayuPaymentSettings;
            this._settingService = settingService;
            this._currencyService = currencyService;
            this._currencySettings = currencySettings;
            this._webHelper = webHelper;
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
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.NewPaymentStatus = PaymentStatus.Pending;
            return result;
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var myUtility = new PayuHelper();
            var orderId= postProcessPaymentRequest.Order.Id;
            var remotePostHelper = new RemotePost();
            remotePostHelper.FormName = "PayuForm";
            remotePostHelper.Url = _PayuPaymentSettings.PayUri;
            remotePostHelper.Add("key", _PayuPaymentSettings.MerchantId.ToString());
            remotePostHelper.Add("amount", postProcessPaymentRequest.Order.OrderTotal.ToString(new CultureInfo("en-US", false).NumberFormat));
			remotePostHelper.Add("productinfo", "productinfo");
            remotePostHelper.Add("Currency", _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode);
            remotePostHelper.Add("Order_Id", orderId.ToString());
			remotePostHelper.Add("txnid", orderId.ToString());
            remotePostHelper.Add("service_provider", "payu_paisa");
            remotePostHelper.Add("surl", _webHelper.GetStoreLocation(false) + "Plugins/PaymentPayu/Return");
			remotePostHelper.Add("furl", _webHelper.GetStoreLocation(false) + "Plugins/PaymentPayu/Return");
            remotePostHelper.Add("hash", myUtility.getchecksum(_PayuPaymentSettings.MerchantId.ToString(), 
                postProcessPaymentRequest.Order.Id.ToString(), postProcessPaymentRequest.Order.OrderTotal.ToString(new CultureInfo("en-US", false).NumberFormat),
                "productinfo",postProcessPaymentRequest.Order.BillingAddress.FirstName.ToString(),
                postProcessPaymentRequest.Order.BillingAddress.Email.ToString(),_PayuPaymentSettings.Key));


            //Billing details
            remotePostHelper.Add("firstname", postProcessPaymentRequest.Order.BillingAddress.FirstName.ToString());
            remotePostHelper.Add("billing_cust_address", postProcessPaymentRequest.Order.BillingAddress.Address1);
            remotePostHelper.Add("phone", postProcessPaymentRequest.Order.BillingAddress.PhoneNumber);
            remotePostHelper.Add("email", postProcessPaymentRequest.Order.BillingAddress.Email.ToString());
            remotePostHelper.Add("billing_cust_city", postProcessPaymentRequest.Order.BillingAddress.City);
            var billingStateProvince = postProcessPaymentRequest.Order.BillingAddress.StateProvince;
            if (billingStateProvince != null)
                remotePostHelper.Add("billing_cust_state", billingStateProvince.Abbreviation);
            else
                remotePostHelper.Add("billing_cust_state", "");
            remotePostHelper.Add("billing_zip_code", postProcessPaymentRequest.Order.BillingAddress.ZipPostalCode);
            var billingCountry = postProcessPaymentRequest.Order.BillingAddress.Country;
            if (billingCountry != null)
                remotePostHelper.Add("billing_cust_country", billingCountry.ThreeLetterIsoCode);
            else
                remotePostHelper.Add("billing_cust_country", "");

            //Delivery details

            if (postProcessPaymentRequest.Order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            {
                remotePostHelper.Add("delivery_cust_name", postProcessPaymentRequest.Order.ShippingAddress.FirstName);
                remotePostHelper.Add("delivery_cust_address", postProcessPaymentRequest.Order.ShippingAddress.Address1);
                remotePostHelper.Add("delivery_cust_notes", string.Empty);
                remotePostHelper.Add("delivery_cust_tel", postProcessPaymentRequest.Order.ShippingAddress.PhoneNumber);
                remotePostHelper.Add("delivery_cust_city", postProcessPaymentRequest.Order.ShippingAddress.City);
                var deliveryStateProvince = postProcessPaymentRequest.Order.ShippingAddress.StateProvince;
                if (deliveryStateProvince != null)
                    remotePostHelper.Add("delivery_cust_state", deliveryStateProvince.Abbreviation);
                else
                    remotePostHelper.Add("delivery_cust_state", "");
                remotePostHelper.Add("delivery_zip_code", postProcessPaymentRequest.Order.ShippingAddress.ZipPostalCode);
                var deliveryCountry = postProcessPaymentRequest.Order.ShippingAddress.Country;
                if (deliveryCountry != null)
                    remotePostHelper.Add("delivery_cust_country", deliveryCountry.ThreeLetterIsoCode);
                else
                    remotePostHelper.Add("delivery_cust_country", "");
            }

          //  remotePostHelper.Add("Merchant_Param", _PayuPaymentSettings.MerchantParam);
            remotePostHelper.Post();
        }



        //Hide payment begins

        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return false;
        }

        //hide payment ends

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return _PayuPaymentSettings.AdditionalFee;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();
            result.AddError("Capture method not supported");
            return result;
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            result.AddError("Refund method not supported");
            return result;
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            result.AddError("Void method not supported");
            return result;
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            //Payu is the redirection payment method
            //It also validates whether order is also paid (after redirection) so customers will not be able to pay twice
            
            //payment status should be Pending
            if (order.PaymentStatus != PaymentStatus.Pending)
                return false;

            //let's ensure that at least 1 minute passed after order is placed
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalMinutes < 1)
                return false;

            return true;
        }

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

        public Type GetControllerType()
        {
            return typeof(PaymentPayuController);
        }

        public override void Install()
        {
            var settings = new PayuPaymentSettings()
            {
                MerchantId = "",
                Key = "",
                MerchantParam = "",
                PayUri = "https://test.payu.in/_payment",
                AdditionalFee = 0,
            };
            _settingService.SaveSetting(settings);

            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Payu.RedirectionTip", "You will be redirected to Payu site to complete the order.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Payu.MerchantId", "Key");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Payu.MerchantId.Hint", "Enter Key.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Payu.Key", "Salt");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Payu.Key.Hint", "Enter salt.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Payu.MerchantParam", "Merchant Param");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Payu.MerchantParam.Hint", "Enter merchant param.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Payu.PayUri", "Pay URI");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Payu.PayUri.Hint", "Enter Pay URI.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Payu.AdditionalFee", "Additional fee");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Payu.AdditionalFee.Hint", "Enter additional fee to charge your customers.");
            
            base.Install();
        }

        public override void Uninstall()
        {
            //locales
            this.DeletePluginLocaleResource("Plugins.Payments.Payu.RedirectionTip");
            this.DeletePluginLocaleResource("Plugins.Payments.Payu.MerchantId");
            this.DeletePluginLocaleResource("Plugins.Payments.Payu.MerchantId.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Payu.Key");
            this.DeletePluginLocaleResource("Plugins.Payments.Payu.Key.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Payu.MerchantParam");
            this.DeletePluginLocaleResource("Plugins.Payments.Payu.MerchantParam.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Payu.PayUri");
            this.DeletePluginLocaleResource("Plugins.Payments.Payu.PayUri.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Payu.AdditionalFee");
            this.DeletePluginLocaleResource("Plugins.Payments.Payu.AdditionalFee.Hint");
            
            base.Uninstall();
        }
        #endregion

        #region Properies

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get
            {
                return RecurringPaymentType.NotSupported;
            }
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType
        {
            get
            {
                return PaymentMethodType.Redirection;
            }
        }


        public bool SkipPaymentInfo
        {
            get
            {
                return false;
            }
        }

      


        #endregion
    }
}
