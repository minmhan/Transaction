using CsvHelper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Transaction.API.Models;
using Transaction.Entity;

namespace Transaction.API.Library
{
    public class EntityExtractor
    {
        private Strategy _strategy;
        public EntityExtractor(Strategy strategy)
        {
            _strategy = strategy;
        }

        public List<UploadTransaction> ExtractEntity(IFormFile file)
        {
            return _strategy.ExtractEntity(file);
        }
    }

    public abstract class Strategy
    {
        public abstract List<UploadTransaction> ExtractEntity(IFormFile file);
    }
    public class CsvExtractor : Strategy
    {
        public override List<UploadTransaction> ExtractEntity(IFormFile file) {
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                using (var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csvReader.Configuration.HasHeaderRecord = false;
                    csvReader.Configuration.MissingFieldFound = null;
                    csvReader.Configuration.RegisterClassMap<CsvTransactionMap>();
                    var transactions = csvReader.GetRecords<UploadTransaction>().ToList();

                    return transactions;
                }
            }
        }
    }

    public class XmlExtractor : Strategy
    {
        public override List<UploadTransaction> ExtractEntity(IFormFile file)
        {
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                // xml file
                var entities = new List<UploadTransaction>();
                //var errorLog = new List<TransactionErrorLog>();
                string dateFormatString = "yyyy-MM-ddTHH:mm:ss";
                var validStatusCode = new List<string>() { "Approved", "Rejected", "Done" };
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(reader);
                var nodeList = xmlDoc.SelectNodes("/Transactions/Transaction");
                foreach (XmlNode item in nodeList)
                {
                    var errors = new List<string>();
                    DateTimeOffset transDate;
                    Decimal amount;
                    var transactionId = (item.Attributes.GetNamedItem("id")?.Value ?? "").Trim();
                    if (transactionId.Length == 0 || transactionId.Length > 50)
                        errors.Add("Invalid TransactionId");
                    var strTransDate = (item.SelectSingleNode("TransactionDate")?.InnerText ?? "").Trim();
                    if (!DateTimeOffset.TryParseExact(strTransDate, dateFormatString, CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal, out transDate))
                        errors.Add("Invalid Transaction Date");
                    var strAmount = (item.SelectSingleNode("PaymentDetails/Amount")?.InnerText ?? "").Trim();
                    if (!Decimal.TryParse(strAmount, out amount))
                        errors.Add("Invalid Payment Amount");
                    var currencyCode = (item.SelectSingleNode("PaymentDetails/CurrencyCode")?.InnerText ?? "").Trim();
                    if (currencyCode.Length != 3)
                        errors.Add("Invalid Currency Code");
                    var status = (item.SelectSingleNode("Status")?.InnerText ?? "").Trim();
                    if (!validStatusCode.Contains(status))
                        errors.Add("Invalid Status Code");

                    var xmlTransaction = new UploadTransaction()
                    {
                        TransactionId = transactionId,
                        Amount = strAmount,
                        ValidAmount = amount,
                        CurrencyCode = currencyCode,
                        TransactionDate = strTransDate,
                        ValidTransactionDate = transDate,
                        Status = status,
                        ValidStatus = GetXmlStatusCode(status),
                        Errors = string.Join(", ", errors)
                    };
                    entities.Add(xmlTransaction);
                }

                return entities;
            }
        }

        private StatusCode GetXmlStatusCode(string status)
        {
            StatusCode statusCode;
            switch (status)
            {
                case "Approved":
                    statusCode = StatusCode.A;
                    break;
                case "Rejected":
                    statusCode = StatusCode.R;
                    break;
                case "Done":
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
