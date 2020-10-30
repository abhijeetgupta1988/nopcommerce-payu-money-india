﻿using Microsoft.AspNetCore.Http;
using Nop.Web.Framework.Models;


namespace Nop.Plugin.Payments.Payu.Models
{
   public class IpnModel:BaseNopModel
    {
        //MVC is suppressing further validation if the IFormCollection is passed to a controller method. That's why we add to the model
        public IFormCollection Form { get; set; }
    }
}
