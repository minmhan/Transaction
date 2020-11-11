using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Transaction.Entity.Entity
{
    public class TransactionFile
    {
        public int Id { get; set; }
        [MaxLength(128)]
        public string FileName { get; set; }
        public DateTime UploadedDate { get; set; }
        //public string UploadedBy { get; set; }
    }
}
