﻿using MediaGraph.Code;
using MediaGraph.Models;
using MediaGraph.ViewModels.Edit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace MediaGraph.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult AccountDeleted()
        {
            return View();
        }
    }
}