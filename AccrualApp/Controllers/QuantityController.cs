using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using AccrualApp.DBModels;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace AccrualApp.Controllers
{
    [Route("api/v2")]
    [ApiController]

    public class QuantityController : Controller
    {
        private readonly aci_databaseContext databaseContext = new aci_databaseContext();

        private readonly IWebHostEnvironment _webHostEnvironment;

        private readonly ILogger<TestController> _logger;

        public QuantityController(ILogger<TestController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        public Dictionary<String, String> getCompanyList()
        {
            _logger.LogInformation("Entering getACICompanyList...");
            Dictionary<String, String> companyList = new Dictionary<String, String>();
            List<Region> acicompanies = databaseContext.Region.ToList();

            foreach (Region company in acicompanies)
            {
                companyList.Add(company.RegionName, company.RegionId);
            }


            _logger.LogInformation("Exiting getACICompanyList\n...");
            return companyList;
        }


        public Dictionary<String, Dictionary<String, String>> getCustomerList()
        {
            _logger.LogInformation("Entering getACICustomerList...");
            Dictionary<String, Dictionary<String, String>> customerList = new Dictionary<String, Dictionary<String, String>>();
            List<Customer> customers = databaseContext.Customer.ToList();


            foreach (Customer customer in customers)
            {
                if (!!!customerList.ContainsKey(customer.CustomerName))
                {
                    customerList.Add(customer.CustomerName, new Dictionary<String, String>());
                }
                customerList.GetValueOrDefault(customer.CustomerName).Add(customer.RegionId, customer.CustomerId);
            }

            _logger.LogInformation("Exiting getACICustomerList\n...");
            return customerList;
        }


        [HttpGet]
        [Route("Quantity")]
        public IActionResult getQuantity()
        {
            // Specify a name for your top-level folder.
            string folderName = @_webHostEnvironment.ContentRootPath;

            long milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            string pathString = System.IO.Path.Combine(folderName, "ZipFiles", milliseconds.ToString());

            System.Globalization.CultureInfo provider = System.Globalization.CultureInfo.InvariantCulture;

            //create folder
            System.IO.Directory.CreateDirectory(pathString);

            _logger.LogInformation("Current Path:" + pathString);

            DateTime startDateObj = DateTime.ParseExact("2020-11-30", "yyyy-MM-dd", provider);
            DateTime endDateObj = DateTime.ParseExact("2020-12-06", "yyyy-MM-dd", provider);

            Dictionary<String, String> companyData = getCompanyList();

            Dictionary<String, Dictionary<String, String>> customerData = getCustomerList();

            Dictionary<String, List<String>> regionCustomer = new Dictionary<string, List<string>>();

            List<String> customerList = new List<String>();
            customerList.Add("ACI Last Mile CA LLC");
            customerList.Add("Acorn Newspapers");
            customerList.Add("Dow Jones & Company");
            customerList.Add("Gazette-Daily News");
            customerList.Add("HOY- FDS");
            customerList.Add("La Prensa");
            customerList.Add("LA Times Santa Barbara-Barrons");
            customerList.Add("Larchmont Chronicle");
            customerList.Add("LAT/OC HD");
            customerList.Add("Norwalk Patriot");
            customerList.Add("OCR - Rack and Stack");
            customerList.Add("Outlook/La Canada Flintridge");
            customerList.Add("San Bernardino Sun");
            //customerList.Add("San Diego Neighborhood News - East County");
            //customerList.Add("San Diego News - Chula Vista Star News");
            customerList.Add("San Pedro Today");
            customerList.Add("Santa Barbara Daily Press");
            //customerList.Add("SDUT HD");
            customerList.Add("SDUT-TMC");
            //customerList.Add("South Bay Digs");
            customerList.Add("The Downey Patriot");
            customerList.Add("The Epoch Times Media Group LA");
            customerList.Add("UT Community Press");
            customerList.Add("Valassis");
            customerList.Add("Ventura County Star (HD)");
            customerList.Add("Ventura County Star (TMC)");
            customerList.Add("Victorville TMC");


            regionCustomer.Add("california", customerList);

            customerList = new List<String>();
            customerList.Add("Shaw Media");
            customerList.Add("Cox Media Group Ohio");

            regionCustomer.Add("midwest", customerList);

            customerList = new List<String>();
            customerList.Add("Houston Chronicle Media Group");
            customerList.Add("San Antonio Express-News");
            customerList.Add("Dallas Morning News, Inc.");

            regionCustomer.Add("southwest", customerList);

            customerList = new List<String>();
            customerList.Add("Cox-Buyers Edge");
            //customerList.Add("Palm Beach Post");
            //customerList.Add("Sun-Sentinel Company, LLC");

            regionCustomer.Add("southeast", customerList);

            Console.WriteLine(startDateObj.AddDays(7));

            var workbook = new XLWorkbook();
            var sheet = workbook.AddWorksheet("Mapping");
            int column = 1;
            int row = 1;
            var currentRow = sheet.Row(row);

            foreach (KeyValuePair<String, List<String>> regionCustomerKeyValue in regionCustomer)
            {
                String currRegion = regionCustomerKeyValue.Key;
                List<String> customerlist = regionCustomer.GetValueOrDefault(currRegion);

                _logger.LogInformation("Company:" + currRegion);

                foreach (String customer in customerlist)
                {
                    Console.WriteLine(customer);
                    startDateObj = DateTime.ParseExact("2020-11-30", "yyyy-MM-dd", provider);
                    endDateObj = DateTime.ParseExact("2020-12-06", "yyyy-MM-dd", provider);

                    String customer_id = customerData.GetValueOrDefault(customer).FirstOrDefault(x => x.Key == currRegion).Value;
                    

                    for (int numberofWeeks = 1; numberofWeeks <= 11; numberofWeeks++)
                    {
                        JObject transactionData = getQuantityData(currRegion+"_102", startDateObj, endDateObj, currRegion, customer_id);
                        Console.WriteLine(transactionData.ToString());
                        currentRow.Cell(1).SetValue(currRegion+"_102");
                        currentRow.Cell(2).SetValue(customer_id);
                        currentRow.Cell(3).SetValue(currRegion);
                        foreach (var pair in transactionData)
                        {
                            currentRow.Cell(4).SetValue(pair.Key);
                            currentRow.Cell(5).SetValue(pair.Value.ToString());
                        }
                        currentRow.Cell(6).SetValue(endDateObj);
                        row = row + 1;
                        currentRow = sheet.Row(row);
                        transactionData = getQuantityData(currRegion +"_121", startDateObj, endDateObj, currRegion, customer_id);
                        Console.WriteLine(transactionData.ToString());
                        currentRow.Cell(1).SetValue(currRegion + "_121");
                        currentRow.Cell(2).SetValue(customer_id);
                        currentRow.Cell(3).SetValue(currRegion);
                        foreach (var pair in transactionData)
                        {
                            currentRow.Cell(4).SetValue(pair.Key);
                            currentRow.Cell(5).SetValue(pair.Value.ToString());
                        }
                        currentRow.Cell(6).SetValue(endDateObj);
                        row = row + 1;
                        currentRow = sheet.Row(row);
                        startDateObj = endDateObj.AddDays(1);
                        endDateObj = endDateObj.AddDays(7);
                    }

                }
            }
            String currFileName = "Quantity";
            currFileName = currFileName + ".xlsx";
            _logger.LogInformation("Curr File Name:" + currFileName);
            //write excel file to filesystem
            workbook.SaveAs(System.IO.Path.Combine(pathString, currFileName));
            String zipFile = System.IO.Path.Combine(folderName, "ZipFiles", milliseconds.ToString() + ".zip");

            return downloadZipFile(pathString, zipFile);
        }

        public IActionResult downloadZipFile(String folderName, String zipFileName)
        {
            ZipFile.CreateFromDirectory(folderName, zipFileName);
            Stream zipStream = new FileStream(zipFileName, FileMode.Open);

            return File(zipStream, "application/zip");
        }

        public JObject getQuantityData(String accountName, DateTime startDate, DateTime endDate, String regionId, String customerId)
        {
            JArray transactionData = new JArray() as dynamic;
            dynamic currRecord = new JObject();

            using (aci_databaseContext db = new aci_databaseContext())
            {
                var data = (from transaction in db.TransactionTmp where transaction.RegionId == regionId &&
                            transaction.CustomerId == customerId && transaction.AccountId == accountName
                            && transaction.TransactionDate >= startDate && transaction.TransactionDate <= endDate orderby transaction.TransactionDate select transaction.Qty).Sum();
                var amount = (from transaction in db.TransactionTmp where transaction.RegionId == regionId 
                              && transaction.CustomerId == customerId && transaction.AccountId == accountName
                              && transaction.TransactionDate >= startDate && transaction.TransactionDate <= endDate
                              orderby transaction.TransactionDate select transaction.Amount).Sum();
                /*            foreach (var d in data)
                            {
                                dynamic currRecord = new JObject();
                                currRecord.acc = d.AccountId;
                                currRecord.cust = d.CustomerId;
                                currRecord.reg = d.RegionId;
                                currRecord.balance = d.Amount;
                                currRecord.date = d.trans_date;
                                transactionData.Add(currRecord);
                            }*/
                //dynamic currRecord = new JObject();
                
                currRecord.Add(data.ToString(), amount);
                //currRecord.qty = data;
                //currRecord.amt = amount;
                //transactionData.Add(currRecord);
            }
            return currRecord;
        }
    }

}
