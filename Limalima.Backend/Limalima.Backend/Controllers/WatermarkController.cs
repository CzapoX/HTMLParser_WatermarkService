﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Limalima.Backend.Controllers
{
    public class WatermarkController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
