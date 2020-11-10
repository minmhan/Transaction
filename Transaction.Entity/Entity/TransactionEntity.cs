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
        // Not really need incremental Id keys, but using primary key as Transaction Identifier(Text) will impace db (as cluster index)
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
        public string CurrencyCode { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public int Status { get; set; }
    }
}
