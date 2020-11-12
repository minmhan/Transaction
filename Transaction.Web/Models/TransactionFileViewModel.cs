using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Transaction.Web.Utils;

namespace Transaction.Web.Models
{
    public class TransactionFileViewModel
    {
        [DataType(DataType.Upload)]
        [MaxFileSize(1 * 1024 * 1024, ErrorMessage = "Maximum allowed file size is 2 MB")]
        [AllowedExtensions(new string[] { ".csv", ".xml" })]
        public IFormFile FormFile { get; set; }
    }
}
