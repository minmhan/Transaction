using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transaction.Web.Models
{
    public class TransactionFileViewModel
    {
        //[AllowedExtensions(new string[] { ".pdf", ".doc", ".docx", ".txt" })]
        public IFormFile FormFile { get; set; }
    }
}
