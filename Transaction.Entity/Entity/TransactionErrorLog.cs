using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Transaction.Entity.Entity
{
    [Table("TransactionErrorLog")]
    public class TransactionErrorLog
    {
        // Relax Data Type and Max Length Validation, as ways of error is infinite
        // Using as varchar take 2 extra bytes + data.
        [Required]
        [Key]
        public int Id { get; set; }
        [Required]
        public int FileId { get; set; }
        public string TransactionId { get; set; }
        public string Amount { get; set; }
        public string CurrencyCode { get; set; }
        public string DateTime { get; set; }
        public string Status { get; set; }
        public string Error { get; set; }
    }

}
