using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Transaction.Web.Models;

namespace Transaction.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _Configure;
        private readonly string _apiBaseUrl;

        public HomeController(IConfiguration configuration, ILogger<HomeController> logger)
        {
            //_logger = logger;
            _Configure = configuration;
            _apiBaseUrl = _Configure.GetValue<string>("WebAPIBaseUrl");
        }

        public IActionResult Index(string msg)
        {
            ViewBag.msg = msg;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index([Bind("FormFile")] TransactionFileViewModel vm)
        {
            if(vm.FormFile == null)
            {
                ModelState.AddModelError("FormFile", "Upload File is required.");
            }
            var message = "";
            if (ModelState.IsValid)
            {
                var endpoint = _apiBaseUrl + "/api/transaction/upload";
                using (HttpClient client = new HttpClient())
                {
                    using(var content = new MultipartFormDataContent())
                    {
                        content.Add(new StreamContent(vm.FormFile.OpenReadStream()),"file",vm.FormFile.FileName);
                        using (var response = await client.PostAsync(endpoint, content))
                        {
                            var status = response.StatusCode.ToString();
                            message = response.Content.ReadAsStringAsync().Result;
                        }
                    }
                }
            }
            else
            {
                return View(vm);
            }

            ViewBag.msg = message;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
