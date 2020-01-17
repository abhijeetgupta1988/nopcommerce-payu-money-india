using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.Payu.Models;
using Nop.Services.Configuration;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Localization;

using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Payments.Payu.Controllers
{
    public class PaymentPayuController : BasePaymentController
    {
        private readonly ISettingService _settingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;

        private readonly PayuPaymentSettings _PayuPaymentSettings;
        private readonly PaymentSettings _paymentSettings;
        private readonly ILocalizationService _localizationService;

        public PaymentPayuController(ISettingService settingService, 
            IPaymentService paymentService, IOrderService orderService, 
            IOrderProcessingService orderProcessingService,
             ILocalizationService localizationService,
            PayuPaymentSettings PayuPaymentSettings,
            PaymentSettings paymentSettings)
        {
            this._settingService = settingService;
            this._paymentService = paymentService;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._PayuPaymentSettings = PayuPaymentSettings;
            this._localizationService = localizationService;
            this._paymentSettings = paymentSettings;
        }
        

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = new ConfigurationModel();
            model.MerchantId = _PayuPaymentSettings.MerchantId;
            model.Key = _PayuPaymentSettings.Key;
            model.MerchantParam = _PayuPaymentSettings.MerchantParam;
            model.PayUri = _PayuPaymentSettings.PayUri;
            model.AdditionalFee = _PayuPaymentSettings.AdditionalFee;
            
           // return View("Nop.Plugin.Payments.Payu.Views.PaymentPayu.Configure", model);
            
           return View("~/Plugins/Payments.Payu/Views/PaymentPayu/Configure.cshtml", model);

           //return View("~/Plugins/Payments.PayPalStandard/Views/PaymentPayPalStandard/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //save settings
            _PayuPaymentSettings.MerchantId = model.MerchantId;
            _PayuPaymentSettings.Key = model.Key;
            _PayuPaymentSettings.MerchantParam = model.MerchantParam;
            _PayuPaymentSettings.PayUri = model.PayUri;
            _PayuPaymentSettings.AdditionalFee = model.AdditionalFee;
            _settingService.SaveSetting(_PayuPaymentSettings);
            
            //return View("Nop.Plugin.Payments.Payu.Views.PaymentPayu.Configure", model);
            //return View("~/Plugins/Payments.Payu/Views/PaymentPayu/Configure.cshtml", model);

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            var model = new PaymentInfoModel();
            //return View("Nop.Plugin.Payments.Payu.Views.PaymentPayu.PaymentInfo", model);
            return View("~/Plugins/Payments.Payu/Views/PaymentPayu/PaymentInfo.cshtml", model);

        }

        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();
            return warnings;
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            return paymentInfo;
        }

        [ValidateInput(false)]
        public ActionResult Return(FormCollection form)
        {
            var processor = _paymentService.LoadPaymentMethodBySystemName("Payments.Payu") as PayuPaymentProcessor;
            if (processor == null ||
                !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
                throw new NopException("Payu module cannot be loaded");


            var myUtility = new PayuHelper();
            string orderId, merchantId, Amount, productinfo, firstname, email, hash, status,checksum;

            //Assign following values to send it to verifychecksum function.
            if (String.IsNullOrWhiteSpace(_PayuPaymentSettings.Key))
                throw new NopException("Payu key is not set");

				
            merchantId = _PayuPaymentSettings.MerchantId.ToString();
            orderId = form["txnid"];
            Amount = form["amount"];
			productinfo = form["productinfo"];
			firstname = form["firstname"];
			email = form["email"];
            hash = form["hash"];
			status = form["status"];

            checksum = myUtility.verifychecksum(merchantId, orderId, Amount, productinfo, firstname, email, status, _PayuPaymentSettings.Key);

            if (checksum == hash)
            {

			 if(status == "success")
			 {
                /* 
                    Here you need to put in the routines for a successful 
                     transaction such as sending an email to customer,
                     setting database status, informing logistics etc etc
                */

                var order = _orderService.GetOrderById(Convert.ToInt32(orderId));
                if (_orderProcessingService.CanMarkOrderAsPaid(order))
                {
                    _orderProcessingService.MarkOrderAsPaid(order);
                }

                //Thank you for shopping with us. Your credit card has been charged and your transaction is successful
                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id});
			  }
			  
			  else
			  {
			    /*
                    Here you need to put in the routines for a failed
                    transaction such as sending an email to customer
                    setting database status etc etc
                */

               return RedirectToAction("Index", "Home", new { area = "" });
			  
			  }

            }
            
            
            else
            {
                /*
                    Here you need to simply ignore this and dont need
                    to perform any operation in this condition
                */

                return Content("Security Error. Illegal access detected");
            }
        }
    }
}