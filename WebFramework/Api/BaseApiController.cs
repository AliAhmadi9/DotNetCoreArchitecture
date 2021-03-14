using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using WebFramework.Configuration;
using WebFramework.Filters;

namespace WebFramework.Api
{
    [ApiController]
    [ApiResultFilter]
    [Route("api/v{version:apiVersion}/[controller]")] // => /api/v1/...
    //[AllowAnonymous] //to development test
    public class BaseApiController : ControllerBase
    {
        public bool IsAuthenticated => User.Identity.IsAuthenticated && CookieManager.Get(HttpContext, CookieManager.CookieKeys.JwtToken) != null;
    }
}
