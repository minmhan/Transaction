using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Transaction.Web.Models;

namespace Transaction.Web.Controllers
{
    public class TransactionController : Controller
    {
        private readonly IConfiguration _Configure;
        private readonly string _apiBaseUrl;
        public TransactionController(IConfiguration configuration)
        {
            _Configure = configuration;
            _apiBaseUrl = _Configure.GetValue<string>("WebAPIBaseUrl");
        }
        public IActionResult Index()
        {
            return View();
        }

        [Route("/transaction")]
        [HttpPost]
        public IActionResult GetTransaction(string status)
        {
            // TODO: Supporse to server side paging, API need to enable pagination.
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            var pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;

            var endpoint = $"{_apiBaseUrl}/api/Transaction/GetByStatusCode/{status}";
            using (HttpClient client = new HttpClient())
            {
                using (var response = client.GetAsync(endpoint).Result)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var r = response.Content.ReadAsStringAsync().Result;
                        var result = JsonConvert.DeserializeObject<List<TransactionViewModel>>(r);
                        // TODO: These suppose to before api call if api support pagination and these features.
                        if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                        {
                            result = result
                                .AsQueryable<TransactionViewModel>().OrderBy(sortColumn + " " + sortColumnDirection)
                                .ToList();
                        }
                        if (!string.IsNullOrEmpty(searchValue))
                        {
                            result = result.Where(m => m.Id.Contains(searchValue)).ToList();
                        }
                        var jsonData = new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = result.Count, data = result };
                        return Ok(jsonData);
                    }
                    else
                    {
                        return NoContent();
                    }
                }
            }
            
        }
    }
}
