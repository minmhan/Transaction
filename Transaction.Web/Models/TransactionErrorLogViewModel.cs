using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transaction.Web.Models
{
    public class TransactionErrorLogViewModel
    {
        public int Id { get; set; }
        public int FileId { get; set; }
        public string TransactionId { get; set; }
        public string Amount { get; set; }
        public string CurrencyCode { get; set; }
        public string DateTime { get; set; }
        public string Status { get; set; }
        public string Error { get; set; }
        public string FileName { get; set; }
        public DateTime UploadedDate { get; set; }
    }
}
