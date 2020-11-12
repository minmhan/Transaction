using System;
using Transaction.Entity;

namespace Transaction.API.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class UploadTransaction
    {
        public string TransactionId { get; set; }
        public string Amount { get; set; }
        public string CurrencyCode { get; set; }
        public string TransactionDate { get; set; }
        public string Status { get; set; }
        public string Errors { get; set; }

        public Decimal? ValidAmount { get; set; }
        public DateTimeOffset? ValidTransactionDate { get; set; }
        public StatusCode ValidStatus { get; set; }
    }
}
