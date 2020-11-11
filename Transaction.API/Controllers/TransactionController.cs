using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using CsvHelper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Transaction.API.Library;
using Transaction.API.Models;
using Transaction.Entity;
using Transaction.Entity.Entity;

namespace Transaction.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly ILogger<TransactionController> _logger;
        private readonly ApplicationDbContext _dbcontext;
        private readonly IWebHostEnvironment _hostingEnvironment;
        //private readonly EntityExtractor _entityExtractor;
        private const int MAX_FILE_SIZE = 1 * 1024 * 1024; // 1 MB

        public TransactionController(ApplicationDbContext dbContext,
            IWebHostEnvironment environment,
            ILogger<TransactionController> logger)
        {
            _logger = logger;
            _dbcontext = dbContext;
            _hostingEnvironment = environment;
        }

        [HttpGet("{currencyCode}")]
        public IEnumerable<TransactionEntity> Get(string currencyCode)
        {
            var rng = new Random();
            var data = (from trans in _dbcontext.TransactionEntity
                       where trans.CurrencyCode == currencyCode
                       select trans)
                       .Skip(0)
                       .Take(10);


            return data;
        }

        [HttpGet("{start,end}")]
        public IEnumerable<TransactionEntity> Get2(DateTime start, DateTime end)
        {
            var rng = new Random();
            var data = (from trans in _dbcontext.TransactionEntity
                        where trans.DateTime >= start && trans.DateTime <= end
                        select trans)
                       .Skip(0)
                       .Take(10);


            return data;
        }

        [HttpGet("{status}")]
        public IEnumerable<TransactionEntity> Get3(StatusCode status)
        {
            var rng = new Random();
            var data = (from trans in _dbcontext.TransactionEntity
                        where trans.Status == status
                        select trans)
                       .Skip(0)
                       .Take(10);


            return data;
        }

        [HttpPost]
        public async Task<IActionResult> Post(IFormFile file)
        {
            if (file == null)
                return BadRequest("File required");

            var ext = Path.GetExtension(file.FileName).ToLower();
            if (!(ext == ".csv" || ext == ".xml"))
                return BadRequest("Invalid File Type");

            if (file.Length > (MAX_FILE_SIZE))
                return BadRequest($"Invalid File Size (Maximum: {MAX_FILE_SIZE / (1024 * 1024)} MB)");

            var uniqueFileName = GetUniqueFileName(file.FileName);
            var uploads = Path.Combine(_hostingEnvironment.ContentRootPath, "Uploads");
            var filePath = Path.Combine(uploads, uniqueFileName);
            await file.CopyToAsync(new FileStream(filePath, FileMode.Create));
            var transFile = new TransactionFile
            {
                FileName = uniqueFileName,
                UploadedDate = DateTime.UtcNow
            };
            _dbcontext.TransactionFile.Add(transFile);
            await _dbcontext.SaveChangesAsync();

            Strategy strategy;
            if (ext == ".csv")
                strategy = new CsvExtractor();
            else
                strategy = new XmlExtractor();
            var entityExtractor = new EntityExtractor(strategy);
            var entities = entityExtractor.ExtractEntity(file);

            if(entities.Where(x => string.IsNullOrWhiteSpace(x.Errors)).Count() == 0){
                // Clean records, add to transaction
                var e = entities.Select(x => new TransactionEntity
                {
                    TransactionId = x.TransactionId,
                    Amount = x.ValidAmount.Value,
                    CurrencyCode = x.CurrencyCode,
                    DateTime = x.ValidTransactionDate.Value.UtcDateTime,
                    Status = Entity.StatusCode.A
                });
                _dbcontext.TransactionEntity.AddRange(e);
            }
            else
            {
                // Dirty records, add to error log
                var errors = entities.Select(x => new TransactionErrorLog
                {
                    TransactionId = x.TransactionId,
                    Amount = x.Amount,
                    CurrencyCode = x.CurrencyCode,
                    DateTime = x.TransactionDate,
                    Status = x.Status
                });
                _dbcontext.TransactionErrorLog.AddRange(errors);
            }
            await _dbcontext.SaveChangesAsync();

            return Ok();
        }

        private bool IsValid()
        {
            return true;
        }

        private string GetUniqueFileName(string fileName)
        {
            fileName = Path.GetFileNameWithoutExtension(fileName) + 
                "_" + 
                Guid.NewGuid().ToString("N") +
                Path.GetExtension(fileName);
            return fileName;
        }

        private StatusCode GetStatusCode(string status)
        {
            //TODO: Declare single place of statuscode
            StatusCode statusCode;
            switch (status)
            {
                case "Approved":
                    statusCode = Entity.StatusCode.A;
                    break;
                case "Failed":
                    statusCode = Entity.StatusCode.R;
                    break;
                default:
                    statusCode = Entity.StatusCode.D;
                    break;
            }

            return statusCode;
        }

        private StatusCode GetXmlStatusCode(string status)
        {
            //TODO: Declare single place of statuscode
            StatusCode statusCode;
            switch (status)
            {
                case "Approved":
                    statusCode = Entity.StatusCode.A;
                    break;
                case "Rejected":
                    statusCode = Entity.StatusCode.R;
                    break;
                default:
                    statusCode = Entity.StatusCode.D;
                    break;
            }

            return statusCode;
        }
        //private bool IsValid(List<string> cols)
        //{
        //    if (cols.Count != 5)
        //        return false;
        //    var validStatus = new List<string> { "Approved", "Failed", "Finished" };
        //    var dateFormatString = "dd/MM/yyyy hh:mm:ss";
        //    var valid = true;

        //    if (cols[0].Trim().Length > 50)
        //        valid = false;
        //    if (!Decimal.TryParse(cols[1].Trim(), out decimal amount))
        //        valid = false;
        //    if (cols[2].Trim().Length != 3) // TODO: Should check with currency code table
        //        valid = false;
        //    if (!DateTime.TryParseExact(cols[3].Trim(), dateFormatString, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime datetime))
        //        valid = false;
        //    if (!validStatus.Contains(cols[4].Trim()))
        //        valid = false;

        //    return valid;
        //}


    }
}
