using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json.Serialization;

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
        // As an exercise, assume transactionId is not unique.
        public string TransactionId { get; set; }
        [Required]
        [Column(TypeName = "decimal(19,2)")]
        public decimal Amount { get; set; }
        [Required]
        [MaxLength(3)]
        public string CurrencyCode { get; set; }
        [Required]
        public DateTime DateTime { get; set; }
        public StatusCode Status { get; set; }
        // Probably better this table without meta data, but lost traceability
        //public string FileName { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum StatusCode
    {
        [Description("Approved")]
        A,
        [Description("Rejected")]
        R,
        [Description("Done")]
        D
    }
}
