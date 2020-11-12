using CsvHelper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using Transaction.API.Models;
using Transaction.Entity;

namespace Transaction.API.Library
{
    /// <summary>
    /// 
    /// </summary>
    public class TransactionExtractor
    {
        private Strategy _strategy;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strategy"></param>
        public TransactionExtractor(Strategy strategy)
        {
            _strategy = strategy;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public List<UploadTransaction> ExtractTransaction(IFormFile file)
        {
            return _strategy.ExtractTransaction(file);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class Strategy
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public abstract List<UploadTransaction> ExtractTransaction(IFormFile file);
    }

    /// <summary>
    /// 
    /// </summary>
    public class CsvExtractor : Strategy
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public override List<UploadTransaction> ExtractTransaction(IFormFile file) {
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

    /// <summary>
    /// 
    /// </summary>
    public class XmlExtractor : Strategy
    {
        private readonly List<string> xmlValidStatusCode = new List<string>() { "Approved", "Rejected", "Done" };
        private const string XML_DATE_FORMAT = "yyyy-MM-ddTHH:mm:ss";
        private const int TRANSACTIONID_MAX_LENGTH = 50;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public override List<UploadTransaction> ExtractTransaction(IFormFile file)
        {
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                var entities = new List<UploadTransaction>();
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(reader);
                var nodeList = xmlDoc.SelectNodes("/Transactions/Transaction");
                foreach (XmlNode item in nodeList)
                {
                    var errors = new List<string>();
                    DateTimeOffset transDate;
                    Decimal amount;
                    var transactionId = (item.Attributes.GetNamedItem("id")?.Value ?? "").Trim();
                    if (transactionId.Length == 0 || transactionId.Length > TRANSACTIONID_MAX_LENGTH)
                        errors.Add("Invalid TransactionId");
                    var strTransDate = (item.SelectSingleNode("TransactionDate")?.InnerText ?? "").Trim();
                    if (!DateTimeOffset.TryParseExact(strTransDate, XML_DATE_FORMAT, CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal, out transDate))
                        errors.Add("Invalid Transaction Date");
                    var strAmount = (item.SelectSingleNode("PaymentDetails/Amount")?.InnerText ?? "").Trim();
                    if (!Decimal.TryParse(strAmount, out amount))
                        errors.Add("Invalid Payment Amount");
                    var currencyCode = (item.SelectSingleNode("PaymentDetails/CurrencyCode")?.InnerText ?? "").Trim();
                    if (currencyCode.Length != 3)
                        errors.Add("Invalid Currency Code");
                    var status = (item.SelectSingleNode("Status")?.InnerText ?? "").Trim();
                    if (!xmlValidStatusCode.Contains(status))
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
                case "Done":
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
