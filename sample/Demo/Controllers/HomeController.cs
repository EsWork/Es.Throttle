using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Es.Throttle;
using Es.Throttle.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Demo.Controllers
{

    public class HomeController : Controller
    {
        [EnableThrottling(3, 3, RateLimitPeriod.Sec)]
        public IActionResult Index()
        {
            return Content("Index");
        }

        public IActionResult Index2()
        {
            return Content("Index2");
        }

        public IActionResult Test()
        {
            return Content("Test");
        }

        public IActionResult Conf()
        {
            var tp = HttpContext.RequestServices.GetService<IOptionsSnapshot<ThrottleOptions>>();

            return Json(tp);

        }
    }
}
