using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using Transaction.API.Models;
using Transaction.Entity;

namespace Transaction.API.Library
{
    /// <summary>
    /// Mapping for CSV File
    /// </summary>
    public class CsvTransactionMap: ClassMap<UploadTransaction>
    {
        private readonly List<string> csvValidStatusCode = new List<string>() { "Approved", "Failed", "Finished" };
        private const string CSV_DATE_FORMAT = "dd/MM/yyyy HH:mm:ss";
        private const int CSV_TRANSACTIONID_LENGTH = 50;
       
        /// <summary>
        /// 
        /// </summary>
        public CsvTransactionMap()
        {
            Map(m => m.TransactionId).Index(0);
            Map(m => m.ValidAmount).Index(1).ConvertUsing((IReaderRow row) =>
            {
                Decimal.TryParse(row.GetField(1), out decimal result);
                return result;
            });
            Map(m => m.CurrencyCode).Index(2);
            Map(m => m.ValidTransactionDate).Index(3).ConvertUsing((IReaderRow row) => {
                // Assume Universal DateTime
                DateTimeOffset.TryParseExact(row.GetField(3)?.Trim(), CSV_DATE_FORMAT,
                    CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTimeOffset datetime);
                return datetime; 
            });
            Map(m => m.ValidStatus).Index(4).ConvertUsing((IReaderRow row) => {
                return GetCsvStatusCode(row.GetField(4)?.Trim());
            });
            Map(m => m.Status).Index(5).ConvertUsing((IReaderRow row) => {
                return row.GetField(5)?.Trim();
            });
            Map(m => m.Amount).Index(6).ConvertUsing((IReaderRow row) => {
                // Map for error log if decimal parsing fail
                return row.GetField(1)?.Trim();
            });
            Map(m => m.TransactionDate).Index(7).ConvertUsing((IReaderRow row) => {
                // Map for error log if date parsing fail
                return row.GetField(3)?.Trim();
            });
            Map(m => m.Errors).Index(8).ConvertUsing((IReaderRow row) =>
            {
                // Map for error log
                var errors = new List<string>();
                if (row.GetField(0)?.Trim().Length == 0 || row.GetField(0)?.Trim().Length > CSV_TRANSACTIONID_LENGTH)
                {
                    errors.Add("Invalid TransactionId");
                }
                if (!Decimal.TryParse(row.GetField(1), out decimal result))
                {
                    errors.Add("Invalid Payment Amount");
                }
                if (row.GetField(2)?.Trim().Length != 3)
                {
                    // May be check with list of valid currency code as well
                    errors.Add("Invalid Currency Code");
                }
                // // Assume Universal DateTime
                if (!DateTimeOffset.TryParseExact(row.GetField(3)?.Trim(), CSV_DATE_FORMAT, 
                    CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTimeOffset datetime))
                {
                    errors.Add("Invalid Transaction Date");
                }
                if (!csvValidStatusCode.Contains(row.GetField(4)?.Trim()))
                {
                    errors.Add("Invalid Status Code");
                }

                return string.Join(", ", errors);
            });
        }

        private StatusCode GetCsvStatusCode(string status)
        {
            StatusCode statusCode;
            switch (status)
            {
                case "Approved":
                    statusCode = StatusCode.A;
                    break;
                case "Finished":
                    statusCode = StatusCode.D;
                    break;
                default:
                    statusCode = StatusCode.R;
                    break;
            }
            return statusCode;
        }
    }
}
