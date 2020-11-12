using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transaction.Entity;

namespace Transaction.API.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionLogController : Controller
    {
        private readonly ILogger<TransactionController> _logger;
        private readonly ApplicationDbContext _dbcontext;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="logger"></param>
        public TransactionLogController(ApplicationDbContext dbContext,
            ILogger<TransactionController> logger)
        {
            _logger = logger;
            _dbcontext = dbContext;
        }

        /// <summary>
        /// Get Transaction Error Log
        /// </summary>
        /// <returns>List of Transaction Errors</returns>
        [HttpGet()]
        [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
        public async Task<IEnumerable<object>> Get()
        {
            try
            {
                //TODO: Pagination
                var transactionLog = await (from trans in _dbcontext.TransactionErrorLog
                                            join transFile in _dbcontext.TransactionFile on trans.FileId equals transFile.Id
                                            orderby trans.Id descending
                                            select new
                                            {
                                                trans.Id,
                                                trans.FileId,
                                                trans.TransactionId,
                                                trans.Amount,
                                                trans.CurrencyCode,
                                                trans.DateTime,
                                                trans.Status,
                                                trans.Error,
                                                transFile.FileName,
                                                transFile.UploadedDate,
                                            })
                                       .Take(3000)
                                       .ToListAsync();

                return transactionLog;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                Response.StatusCode = 500;
                return null;
            }
        }
    }
}
