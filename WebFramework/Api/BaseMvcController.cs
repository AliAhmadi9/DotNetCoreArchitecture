using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using WebFramework.Configuration;

namespace WebFramework.Api
{
    public class BaseMvcController : Controller
    {
        public bool IsAuthenticated => User.Identity.IsAuthenticated && CookieManager.Get(HttpContext, CookieManager.CookieKeys.JwtToken) != null;
    }
}
