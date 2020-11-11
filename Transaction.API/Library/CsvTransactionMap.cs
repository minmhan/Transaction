using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using Transaction.API.Models;
using Transaction.Entity;

namespace Transaction.API.Library
{
    public class CsvTransactionMap: ClassMap<UploadTransaction>
    {
        private List<string> validStatusCode = new List<string>() { "Approved", "Failed", "Finished" };
        private string dateFormatString = "dd/MM/yyyy HH:mm:ss";
        public CsvTransactionMap()
        {
            Map(m => m.TransactionId).Index(0);
            Map(m => m.ValidAmount).Index(1);
            Map(m => m.CurrencyCode).Index(2);
            Map(m => m.ValidTransactionDate).Index(3).ConvertUsing((IReaderRow row) => {
                DateTimeOffset.TryParseExact(row.GetField(3)?.Trim(), dateFormatString,
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
                if (row.GetField(0)?.Trim().Length == 0 || row.GetField(0)?.Trim().Length > 50)
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
                if (!DateTimeOffset.TryParseExact(row.GetField(3)?.Trim(), dateFormatString, 
                    CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTimeOffset datetime))
                {
                    errors.Add("Invalid Transaction Date");
                }
                if (!validStatusCode.Contains(row.GetField(4)?.Trim()))
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
                case "Failed":
                    statusCode = StatusCode.R;
                    break;
                case "Finished":
                    statusCode = StatusCode.D;
                    break;
                default:
                    statusCode = StatusCode._;
                    break;
            }
            return statusCode;
        }
    }
}
