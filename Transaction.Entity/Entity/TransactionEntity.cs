using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Transaction.Entity
{
    [Table("Transaction")]
    public class TransactionEntity
    {
        // Not really need incremental Id keys, 
        // because using primary key as Transaction Identifier(Text) impact db (as cluster index)
        [Required]
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string TransactionId { get; set; }
        [Required]
        [Column(TypeName = "decimal(19,4)")]
        public decimal Amount { get; set; }
        [Required]
        [MaxLength(3)]
        public string CurrencyCode { get; set; }
        [Required]
        public DateTime DateTime { get; set; }
        public StatusCode Status { get; set; }
        // Probably better this table without meta data, but cost lost traceability
        //public string FileName { get; set; }
    }

    public enum StatusCode
    {
        _,
        A,
        R,
        D
    }
}
