﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.Payu.Models;
using Nop.Services.Configuration;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Localization;

using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Nop.Services.Security;
using Nop.Core.Domain.Orders;
using System.Text;
using Nop.Services.Messages;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.Payu.Controllers
{
    public class PaymentPayuController : BasePaymentController
    {
        private readonly ISettingService _settingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly INotificationService _notificationService;
        private readonly PayuPaymentSettings _payuPaymentSettings;
        private readonly PaymentSettings _paymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;

        public PaymentPayuController(ISettingService settingService,
            IPaymentService paymentService, IOrderService orderService,
            IOrderProcessingService orderProcessingService, INotificationService notificationService,
             ILocalizationService localizationService,
            PayuPaymentSettings payuPaymentSettings,
            PaymentSettings paymentSettings,
             IPermissionService permissionService)
        {
            this._settingService = settingService;
            this._paymentService = paymentService;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._notificationService = notificationService;
            this._payuPaymentSettings = payuPaymentSettings;
            this._localizationService = localizationService;
            this._paymentSettings = paymentSettings;
            this._permissionService = permissionService;
        }


        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            var model = new ConfigurationModel();
            model.MerchantId = _payuPaymentSettings.MerchantId;
            model.Key = _payuPaymentSettings.Key;
            model.MerchantParam = _payuPaymentSettings.MerchantParam;
            model.PayUri = _payuPaymentSettings.PayUri;
           
            model.AdditionalFee = _payuPaymentSettings.AdditionalFee;
            model.AdditionalFeePercentage = _payuPaymentSettings.AdditionalFeePercentage;

            // return View("Nop.Plugin.Payments.Payu.Views.PaymentPayu.Configure", model);

            return View("~/Plugins/Payments.Payu/Views/Configure.cshtml", model);

            //return View("~/Plugins/Payments.PayPalStandard/Views/PaymentPayPalStandard/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();
            if (!ModelState.IsValid)
                return await  Configure();

            //save settings
            _payuPaymentSettings.MerchantId = model.MerchantId;
            _payuPaymentSettings.Key = model.Key;
            _payuPaymentSettings.MerchantParam = model.MerchantParam;
            _payuPaymentSettings.PayUri = model.PayUri;
            _payuPaymentSettings.AdditionalFee = model.AdditionalFee;
            _payuPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            await  _settingService.SaveSettingAsync(_payuPaymentSettings);

            //return View("Nop.Plugin.Payments.Payu.Views.PaymentPayu.Configure", model);
            //return View("~/Plugins/Payments.Payu/Views/PaymentPayu/Configure.cshtml", model);
            //now clear settings cache
       await      _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await  Configure();
        }

        //[ChildActionOnly]
        public IActionResult PaymentInfo()
        {
            var model = new PaymentInfoModel();
            //return View("Nop.Plugin.Payments.Payu.Views.PaymentPayu.PaymentInfo", model);
            return View("~/Plugins/Payments.Payu/Views/PaymentPayu/PaymentInfo.cshtml", model);

        }

        /*

        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();
            return warnings;
        }

        [NonAction]
        public override ProcessPaymentRequest Ge                                                                                                                                                                                         tPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            return paymentInfo;
        }
        */

        //[ValidateInput(false)]
        public async Task<IActionResult> Return(IpnModel model)
        {

            try
            {
                /*
                var processor = _paymentService.LoadPaymentMethodBySystemName("Payments.Payu") as PayuPaymentProcessor;
                if (processor == null ||
                    !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
                    throw new NopException("Payu module cannot be loaded");

                */
                var myUtility = new PayuHelper();
                string orderId, merchantId, amount, productinfo, firstname, email, hash, status, checksum;

                //Assign following values to send it to verifychecksum function.
                if (String.IsNullOrWhiteSpace(_payuPaymentSettings.Key))
                    throw new NopException("Payu key is not set");


                merchantId = _payuPaymentSettings.MerchantId.ToString();
                orderId = model.Form["txnid"];
                amount = model.Form["amount"];
                productinfo = model.Form["productinfo"];
                firstname = model.Form["firstname"];
                email = model.Form["email"];
                hash = model.Form["hash"];
                status = model.Form["status"];

                checksum = myUtility.verifychecksum(merchantId, orderId, amount, productinfo, firstname, email, status, _payuPaymentSettings.Key);

                if (checksum == hash)
                {

                    if (status == "success")
                    {
                        /* 
                            Here you need to put in the routines for a successful 
                             transaction such as sending an email to customer,
                             setting database status, informing logistics etc etc
                        */

                        var order =await  _orderService.GetOrderByIdAsync(Convert.ToInt32(orderId));
                        if (_orderProcessingService.CanMarkOrderAsPaid(order))
                        {
                          await  _orderProcessingService.MarkOrderAsPaidAsync(order);

                            var sb = new StringBuilder();
                            sb.AppendLine("PayU IPN:");
                            foreach (var v in model.Form)
                            {
                                sb.AppendLine(v.Key + ": " + v.Value);
                            }
                            //order note
                            //OrderNoteAdd
                          await  _orderService.InsertOrderNoteAsync(new OrderNote
                            {

                                OrderId = order.Id,
                                Note = sb.ToString(),
                                DisplayToCustomer = false,
                                CreatedOnUtc = DateTime.UtcNow
                            });
                          await   _orderService.UpdateOrderAsync(order);

                        }

                        //Thank you for shopping with us. Your credit card has been charged and your transaction is successful
                        return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
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
            catch (Exception ex)
            {

                return Content("Error Occured :" + ex.Message);
            }
        }
    }
}