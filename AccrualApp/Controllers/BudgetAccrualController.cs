using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using AccrualApp.Constants;
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
    [Route("api/")]
    [ApiController]
    public class BudgetAccrualController : ControllerBase
    {
        private readonly aci_databaseContext databaseContext = new aci_databaseContext();

        private readonly IWebHostEnvironment _webHostEnvironment;

        private readonly ILogger<BudgetAccrualController> _logger;

        public BudgetAccrualController(ILogger<BudgetAccrualController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        public Dictionary<int, Dictionary<String, int>> getRegCusId()
        {
            _logger.LogInformation("Entering Customer Table...");
            Dictionary<int, Dictionary<String, int>> customerList = new Dictionary<int, Dictionary<String, int>>();
            List<AcicustomerMaster> customers = databaseContext.AcicustomerMaster.ToList();


            foreach (AcicustomerMaster customer in customers)
            {
                if (!!!customerList.ContainsKey(customer.AcicompanyId))
                {
                    customerList.Add(customer.AcicompanyId, new Dictionary<String, int>());
                }
                customerList.GetValueOrDefault(customer.AcicompanyId).Add(customer.AcicustomerName, customer.AcicustomerId);
            }

            _logger.LogInformation("Exiting getRegCusId\n...");
            return customerList;
        }

        public Dictionary<int, String> getLineitems()
        {

            Dictionary<int, String> lineItems = new Dictionary<int, String>();

            List<AciitemMaster> items = databaseContext.AciitemMaster.ToList();

            foreach (AciitemMaster line in items)
            {
                lineItems.Add(line.AcilineItemId, line.QbaccountNum);
            }
            return lineItems;
        }


        [HttpPost]
        [Route("weeklyAccrual/{regionName}")]
        public IActionResult weeklyAccrual(IFormFile mappingFile,String regionName)
        {
            
            //for storing regionname, customer and id
            Dictionary<int, Dictionary<String, int>> regCus = new Dictionary<int, Dictionary<String, int>>();

            Mapping map = new Mapping();

            Dictionary<String,int> companyId = map.regionId();

            int regId = companyId.GetValueOrDefault(regionName);
            
            regCus = getRegCusId();

            Dictionary<int, String> lineItems = getLineitems();


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

            ISheet sheet = mappingWorkbook.GetSheetAt(0);

            String regionId = regionName;

            String companyName = sheet.GetRow(0).GetCell(0).ToString();

            int dateIdexCount = sheet.GetRow(1).PhysicalNumberOfCells;
            try
            {
                while (dateIndexStart <= dateIdexCount - 1)
                {
                    KeyValuePair<decimal, int> mappedVal = new KeyValuePair<decimal,int>();
                    Dictionary<decimal,int> mappedVal1 = new Dictionary<decimal, int>();

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

                    int customerId = regCus.GetValueOrDefault(regId).FirstOrDefault(x => x.Key == companyName).Value;

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
                            decimal val = decimal.Parse(sheet.GetRow(row).GetCell(dateIndexStart).ToString().Replace(",", ""));

                            //first line item
                            currentCustomerWorkSheet.Row(currentRow).Cell(1).SetValue(regId);
                            currentCustomerWorkSheet.Row(currentRow).Cell(2).SetValue(customerId);
                            currentCustomerWorkSheet.Row(currentRow).Cell(3).SetValue(-13);
                            currentCustomerWorkSheet.Row(currentRow).Cell(5).SetValue(reformattedStrDate);
                            currentCustomerWorkSheet.Row(currentRow).Cell(4).SetValue(val * -1);
                            currentRow++;

                            //second line item
                            currentCustomerWorkSheet.Row(currentRow).Cell(1).SetValue(regId);
                            currentCustomerWorkSheet.Row(currentRow).Cell(2).SetValue(customerId);
                            currentCustomerWorkSheet.Row(currentRow).Cell(3).SetValue(95);
                            currentCustomerWorkSheet.Row(currentRow).Cell(5).SetValue(reformattedStrDate);
                            currentCustomerWorkSheet.Row(currentRow).Cell(4).SetValue(val);
                            currentRow++;
                        }
                        else
                        {
                            //for revenue
                            if ((row >= 10 && row <= 29))
                            {
                                String val = sheet.GetRow(row).GetCell(dateIndexStart).ToString();
                                decimal va = decimal.Parse(val);
                                Console.WriteLine(sheet.GetRow(row).GetCell(0).ToString());
                                String[] splitWord = sheet.GetRow(row).GetCell(0).ToString().Split("·");

                                //first line item
                                currentCustomerWorkSheet.Row(currentRow).Cell(1).SetValue(regId);
                                currentCustomerWorkSheet.Row(currentRow).Cell(2).SetValue(customerId);
                                currentCustomerWorkSheet.Row(currentRow).Cell(3).SetValue(40);
                                currentCustomerWorkSheet.Row(currentRow).Cell(5).SetValue(reformattedStrDate);
                                currentCustomerWorkSheet.Row(currentRow).Cell(4).SetValue(va*-1);
                                currentRow++;

                                //second line item
                                currentCustomerWorkSheet.Row(currentRow).Cell(1).SetValue(regId);
                                currentCustomerWorkSheet.Row(currentRow).Cell(2).SetValue(customerId);
                                currentCustomerWorkSheet.Row(currentRow).Cell(3).SetValue(lineItems.FirstOrDefault(x => x.Value == splitWord[0].Trim()).Key);
                                currentCustomerWorkSheet.Row(currentRow).Cell(5).SetValue(reformattedStrDate);
                                currentCustomerWorkSheet.Row(currentRow).Cell(4).SetValue(va);
                                currentRow++;

                            }
                            //for cogs
                            else if ((row >= 34 && row <= 55))
                            {
                                String val = sheet.GetRow(row).GetCell(dateIndexStart).ToString();
                                decimal va = decimal.Parse(val);
                                Console.WriteLine(sheet.GetRow(row).GetCell(0).ToString());
                                String[] splitWord = sheet.GetRow(row).GetCell(0).ToString().Split("·");

                                //first line item
                                currentCustomerWorkSheet.Row(currentRow).Cell(1).SetValue(regId);
                                currentCustomerWorkSheet.Row(currentRow).Cell(2).SetValue(customerId);
                                currentCustomerWorkSheet.Row(currentRow).Cell(3).SetValue(lineItems.FirstOrDefault(x => x.Value == splitWord[0].Trim()).Key);
                                currentCustomerWorkSheet.Row(currentRow).Cell(5).SetValue(reformattedStrDate);
                                currentCustomerWorkSheet.Row(currentRow).Cell(4).SetValue(va*-1);
                                currentRow++;

                                //second line item
                                currentCustomerWorkSheet.Row(currentRow).Cell(1).SetValue(regId);
                                currentCustomerWorkSheet.Row(currentRow).Cell(2).SetValue(customerId);
                                currentCustomerWorkSheet.Row(currentRow).Cell(3).SetValue(98);
                                currentCustomerWorkSheet.Row(currentRow).Cell(5).SetValue(reformattedStrDate);
                                currentCustomerWorkSheet.Row(currentRow).Cell(4).SetValue(va);
                                currentRow++;


                            }
                            else if ((row >= 59 && row <= 137))
                            {
                                String val = sheet.GetRow(row).GetCell(dateIndexStart).ToString();
                                decimal va = decimal.Parse(val);
                                Console.WriteLine(sheet.GetRow(row).GetCell(0).ToString());
                                String[] splitWord = sheet.GetRow(row).GetCell(0).ToString().Split("·");

                                //first line item
                                currentCustomerWorkSheet.Row(currentRow).Cell(1).SetValue(regId);
                                currentCustomerWorkSheet.Row(currentRow).Cell(2).SetValue(customerId);
                                currentCustomerWorkSheet.Row(currentRow).Cell(3).SetValue(lineItems.FirstOrDefault(x => x.Value == splitWord[0].Trim()).Key);
                                currentCustomerWorkSheet.Row(currentRow).Cell(5).SetValue(reformattedStrDate);
                                currentCustomerWorkSheet.Row(currentRow).Cell(4).SetValue(va * -1);
                                currentRow++;

                                //second line item
                                currentCustomerWorkSheet.Row(currentRow).Cell(1).SetValue(regId);
                                currentCustomerWorkSheet.Row(currentRow).Cell(2).SetValue(customerId);
                                currentCustomerWorkSheet.Row(currentRow).Cell(3).SetValue(95);
                                currentCustomerWorkSheet.Row(currentRow).Cell(5).SetValue(reformattedStrDate);
                                currentCustomerWorkSheet.Row(currentRow).Cell(4).SetValue(va);
                                currentRow++;

                            }
                            else if ((row >= 142 && row <= 146))
                            {
                                String val = sheet.GetRow(row).GetCell(dateIndexStart).ToString();
                                decimal va = decimal.Parse(val);
                                Console.WriteLine(sheet.GetRow(row).GetCell(0).ToString());
                                String[] splitWord = sheet.GetRow(row).GetCell(0).ToString().Split("·");

                                //first line item
                                currentCustomerWorkSheet.Row(currentRow).Cell(1).SetValue(regId);
                                currentCustomerWorkSheet.Row(currentRow).Cell(2).SetValue(customerId);
                                currentCustomerWorkSheet.Row(currentRow).Cell(3).SetValue(lineItems.FirstOrDefault(x => x.Value == splitWord[0].Trim()).Key);
                                currentCustomerWorkSheet.Row(currentRow).Cell(5).SetValue(reformattedStrDate);
                                currentCustomerWorkSheet.Row(currentRow).Cell(4).SetValue(va);
                                currentRow++;

                                //second line item
                                currentCustomerWorkSheet.Row(currentRow).Cell(1).SetValue(regId);
                                currentCustomerWorkSheet.Row(currentRow).Cell(2).SetValue(customerId);
                                currentCustomerWorkSheet.Row(currentRow).Cell(3).SetValue(95);
                                currentCustomerWorkSheet.Row(currentRow).Cell(5).SetValue(reformattedStrDate);
                                currentCustomerWorkSheet.Row(currentRow).Cell(4).SetValue(va*-1);
                                currentRow++;
                             
                            }
                            else 
                            {
                                String val = sheet.GetRow(row).GetCell(dateIndexStart).ToString();
                                decimal va = decimal.Parse(val);
                                Console.WriteLine(sheet.GetRow(row).GetCell(0).ToString());
                                String[] splitWord = sheet.GetRow(row).GetCell(0).ToString().Split("·");


                                //first line item
                                currentCustomerWorkSheet.Row(currentRow).Cell(1).SetValue(regId);
                                currentCustomerWorkSheet.Row(currentRow).Cell(2).SetValue(customerId);
                                currentCustomerWorkSheet.Row(currentRow).Cell(3).SetValue(lineItems.FirstOrDefault(x => x.Value == splitWord[0].Trim()).Key);
                                currentCustomerWorkSheet.Row(currentRow).Cell(5).SetValue(reformattedStrDate);
                                currentCustomerWorkSheet.Row(currentRow).Cell(4).SetValue(va*-1);
                                currentRow++;

                                //second line item
                                currentCustomerWorkSheet.Row(currentRow).Cell(1).SetValue(regId);
                                currentCustomerWorkSheet.Row(currentRow).Cell(2).SetValue(customerId);
                                currentCustomerWorkSheet.Row(currentRow).Cell(3).SetValue(95);
                                currentCustomerWorkSheet.Row(currentRow).Cell(5).SetValue(reformattedStrDate);
                                currentCustomerWorkSheet.Row(currentRow).Cell(4).SetValue(va);
                                currentRow++;

                            }

                        }

                    }
                    dateIndexStart += 1;
                }
            }
            catch (Exception e)
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

