using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using AccrualApp.DBModels;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace AccrualApp.Controllers
{
    [Route("api/v2")]
    [ApiController]
    public class BudgetController : ControllerBase
    {
        private readonly aci_databaseContext databaseContext = new aci_databaseContext();

        private readonly IWebHostEnvironment _webHostEnvironment;

        private readonly ILogger<BudgetController> _logger;

        public BudgetController(ILogger<BudgetController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }
        public Dictionary<String, Dictionary<String, String>> getRegCusId()
        {
            _logger.LogInformation("Entering Customer Table...");
            Dictionary<String, Dictionary<String, String>> customerList = new Dictionary<String, Dictionary<String, String>>();
            List<Customer> customers = databaseContext.Customer.ToList();


            foreach (Customer customer in customers)
            {
                if (!!!customerList.ContainsKey(customer.RegionId))
                {
                    customerList.Add(customer.RegionId, new Dictionary<String, String>());
                }
                customerList.GetValueOrDefault(customer.RegionId).Add(customer.CustomerId, customer.CustomerName);
            }

            _logger.LogInformation("Exiting getRegCusId\n...");
            return customerList;
        }

        public Dictionary<String, String> getAccountId()
        {

            Dictionary<String, String> accountItems = new Dictionary<String, String>();

            List<Account> items = databaseContext.Account.ToList();

            foreach (Account line in items)
            {
                accountItems.Add(line.AccountId, line.AccountNum);
            }
            return accountItems;
        }


        [HttpPost]
        [Route("test")]
        public IActionResult getCustomerList(IFormFile mappingFile)
        {
            Dictionary<String, String> accId = new Dictionary<String, String>();
            
            Dictionary<String, Dictionary<String, String>> regCus = new Dictionary<String, Dictionary<String, String>>();

            accId = getAccountId();
            regCus = getRegCusId();

            int id = 7575;
            int currentRow = 1;
            int dateIndexStart = 1;
            String reformattedStrDate = "";




            var workbook = new XLWorkbook();
            //create sheet
            var currentCustomerWorkSheet = workbook.AddWorksheet("Mapping");

            string folderName = @_webHostEnvironment.ContentRootPath;

            long milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            string pathString = System.IO.Path.Combine(folderName, "ZipFiles", milliseconds.ToString());

            //create folder
            System.IO.Directory.CreateDirectory(pathString);

            _logger.LogInformation("Current Path:" + pathString);

            var mappingWorkbook = new XSSFWorkbook(mappingFile.OpenReadStream());

            ISheet sheet = mappingWorkbook.GetSheetAt(40);

            String regionId = "advertisingconsultants";

            String companyName = sheet.GetRow(0).GetCell(0).ToString();

            int dateIdexCount = sheet.GetRow(1).PhysicalNumberOfCells;
            try
            {
                while (dateIndexStart<=dateIdexCount-1)
                {
                    Dictionary<String, decimal> mappedVal = new Dictionary<String,decimal>();

                    String date = sheet.GetRow(1).GetCell(dateIndexStart).ToString();
                    Console.WriteLine(date);
                    SimpleDateFormat myFormat = new SimpleDateFormat("yyyy-MM-dd");

                    try
                    {
                        reformattedStrDate = myFormat.Format(myFormat.Parse(date));
                    }
                    catch (Exception e)
                    {
                        e.ToString();
                    }

                    String customerId = regCus.GetValueOrDefault(regionId).FirstOrDefault(x => x.Value == companyName).Key;

                    int start = 10;

                    for (int row = start; row <= 160; row++)
                    {
                        Console.WriteLine(sheet.GetRow(row).GetCell(0).ToString());
                        if (sheet.GetRow(row).GetCell(0).ToString() == "Total Income" ||
                            sheet.GetRow(row).GetCell(0).ToString() == "Cost of Goods Sold" ||
                             sheet.GetRow(row).GetCell(0).ToString() == "Rate" ||
                              sheet.GetRow(row).GetCell(0).ToString() == "Quantity" ||
                              sheet.GetRow(row).GetCell(0).ToString() == "Total COGS" ||
                              sheet.GetRow(row).GetCell(0).ToString() == "Gross Profit" ||
                              sheet.GetRow(row).GetCell(0).ToString() == "Expense" ||
                              sheet.GetRow(row).GetCell(0).ToString() == "Total Expense" ||
                              sheet.GetRow(row).GetCell(0).ToString() == "Net Ordinary Income" ||
                              sheet.GetRow(row).GetCell(0).ToString() == "Other Income/Expense" ||
                              sheet.GetRow(row).GetCell(0).ToString() == "Other Income" ||
                              sheet.GetRow(row).GetCell(0).ToString() == "Total Other Income" ||
                              sheet.GetRow(row).GetCell(0).ToString() == "Total Other Expense" ||
                              sheet.GetRow(row).GetCell(0).ToString() == "Other Expense" ||
                              sheet.GetRow(row).GetCell(0).ToString() == "EBITDA" ||
                              sheet.GetRow(row).GetCell(0).ToString() == "Net Income")
                        {

                        }
                        else if (sheet.GetRow(row).GetCell(dateIndexStart).CellType == CellType.Blank || sheet.GetRow(row).GetCell(dateIndexStart).ToString() == "0.00")
                        {
                            Console.WriteLine(sheet.GetRow(row).GetCell(0).ToString());
                        }
                        else if (sheet.GetRow(row).GetCell(0).ToString().Contains("- Other"))
                        {

                        }
                        else if (sheet.GetRow(row).GetCell(0).ToString().Contains("LAT breakup fee/ misc income"))
                        {
                            int val = int.Parse(sheet.GetRow(row).GetCell(dateIndexStart).ToString().Replace(",",""));
                            mappedVal.Add("-13",val);
                        }
                        else
                        {
                            if ((row >= 10 && row <= 29) || (row >= 145 && row <= 150))
                            {
                                String val = sheet.GetRow(row).GetCell(dateIndexStart).ToString();
                                decimal va = decimal.Parse(val);
                                Console.WriteLine(sheet.GetRow(row).GetCell(0).ToString());
                                String[] splitWord = sheet.GetRow(row).GetCell(0).ToString().Split("·");
                                mappedVal.Add(accId.FirstOrDefault(x => x.Value == splitWord[0].Trim()).Key, va);
                            }
                            else 
                            {
                                String val = sheet.GetRow(row).GetCell(dateIndexStart).ToString();
                                decimal va = decimal.Parse(val);
                                Console.WriteLine(sheet.GetRow(row).GetCell(0).ToString());
                                String[] splitWord = sheet.GetRow(row).GetCell(0).ToString().Split("·");
                                mappedVal.Add(accId.FirstOrDefault(x => x.Value == splitWord[0].Trim()).Key, va*-1);
                            }
                           
                        }

                    }

                    foreach (var keyval in mappedVal)
                    {
                        currentCustomerWorkSheet.Row(currentRow).Cell(1).SetValue(id);
                        currentCustomerWorkSheet.Row(currentRow).Cell(2).SetValue(keyval.Key);
                        currentCustomerWorkSheet.Row(currentRow).Cell(3).SetValue(regionId);
                        currentCustomerWorkSheet.Row(currentRow).Cell(4).SetValue(customerId);
                        currentCustomerWorkSheet.Row(currentRow).Cell(5).SetValue(reformattedStrDate);
                        currentCustomerWorkSheet.Row(currentRow).Cell(6).SetValue(keyval.Value);
                        currentRow += 1;
                        id += 1;


                    }
                    dateIndexStart += 1;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }


            //writing to excel
            String currFileName = "Mapping_" + milliseconds;
            currFileName = currFileName.Replace("/", "_") + ".xlsx";
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


    }
}
