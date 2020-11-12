using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Transaction.API.Library;
using Transaction.Entity;
using Transaction.Entity.Entity;

namespace Transaction.API.Controllers
{
    /// <summary>
    /// Transaction Upload API
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly ILogger<TransactionController> _logger;
        private readonly ApplicationDbContext _dbcontext;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private const int MAX_FILE_SIZE = 1 * 1024 * 1024; // 1 MB

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="environment"></param>
        /// <param name="logger"></param>
        public TransactionController(ApplicationDbContext dbContext,
            IWebHostEnvironment environment,
            ILogger<TransactionController> logger)
        {
            _logger = logger;
            _dbcontext = dbContext;
            _hostingEnvironment = environment;
        }

        /// <summary>
        /// Get Transaction by Currency Code (ISO4217 Format)
        /// e.g: USD, EUR
        /// </summary>
        /// <param name="currencyCode">ISO4217 Format</param>
        /// <returns>List of Transaction</returns>
        [HttpGet()]
        [Route("/api/Transaction/GetByCurrencyCode/{currencyCode}")]
        [ProducesResponseType(typeof(IEnumerable<Object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IEnumerable<object>> GetByCurrencyCode(string currencyCode)
        {
            try
            {
                //TODO: Pagination
                var data = await (from trans in _dbcontext.TransactionEntity
                                  where trans.CurrencyCode == currencyCode
                                  orderby trans.Id descending
                                  select new
                                  {
                                      id = trans.TransactionId,
                                      payment = string.Format("{0} {1}", trans.Amount, trans.CurrencyCode),
                                      Status = trans.Status.ToString()
                                  })
                           .Take(3000)
                           .ToListAsync();

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                Response.StatusCode = 500;
                return null;
            }
        }

        /// <summary>
        /// Get Transaction by Date Range(inclusive [startDate, endDate]) (UTC Date Format e.g:2020-11-28T13:55:55Z)
        /// </summary>
        /// <param name="startDate">2010-11-28T13:55:55Z</param>
        /// <param name="endDate">2020-11-28T13:55:55Z</param>
        /// <returns>List of Transactions</returns>
        [HttpGet()]
        [Route("/api/Transaction/GetByDateRange/{startDate},{endDate}")]
        [ProducesResponseType(typeof(IEnumerable<Object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IEnumerable<Object>> GetByDateRange(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            try
            {
                //TODO: Pagination
                var data = await (from trans in _dbcontext.TransactionEntity
                                  where trans.DateTime >= startDate.UtcDateTime && trans.DateTime <= endDate.UtcDateTime
                                  orderby trans.Id descending
                                  select new
                                  {
                                      id = trans.TransactionId,
                                      payment = string.Format("{0} {1}", trans.Amount, trans.CurrencyCode),
                                      Status = trans.Status.ToString()
                                  })
                           .Take(3000)
                           .ToListAsync();

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                Response.StatusCode = 500;
                return null;
            }
        }

        /// <summary>
        /// Get Transaction by Status Code (Approve:A, Reject:R, Done:D)
        /// </summary>
        /// <param name="statusCode"></param>
        /// <returns>List of Transaction</returns>
        [HttpGet()]
        [Route("/api/Transaction/GetByStatusCode/{statusCode}")]
        [ProducesResponseType(typeof(IEnumerable<Object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IEnumerable<Object>> GetByStatusCode(StatusCode statusCode)
        {
            try
            {
                //TODO: Pagination
                //throw new Exception();
                var data = await (from trans in _dbcontext.TransactionEntity
                                  where trans.Status == statusCode
                                  orderby trans.Id descending
                                  select new
                                  {
                                      id = trans.TransactionId,
                                      payment = string.Format("{0} {1}", trans.Amount, trans.CurrencyCode),
                                      Status = trans.Status.ToString()
                                  })
                           .Take(3000)
                           .ToListAsync();

                return data;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                Response.StatusCode = 500;
                return null;
            }
        }

        /// <summary>
        /// Upload Transaction File (csv or xml format)
        /// </summary>
        /// <param name="file">.csv, .xml</param>
        /// <returns>List of Transaction</returns>
        [HttpPost("upload", Name = "upload")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null)
                return BadRequest("File required");

            var ext = Path.GetExtension(file.FileName).ToLower();
            if (!(ext == ".csv" || ext == ".xml"))
                return BadRequest("Unknown format");

            if (file.Length > (MAX_FILE_SIZE))
                return BadRequest($"Invalid File Size (Maximum: {MAX_FILE_SIZE / (1024 * 1024)} MB)");


            // Save file into Uploads folder
            var transFile = new TransactionFile();
            string errormsg = "";
            try
            {
                //throw new Exception();
                transFile = await SaveFileAsync(file);
                Strategy strategy;
                if (ext == ".csv")
                    strategy = new CsvExtractor();
                else
                    strategy = new XmlExtractor();

                var entityExtractor = new TransactionExtractor(strategy);
                var entities = entityExtractor.ExtractTransaction(file);
                if (entities.Where(x => !string.IsNullOrWhiteSpace(x.Errors)).Count() == 0)
                {
                    // Clean records, add to transaction
                    var cleanEntities = entities.Select(x => new TransactionEntity
                    {
                        TransactionId = x.TransactionId,
                        Amount = x.ValidAmount.Value,
                        CurrencyCode = x.CurrencyCode,
                        DateTime = x.ValidTransactionDate.Value.UtcDateTime,
                        Status = x.ValidStatus
                    });
                    _dbcontext.TransactionEntity.AddRange(cleanEntities);
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
                        Status = x.Status,
                        Error = x.Errors,
                        FileId = transFile.Id
                    });
                    _dbcontext.TransactionErrorLog.AddRange(errors);
                    errormsg = "Some records in uploaded file is not valid, please check error log for details.";
                }
                await _dbcontext.SaveChangesAsync();
                if (string.IsNullOrWhiteSpace(errormsg))
                    return Ok("Success");
                else
                    return BadRequest(errormsg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                if (System.IO.File.Exists(Path.Combine(_hostingEnvironment.ContentRootPath, "Uploads", transFile.FileName ?? "")))
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    System.IO.File.Delete(Path.Combine(_hostingEnvironment.ContentRootPath, "Uploads", transFile.FileName ?? ""));
                }
                // TODO: If db exception in here
                if (transFile.Id != 0)
                {
                    _dbcontext.TransactionFile.Remove(transFile);
                    await _dbcontext.SaveChangesAsync();
                }
                return new StatusCodeResult(500);
            }
        }

        private async Task<TransactionFile> SaveFileAsync(IFormFile file)
        {
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

            return transFile;
        }

        private string GetUniqueFileName(string fileName)
        {
            fileName = Path.GetFileNameWithoutExtension(fileName) +
                "_" +
                Guid.NewGuid().ToString("N") +
                Path.GetExtension(fileName);
            return fileName;
        }
    }
}
