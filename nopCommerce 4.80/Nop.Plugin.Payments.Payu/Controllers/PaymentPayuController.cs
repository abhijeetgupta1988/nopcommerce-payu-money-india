using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.Payu.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Payments.Payu.Controllers;

public class PaymentPayuController : BasePaymentController
{
    private readonly ISettingService _settingService;
    private readonly IOrderService _orderService;
    private readonly IOrderProcessingService _orderProcessingService;
    private readonly INotificationService _notificationService;
    private readonly PayuPaymentSettings _payuPaymentSettings;
    private readonly ILocalizationService _localizationService;
    private readonly IPaymentPluginManager _paymentPluginManager;

    public PaymentPayuController(ISettingService settingService,
            IPaymentService paymentService,
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            PayuPaymentSettings payuPaymentSettings,
            PaymentSettings paymentSettings,
            IPermissionService permissionService,
            IPaymentPluginManager paymentPluginManager)
    {
        this._settingService = settingService;
        this._payuPaymentSettings = payuPaymentSettings;
        this._orderService = orderService;
        this._orderProcessingService = orderProcessingService;
        this._notificationService = notificationService;
        this._localizationService = localizationService;
        this._paymentPluginManager = paymentPluginManager;
    }

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PAYMENT_METHODS)]
    public IActionResult Configure()
    {
        var model = new ConfigurationModel();
        model.MerchantId = _payuPaymentSettings.MerchantId;
        model.Key = _payuPaymentSettings.Key;
        model.PayUri = _payuPaymentSettings.PayUri;
        model.AdditionalFee = _payuPaymentSettings.AdditionalFee;
        model.AdditionalFeePercentage = _payuPaymentSettings.AdditionalFeePercentage;

        return View("~/Plugins/Payments.Payu/Views/Configure.cshtml", model);
    }

    [HttpPost]
    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [AutoValidateAntiforgeryToken]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PAYMENT_METHODS)]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!ModelState.IsValid)
            return Configure();

        //save settings
        _payuPaymentSettings.MerchantId = model.MerchantId;
        _payuPaymentSettings.Key = model.Key;
        _payuPaymentSettings.PayUri = model.PayUri;
        _payuPaymentSettings.AdditionalFee = model.AdditionalFee;
        _payuPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
        await _settingService.SaveSettingAsync(_payuPaymentSettings);

        //now clear settings cache
        await _settingService.ClearCacheAsync();
        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));
        return Configure();
    }

    //[ChildActionOnly]
    public IActionResult PaymentInfo()
    {
        var model = new PaymentInfoModel();
        return View("~/Plugins/Payments.Payu/Views/PaymentPayu/PaymentInfo.cshtml", model);
    }

    public async Task<IActionResult> Return(IpnModel model)
    {
        try
        {
            if (!(await _paymentPluginManager.LoadPluginBySystemNameAsync("Payments.Payu") is PayuPaymentProcessor processor) ||
                !_paymentPluginManager.IsPluginActive(processor) ||
                !processor.PluginDescriptor.Installed)
                throw new NopException("Payu module cannot be loaded");

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
            checksum = myUtility.VerifyChecksum(merchantId, orderId, amount, productinfo, firstname, email, status, _payuPaymentSettings.Key);

            if (checksum == hash)
            {
                if (status == "success")
                {
                    /* 
                        Here you need to put in the routines for a successful 
                         transaction such as sending an email to customer,
                         setting database status, informing logistics etc etc
                    */

                    var order = await _orderService.GetOrderByIdAsync(Convert.ToInt32(orderId));
                    if (_orderProcessingService.CanMarkOrderAsPaid(order))
                    {
                        await _orderProcessingService.MarkOrderAsPaidAsync(order);

                        var formData = model.Form.ToDictionary(k => k.Key, v => v.Value.ToString());
                        string jsonString = JsonSerializer.Serialize(formData);
                        PayUIPNResponse response = JsonSerializer.Deserialize<PayUIPNResponse>(jsonString);

                        //Add Order Note
                        await _orderService.InsertOrderNoteAsync(new OrderNote
                        {
                            OrderId = order.Id,
                            Note = JsonSerializer.Serialize(response),
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow
                        });
                        await _orderService.UpdateOrderAsync(order);
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