using System;
using System.Collections.Generic;
using System.Linq;
using AccrualApp.DBModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office.CustomUI;
using System.IO.Compression;
using System.IO;
using Newtonsoft.Json;
using System.Net;
using NPOI.HSSF.UserModel;
using NPOI.Util;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;

namespace AccrualApp.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class TestController : Controller
    {
        private readonly aci_databaseContext databaseContext = new aci_databaseContext();

        private readonly IWebHostEnvironment _webHostEnvironment;

        private readonly ILogger<TestController> _logger;

        public TestController(ILogger<TestController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }


        public Dictionary<String, int> getACICompanyList()
        {
            _logger.LogInformation("Entering getACICompanyList...");
            Dictionary<String, int> companyList = new Dictionary<String, int>();
            List<AcicompanyMaster> acicompanies = databaseContext.AcicompanyMaster.ToList();

            foreach (AcicompanyMaster company in acicompanies)
            {
                companyList.Add(company.AcicompanyName, company.AcicompanyId);
            }


            _logger.LogInformation("Exiting getACICompanyList\n...");
            return companyList;
        }


        public Dictionary<String, Dictionary<String, String>> getACICustomerList()
        {
            _logger.LogInformation("Entering getACICustomerList...");
            Dictionary<String, Dictionary<String, String>> customerList = new Dictionary<String, Dictionary<String, String>>();
            List<Customer> customers = databaseContext.Customer.ToList();


            foreach (Customer customer in customers)
            {
                if (!!!customerList.ContainsKey(customer.CustomerName))
                {
                    customerList.Add(customer.CustomerName, new Dictionary<string, string>());
                }
                customerList.GetValueOrDefault(customer.CustomerName).Add(customer.RegionId, customer.CustomerId);
            }

            _logger.LogInformation("Exiting getACICustomerList\n...");
            return customerList;
        }


        [HttpGet]
        [Route("customers")]
        public ActionResult getCustomerList()
        {
            _logger.LogTrace("Entering getCustomerList...");
            Dictionary<String, List<String>> retVal = new Dictionary<string, List<string>>();
            using (aci_databaseContext db = new aci_databaseContext())
            {
                var data = (from customer in db.Customer
                            select new
                            {
                                customerName = customer.CustomerName,
                                companyId = customer.RegionId
                            });
                foreach (var d in data)
                {
                    if (!!!retVal.ContainsKey(d.companyId))
                    {
                        retVal.Add(d.companyId, new List<string>());
                    }
                    retVal.GetValueOrDefault(d.companyId).Add(d.customerName);
                }
            }

            _logger.LogTrace("Exiting getCustomerList\n...");
            return Ok(new
            {
                status = "SUCCESS",
                statuscode = HttpStatusCode.OK,
                message = "customers....",
                data = retVal
            });
        }

        [HttpPost]
        [Route("vacationpto/{startdate}/{enddate}")]
        public IActionResult getVacationPTO(String startDate, String endDate, IFormFile previousMonthFile, IFormFile currentMonthFile)
        {


            //TODO:
            //Configure this values
            // previous month file configuration
            int previousMonthXLRowStartNum = 5;
            int previousMonthXLEmpNameColumnNum = 1;
            int previousMonthXLLOCColumnNum = 3;
            int previousMonthXLDeptColumnNum = 4;
            int previousMonthXLCashBLColumnNum = 13;


            // current month file configuration
            int currentMonthXLRowStartNum = 5;
            int currentMonthXLEmpNameColumnNum = 1;
            int currentMonthXLLOCColumnNum = 3;
            int currentMonthXLDeptColumnNum = 4;
            int currentMonthXLCashBLColumnNum = 13;




            List<ISheet> previouMonthsheets = new List<ISheet>(); //Create the ISheet object to read the sheet cell values  
            var fileExt = Path.GetExtension(previousMonthFile.FileName);



            if (fileExt == ".xls")
            {
                var previousMonthWorkbook = new HSSFWorkbook(previousMonthFile.OpenReadStream()); //HSSWorkBook object will read the Excel 97-2000 formats  

                for (int sheetIndex = 0; sheetIndex < previousMonthWorkbook.NumberOfSheets; sheetIndex++)
                {
                    ISheet sheet = previousMonthWorkbook.GetSheetAt(sheetIndex);
                    previouMonthsheets.Add(previousMonthWorkbook.GetSheetAt(sheetIndex));
                }
            }
            else
            {
                var previousMonthWorkbook = new XSSFWorkbook(previousMonthFile.OpenReadStream()); //XSSFWorkBook will read 2007 Excel format  
                for (int sheetIndex = 0; sheetIndex < previousMonthWorkbook.NumberOfSheets; sheetIndex++)
                {
                    previouMonthsheets.Add(previousMonthWorkbook.GetSheetAt(sheetIndex));
                }
            }

            Dictionary<String, Double> customerCashBLMap = new Dictionary<string, double>();

            foreach (ISheet sheet in previouMonthsheets)
            {
                _logger.LogInformation("Curr SheetName:" + sheet.SheetName);

                int rowIndex = previousMonthXLRowStartNum;

                while (sheet.GetRow(rowIndex) != null)
                {
                    var row = sheet.GetRow(rowIndex);
                    //all cells are empty, so is a 'blank row'
                    if (row.Cells.All(d => d.CellType == NPOI.SS.UserModel.CellType.Blank)) break;


                    String empName = sheet.GetRow(rowIndex).GetCell(previousMonthXLEmpNameColumnNum).StringCellValue;
                    double cashBalance = sheet.GetRow(rowIndex).GetCell(previousMonthXLCashBLColumnNum).NumericCellValue;
                    _logger.LogInformation("Emp Name:" + empName + " | Cash Balance:" + cashBalance);
                    if (empName.Length == 0)
                    {
                        break;
                    }
                    customerCashBLMap.Add(empName, cashBalance);
                    rowIndex++;
                }
            }

            /**
             * Current Month File Operation
             */

            List<ISheet> currentMonthsheets = new List<ISheet>(); //Create the ISheet object to read the sheet cell values  
            fileExt = Path.GetExtension(currentMonthFile.FileName);



            if (fileExt == ".xls")
            {
                var currentMonthWorkbook = new HSSFWorkbook(currentMonthFile.OpenReadStream()); //HSSWorkBook object will read the Excel 97-2000 formats  

                for (int sheetIndex = 0; sheetIndex < currentMonthWorkbook.NumberOfSheets; sheetIndex++)
                {
                    currentMonthsheets.Add(currentMonthWorkbook.GetSheetAt(sheetIndex));
                }
            }
            else
            {
                var currentMonthWorkbook = new XSSFWorkbook(currentMonthFile.OpenReadStream()); //XSSFWorkBook will read 2007 Excel format  
                for (int sheetIndex = 0; sheetIndex < currentMonthWorkbook.NumberOfSheets; sheetIndex++)
                {
                    currentMonthsheets.Add(currentMonthWorkbook.GetSheetAt(sheetIndex));
                }
            }

            //write previous month cash balance in current month file

            foreach (ISheet sheet in currentMonthsheets)
            {
                _logger.LogInformation("Curr SheetName:" + sheet.SheetName);

                int rowIndex = currentMonthXLRowStartNum;

                while (sheet.GetRow(rowIndex) != null && sheet.GetRow(rowIndex).GetCell(currentMonthXLEmpNameColumnNum) != null)
                {
                    var row = sheet.GetRow(rowIndex);
                    //all cells are empty, so is a 'blank row'
                    if (row.Cells.All(d => d.CellType == NPOI.SS.UserModel.CellType.Blank)) break;


                    String empName = sheet.GetRow(rowIndex).GetCell(currentMonthXLEmpNameColumnNum).StringCellValue;
                    double cashBalance = sheet.GetRow(rowIndex).GetCell(currentMonthXLCashBLColumnNum).NumericCellValue;
                    _logger.LogInformation("Emp Name:" + empName + " | Cash Balance:" + cashBalance);

                    double lastMonthCashBalance = 0;
                    if (customerCashBLMap.ContainsKey(empName))
                    {
                        lastMonthCashBalance = customerCashBLMap.GetValueOrDefault(empName);
                    }
                    //write last month balance
                    sheet.GetRow(rowIndex).GetCell(currentMonthXLCashBLColumnNum + 1).SetCellValue(lastMonthCashBalance);

                    sheet.GetRow(rowIndex).GetCell(currentMonthXLCashBLColumnNum + 2).SetCellValue(cashBalance - lastMonthCashBalance);


                    rowIndex++;
                }
            }


            HSSFWorkbook finalWorkbook = new HSSFWorkbook();

            foreach (ISheet sheet in currentMonthsheets)
            {

                sheet.CopyTo(finalWorkbook, sheet.SheetName, true, true);
            }

            // Specify a name for your top-level folder.
            string folderName = @_webHostEnvironment.ContentRootPath;

            long milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            string pathString = System.IO.Path.Combine(folderName, "ZipFiles", milliseconds.ToString());

            //create folder
            System.IO.Directory.CreateDirectory(pathString);

            // Write Excel to disk 
            using (var fileData = new FileStream(System.IO.Path.Combine(pathString, currentMonthFile.Name + ".xls"), FileMode.Create))
            {
                finalWorkbook.Write(fileData);
            }


            String zipFile = System.IO.Path.Combine(folderName, "ZipFiles", milliseconds.ToString() + ".zip");

            return downloadZipFile(pathString, zipFile);

        }

        [HttpGet]
        [Route("hoylatreclassfile/{startdate}/{enddate}")]
        public IActionResult getHoyLatReclassFile(String startDate, String endDate)
        {
            System.Globalization.CultureInfo provider = System.Globalization.CultureInfo.InvariantCulture;

            DateTime startDateObj = DateTime.ParseExact(startDate, "yyyy-MM-dd", provider);
            DateTime endDateObj = DateTime.ParseExact(endDate, "yyyy-MM-dd", provider);

            // Specify a name for your top-level folder.
            string folderName = @_webHostEnvironment.ContentRootPath;

            long milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            string pathString = System.IO.Path.Combine(folderName, "ZipFiles", milliseconds.ToString());

            //create folder
            System.IO.Directory.CreateDirectory(pathString);

            _logger.LogInformation("Current Path:" + pathString);

            String accountNum = "5000";
            String transactionType = "bill";
            String companyId = "california";
            List<String> customerList = new List<String>
                {
                    "LA Times - CIPS",
                    "HOY- FDS"
                };


            Dictionary<String, Dictionary<String, String>> assignMemo = new Dictionary<string, Dictionary<string, string>>();

            Dictionary<String, String> currCompanyMemo = new Dictionary<string, string>();
            currCompanyMemo.Add("ACORN", "Acorn");
            currCompanyMemo.Add("BEACH REPORTER", "Beach");
            currCompanyMemo.Add("DOWNEY PATRIOT ", "Downey");
            currCompanyMemo.Add("GAZETTE", "Gazette");
            currCompanyMemo.Add("LARCHMONT CHRONICLE", "Larch:|:Chronicle:|:Larchmont");
            currCompanyMemo.Add("NORWALK PATRIOT", "Norwalk");
            currCompanyMemo.Add("OUTLOOK", "Outlook");
            currCompanyMemo.Add("RACK & STACK", "Rack and Stack:|:RS:|:OC Catholic:|:Home:|:OCR Rack");
            currCompanyMemo.Add("OC FAMILY", "Coast:|:OCR:|:NSD:|:OC");
            currCompanyMemo.Add("EASY READER", "Easy:|:Reader");
            currCompanyMemo.Add("GREENLEAF GUARDIAN / WHITTIER", " Whittier:|:Greenleaf:|:Guardian");
            currCompanyMemo.Add("PARCELS", "Parcels");
            currCompanyMemo.Add("RIVERSIDE", "Riverside");
            currCompanyMemo.Add("SOUTH BAY DIGS", "Digs");
            currCompanyMemo.Add("SAN PEDRO TODAY", "San Pedro");
            currCompanyMemo.Add("OTHERS", "_________________________________");

            assignMemo.Add("LA Times - CIPS", currCompanyMemo);


            currCompanyMemo = new Dictionary<string, string>();

            currCompanyMemo.Add("ACORN", "Acorn");
            currCompanyMemo.Add("Home & Garden", "HG:|:Rack and Stack:|:RS:|:OC Catholic:|:Home:|:OCR Rack");
            currCompanyMemo.Add("SCNG", "Excelsior");
            currCompanyMemo.Add("HOY- FDS", ": FDS:|:FDS");
            currCompanyMemo.Add("OTHERS", "_________________________________");

            assignMemo.Add("HOY- FDS", currCompanyMemo);




            foreach (String customer in customerList)
            {
                _logger.LogInformation("Curr Customer:" + customer);

                var workbook = new XLWorkbook();
                //create sheet
                var currentCustomerWorkSheet = workbook.AddWorksheet("Data");


                //get data
                JArray transactionData = getHoyLatData(accountNum, companyId, customer, transactionType, startDateObj, endDateObj);



                //add headers
                int columnCount = 1;
                int rowCount = 1;
                String[] columnList = new String[] { "type", "account_name", "memo", "week", "balance" };
                var currentRow = currentCustomerWorkSheet.Row(rowCount++);
                foreach (String column in columnList)
                {
                    currentRow.Cell(columnCount++).SetValue(column);
                }

                double totalGlValue = 0;

                Dictionary<String, Dictionary<String, Double>> transWeekMemo = new Dictionary<string, Dictionary<string, double>>();

                Dictionary<String, Double> weeklyBalance = new Dictionary<string, double>();

                //write data to excel
                foreach (JObject transaction in transactionData)
                {
                    String week = transaction.Value<string>("week");
                    String type = transaction.Value<string>("type");
                    String memo = transaction.Value<string>("memo");
                    double balance = transaction.Value<double>("balance") * -1;

                    if (!!!transWeekMemo.ContainsKey(week))
                    {
                        transWeekMemo.Add(week, new Dictionary<string, double>());
                        transWeekMemo.GetValueOrDefault(week).Add("OTHERS", 0);
                        rowCount++;
                    }

                    if (!!!weeklyBalance.ContainsKey(week))
                    {
                        weeklyBalance.Add(week, 0);
                    }

                    weeklyBalance[week] = weeklyBalance.GetValueOrDefault(week) + balance;

                    Boolean foundMemo = false;

                    foreach (KeyValuePair<String, String> assignMemoIter in assignMemo.GetValueOrDefault(customer))
                    {
                        if (foundMemo)
                        {
                            break;
                        }

                        if (!!!transWeekMemo.GetValueOrDefault(week).ContainsKey(assignMemoIter.Key))
                        {
                            transWeekMemo.GetValueOrDefault(week).Add(assignMemoIter.Key, 0);
                        }

                        String[] currAssignMemoList = assignMemoIter.Value.Split(":|:");

                        foreach (String m in currAssignMemoList)
                        {
                            if (memo.Contains(m))
                            {
                                foundMemo = true;
                                transWeekMemo[week][assignMemoIter.Key] = balance + transWeekMemo.GetValueOrDefault(week).GetValueOrDefault(assignMemoIter.Key);
                                break;
                            }
                        }
                    }//end loop for assignMemoIter

                    if (!!!foundMemo)
                    {//if not add it to others
                        transWeekMemo[week]["OTHERS"] = transWeekMemo.GetValueOrDefault(week).GetValueOrDefault("OTHERS") + balance;
                    }



                    currentRow = currentCustomerWorkSheet.Row(rowCount++);
                    columnCount = 1;
                    foreach (String column in columnList)
                    {
                        var currCell = currentRow.Cell(columnCount++);

                        switch (column)
                        {
                            case "balance":
                                currCell.SetValue(balance);
                                break;
                            default:
                                currCell.SetValue(transaction.Value<string>(column));
                                break;
                        }
                    }
                    totalGlValue = totalGlValue + balance;

                }//end of transaction loop


                //create sheet
                currentCustomerWorkSheet = workbook.AddWorksheet("Reclass");

                rowCount = 1;
                columnCount = 2;
                currentRow = currentCustomerWorkSheet.Row(rowCount++);
                foreach (KeyValuePair<String, Double> week in weeklyBalance)
                {
                    currentRow.Cell(columnCount++).SetValue(DateTime.Parse(week.Key).ToString("yyyy-MM-dd"));
                }

                foreach (KeyValuePair<String, String> assignMemoIter in assignMemo.GetValueOrDefault(customer))
                {
                    currentRow = currentCustomerWorkSheet.Row(rowCount++);
                    columnCount = 1;
                    currentRow.Cell(columnCount).SetValue(assignMemoIter.Key);
                }
                currentRow = currentCustomerWorkSheet.Row(rowCount++);
                columnCount = 1;
                currentRow.Cell(columnCount).SetValue("Total");

                currentRow = currentCustomerWorkSheet.Row(rowCount++);
                columnCount = 1;
                currentRow.Cell(columnCount).SetValue("Weekly Balance");

                currentRow = currentCustomerWorkSheet.Row(rowCount++);
                columnCount = 1;
                currentRow.Cell(columnCount).SetValue("Matching");

                columnCount = 2;
                foreach (KeyValuePair<String, Double> week in weeklyBalance)
                {
                    double currWeekAmount = 0;
                    rowCount = 2;
                    foreach (KeyValuePair<String, String> assignMemoIter in assignMemo.GetValueOrDefault(customer))
                    {
                        double value = transWeekMemo.GetValueOrDefault(week.Key).GetValueOrDefault(assignMemoIter.Key);
                        currWeekAmount += value;
                        currentRow = currentCustomerWorkSheet.Row(rowCount++);
                        currentRow.Cell(columnCount).SetValue(value);
                    }
                    currentRow = currentCustomerWorkSheet.Row(rowCount++);
                    currentRow.Cell(columnCount).SetValue(currWeekAmount);

                    currentRow = currentCustomerWorkSheet.Row(rowCount++);
                    currentRow.Cell(columnCount).SetValue(week.Value);

                    currentRow = currentCustomerWorkSheet.Row(rowCount++);
                    currentRow.Cell(columnCount).SetValue(week.Value == currWeekAmount);

                    columnCount++;

                    if (week.Value == currWeekAmount)
                    {
                        _logger.LogInformation("Week is matching !!!");
                    }
                }

                String currFileName = customer;
                currFileName = currFileName.Replace("/", "_") + ".xlsx";
                _logger.LogInformation("Curr File Name:" + currFileName);
                //write excel file to filesystem
                workbook.SaveAs(System.IO.Path.Combine(pathString, currFileName));


            }//end of customer loop

            String zipFile = System.IO.Path.Combine(folderName, "ZipFiles", milliseconds.ToString() + ".zip");

            return downloadZipFile(pathString, zipFile);
        }


        [HttpGet]
        [Route("mdnetreallocationfile/{startdate}/{enddate}")]
        public IActionResult getMdNetReallocationFile(String startDate, String endDate)
        {
            System.Globalization.CultureInfo provider = System.Globalization.CultureInfo.InvariantCulture;


            DateTime startDateObj = DateTime.ParseExact(startDate, "yyyy-MM-dd", provider);
            DateTime endDateObj = DateTime.ParseExact(endDate, "yyyy-MM-dd", provider);

            // Specify a name for your top-level folder.
            string folderName = @_webHostEnvironment.ContentRootPath;

            long milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            string pathString = System.IO.Path.Combine(folderName, "ZipFiles", milliseconds.ToString());

            //create folder
            System.IO.Directory.CreateDirectory(pathString);

            _logger.LogInformation("Current Path:" + pathString);

            String accountNum = "6725";
            String transactionType = "bill";
            List<String> customerList = new List<String>
                {
                    "california",
                    "midwest",
                    "southeast",
                    "southwest"
                };


            Dictionary<String, Dictionary<String, String>> assignMemo = new Dictionary<string, Dictionary<string, string>>();

            Dictionary<String, String> currCompanyMemo = new Dictionary<string, string>();

            currCompanyMemo.Add("HOY", "LA Times HOY");
            currCompanyMemo.Add("CIPS Marketing", "LA Times");
            currCompanyMemo.Add("San Diego", "San Diego");
            currCompanyMemo.Add("Ventura", "Ventura");
            currCompanyMemo.Add("Victorville", "Victorville");
            currCompanyMemo.Add("Thryv", "_________________");
            assignMemo.Add("california", currCompanyMemo);


            currCompanyMemo = new Dictionary<string, string>();
            currCompanyMemo.Add("Cox", "Atlanta Buyer's Edge:|:Atlanta Evening Edge");
            currCompanyMemo.Add("Sun-Sentinel", "Sun-Sentinel");
            currCompanyMemo.Add("Palm Beach", "Palm Beach");
            assignMemo.Add("southeast", currCompanyMemo);

            currCompanyMemo = new Dictionary<string, string>();
            currCompanyMemo.Add("Dallas", "Al Dia");
            currCompanyMemo.Add("Houston", "Houston");
            currCompanyMemo.Add("San Antonio", "San Antonio");
            assignMemo.Add("southwest", currCompanyMemo);


            currCompanyMemo = new Dictionary<string, string>();
            currCompanyMemo.Add("Dayton", "Dayton");
            currCompanyMemo.Add("St. Louis", "St. Louis:|:EL01:|:FR01");
            currCompanyMemo.Add("Shaw", "Shaw");
            assignMemo.Add("midwest", currCompanyMemo);


            var workbook = new XLWorkbook();
            //create sheet
            var currentCustomerWorkSheet = workbook.AddWorksheet("Data");

            //add headers
            int columnCount = 1;
            int rowCount = 1;
            String[] columnList = new String[] { "type", "account_name", "memo", "week", "balance", "customer", "company" };
            var currentRow = currentCustomerWorkSheet.Row(rowCount++);
            foreach (String column in columnList)
            {
                currentRow.Cell(columnCount++).SetValue(column);
            }

            Dictionary<String, Dictionary<String, Double>> companyCustomerMap = new Dictionary<string, Dictionary<string, double>>();

            double totalGlValue = 0;

            //get data
            JArray transactionData = getHoyLatData(accountNum, "advertisingconsultants", "General Company OH", transactionType, startDateObj, endDateObj);

            foreach (JObject transaction in transactionData)
            {
                String week = transaction.Value<string>("week");
                String type = transaction.Value<string>("type");
                String memo = transaction.Value<string>("memo");
                double balance = transaction.Value<double>("balance") * -1;

                String selectedCustomer = "";
                String selectedCompany = "";

                Boolean foundMemo = false;

                foreach (KeyValuePair<String, Dictionary<String, String>> assignMemoIter in assignMemo)
                {

                    if (!!!companyCustomerMap.ContainsKey(assignMemoIter.Key))
                    {
                        companyCustomerMap.Add(assignMemoIter.Key, new Dictionary<string, double>());
                    }

                    foreach (KeyValuePair<String, String> customerIter in assignMemoIter.Value)
                    {
                        if (foundMemo)
                        {
                            break;
                        }

                        if (!!!companyCustomerMap.GetValueOrDefault(assignMemoIter.Key).ContainsKey(customerIter.Key))
                        {
                            companyCustomerMap.GetValueOrDefault(assignMemoIter.Key).Add(customerIter.Key, 0);
                        }

                        String[] currAssignMemoList = customerIter.Value.Split(":|:");

                        foreach (String m in currAssignMemoList)
                        {
                            if (memo.Contains(m))
                            {
                                selectedCompany = assignMemoIter.Key;
                                selectedCustomer = customerIter.Key;

                                foundMemo = true;
                                companyCustomerMap[assignMemoIter.Key][customerIter.Key] = companyCustomerMap.GetValueOrDefault(assignMemoIter.Key).GetValueOrDefault(customerIter.Key) + balance;
                                break;
                            }
                        }
                    }//end of customer iter

                }//end of assign memo iter

                if (!!!foundMemo)
                {
                    selectedCompany = "california";
                    selectedCustomer = "Thryv";

                    if (!!!companyCustomerMap.ContainsKey(selectedCompany))
                    {
                        companyCustomerMap.Add(selectedCompany, new Dictionary<string, double>());
                        if (!!!companyCustomerMap.GetValueOrDefault(selectedCompany).ContainsKey(selectedCustomer))
                        {
                            companyCustomerMap.GetValueOrDefault(selectedCompany).Add(selectedCustomer, 0);
                        }
                    }

                    companyCustomerMap[selectedCompany][selectedCustomer] = companyCustomerMap.GetValueOrDefault(selectedCompany).GetValueOrDefault(selectedCustomer) + balance;
                }


                currentRow = currentCustomerWorkSheet.Row(rowCount++);
                columnCount = 1;
                foreach (String column in columnList)
                {
                    var currCell = currentRow.Cell(columnCount++);

                    switch (column)
                    {
                        case "company":
                            currCell.SetValue(selectedCompany);
                            break;
                        case "customer":
                            currCell.SetValue(selectedCustomer);
                            break;
                        case "balance":
                            currCell.SetValue(balance);
                            break;
                        default:
                            currCell.SetValue(transaction.Value<string>(column));
                            break;
                    }
                }
                totalGlValue = totalGlValue + balance;

            }//end of transaction loop

            //create sheet
            currentCustomerWorkSheet = workbook.AddWorksheet("Reclass");

            rowCount = 1;
            columnCount = 1;
            foreach (KeyValuePair<String, Dictionary<String, Double>> companyIter in companyCustomerMap)
            {
                columnCount = 1;
                double currCompanyTotal = 0;

                currentRow = currentCustomerWorkSheet.Row(rowCount++);
                currentRow.Cell(columnCount++).SetValue(companyIter.Key);

                foreach (KeyValuePair<String, Double> customerIter in companyIter.Value)
                {
                    columnCount = 1;
                    double value = customerIter.Value;

                    currentRow = currentCustomerWorkSheet.Row(rowCount++);
                    currentRow.Cell(columnCount++).SetValue(customerIter.Key);
                    currentRow.Cell(columnCount++).SetValue<double>(value);

                    currCompanyTotal += value;
                }
                columnCount = 1;
                currentRow = currentCustomerWorkSheet.Row(rowCount++);
                currentRow.Cell(columnCount++).SetValue(companyIter.Key + "(Total)");
                columnCount = 3;
                currentRow.Cell(columnCount++).SetValue(currCompanyTotal);

                rowCount += 2;
            }




            String currFileName = "Reallocation_" + milliseconds;
            currFileName = currFileName.Replace("/", "_") + ".xlsx";
            _logger.LogInformation("Curr File Name:" + currFileName);
            //write excel file to filesystem
            workbook.SaveAs(System.IO.Path.Combine(pathString, currFileName));



            String zipFile = System.IO.Path.Combine(folderName, "ZipFiles", milliseconds.ToString() + ".zip");

            return downloadZipFile(pathString, zipFile);
        }


        /**
         * Method to create and download zip files.
         * */
        public IActionResult downloadZipFile(String folderName, String zipFileName)
        {
            ZipFile.CreateFromDirectory(folderName, zipFileName);
            Stream zipStream = new FileStream(zipFileName, FileMode.Open);

            return File(zipStream, "application/zip");
        }

        [HttpPost]
        [Route("payrollaccrualfile/{startDate}/{endDate}/{daysInWeek}/{daysInLastWeek}/{weeklyBiWeekly}")]
        public IActionResult getPayrollAccrualFile(String startDate, String endDate, int daysInWeek, int daysInLastWeek, String weeklyBiWeekly, [FromBody] System.Text.Json.JsonElement requestData)
        {
            System.Globalization.CultureInfo provider = System.Globalization.CultureInfo.InvariantCulture;

            String rawJson = requestData.ToString();
            JObject inputData = JsonConvert.DeserializeObject<JObject>(rawJson);

            // Specify a name for your top-level folder.
            string folderName = @_webHostEnvironment.ContentRootPath;

            long milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            string pathString = System.IO.Path.Combine(folderName, "ZipFiles", milliseconds.ToString());

            //create folder
            System.IO.Directory.CreateDirectory(pathString);


            _logger.LogInformation("Current Path:" + pathString);

            Dictionary<String, List<String>> regionCustomer = new Dictionary<string, List<string>>();
            List<String> customerList;
            if (inputData.Count > 0)
            {
                foreach (var company in inputData)
                {
                    customerList = new List<String>();
                    foreach (var customer in company.Value)
                    {
                        customerList.Add(customer.ToString());
                    }
                    regionCustomer.Add(company.Key, customerList);
                }
            }


            if (regionCustomer.Count == 0 && weeklyBiWeekly.Contains("weekly"))
            {
                customerList = new List<String>
                {
                    "IT/Mapping/Admin",
                    "Management OH",
                    "Accounting OH"
                };
                regionCustomer.Add("advertisingconsultants", customerList);

                customerList = new List<String>
                {
                    "ACI Last Mile CA LLC",
                    "CIPS Marketing Group, LLC"
                };
                regionCustomer.Add("california", customerList);

                customerList = new List<String>();
                customerList.Add("Palm Beach Post");
                customerList.Add("Cox-Buyers Edge");
                customerList.Add("ACI Last Mile Southeast LLC");
                regionCustomer.Add("southeast", customerList);

                customerList = new List<String>();
                customerList.Add("Dallas Morning News, Inc.");
                customerList.Add("Houston Chronicle Media Group");
                customerList.Add("San Antonio Express-News");
                regionCustomer.Add("southwest", customerList);

                customerList = new List<String>();
                customerList.Add("MDNET, LLC");
                regionCustomer.Add("mdnet", customerList);

                customerList = new List<String>();
                customerList.Add("Last Mile Network, LLC");
                regionCustomer.Add("lastmilenetwork", customerList);

                customerList = new List<String>();
                customerList.Add("ACI Last Mile Midwest LLC");
                regionCustomer.Add("midwest", customerList);
            }
            else if (regionCustomer.Count == 0 && weeklyBiWeekly.Contains("biweekly"))
            {
                customerList = new List<String>();
                customerList.Add("Last Mile Network, LLC");
                regionCustomer.Add("lastmilenetwork", customerList);

                customerList = new List<String>
                {
                    "Cox Media Group Ohio",
                    "St Louis Post-Dispatch"
                };
                regionCustomer.Add("midwest", customerList);
            }

            var consolidatedWorkBook = new XLWorkbook();

            String[] accountNums = new string[] { "8010", "8020", "8040", "8080", "8200.1", "8200.2", "8200.3", "8200.4", "8035", "8060", "8030", "8045" };

            DateTime startDateObj = DateTime.ParseExact(startDate, "yyyy-MM-dd", provider);
            DateTime endDateObj = DateTime.ParseExact(endDate, "yyyy-MM-dd", provider);

            foreach (KeyValuePair<String, List<String>> regionCustomerKeyValue in regionCustomer)
            {
                String currRegion = regionCustomerKeyValue.Key;
                customerList = regionCustomer.GetValueOrDefault(currRegion);

                _logger.LogInformation("Company:" + currRegion);

                String[] columnList4ConsolidatedSheet = new String[] { "Customer Name", "Account #", "Account Name",
                    weeklyBiWeekly, "Days in Last Week", "Days in Week", "Accrued Amount" };

                int rowCount4ConsolidatedSheet = 1;
                int columnCount4ConsolidatedSheet = 1;

                // add accrual data to consolidated sheet
                var currConsolidatedSheet = consolidatedWorkBook.AddWorksheet(currRegion.Replace("/", "_").Substring(0, currRegion.Length > 30 ? 30 : currRegion.Length));

                var currentRow = currConsolidatedSheet.Row(rowCount4ConsolidatedSheet++);

                foreach (String column in columnList4ConsolidatedSheet)
                {
                    currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue(column);
                }

                foreach (String customer in customerList)
                {
                    _logger.LogInformation("Customer:" + customer);

                    var workbook = new XLWorkbook();
                    //create sheet
                    var currentCustomerWorkSheet = workbook.AddWorksheet();


                    //get data
                    JArray payrollData = getPayrollData(accountNums, currRegion, customer, startDateObj, endDateObj);

                    String[] columnList = new String[] { "type", "account_num", "account_name", "memo", "week", "balance" };
                    currentRow = currentCustomerWorkSheet.Row(1);
                    int columnCount = 1;
                    foreach (String column in columnList)
                    {
                        currentRow.Cell(columnCount++).SetValue(column);
                    }

                    int rowCount = 1;

                    Dictionary<String, Double> accountNumValue = new Dictionary<String, Double>();

                    // temp variable to store 401k information
                    Dictionary<String, Double> _401kContribution = new Dictionary<String, Double>();

                    //write data to excel
                    foreach (JObject transaction in payrollData)
                    {
                        String accountNum = transaction.Value<string>("account_num");
                        String accountName = transaction.Value<string>("account_name");
                        String week = transaction.Value<string>("week");
                        String type = transaction.Value<string>("type");
                        String memo = transaction.Value<string>("memo");
                        double balance = transaction.Value<double>("balance") * -1;

                        String accountNumName = accountNum + ":|:" + accountName;

                        if (memo.ToLower().Contains("401k contributions"))
                        {
                            double amountToConsider = 0;
                            switch (customer)
                            {
                                case "Last Mile Network, LLC":
                                    if (weeklyBiWeekly.Contains("biweekly") && memo.ToLower().Contains("jaris"))
                                    {
                                        amountToConsider = balance;
                                    }
                                    else if (weeklyBiWeekly.Contains("weekly") && !memo.ToLower().Contains("jaris"))
                                    {
                                        amountToConsider = balance;
                                    }
                                    break;
                                default:
                                    amountToConsider = balance;
                                    break;
                            }
                            if (!!!_401kContribution.ContainsKey(accountNumName))
                            {
                                _401kContribution.Add(accountNumName, 0.0);
                            }

                            if (amountToConsider > _401kContribution.GetValueOrDefault(accountNumName))
                            {
                                _401kContribution[accountNumName] = amountToConsider;
                            }

                        }
                        else if (!!!accountNumValue.ContainsKey(accountNumName))
                        {
                            accountNumValue.Add(accountNumName, amountToConsider(customer, accountName, balance, weeklyBiWeekly, memo));
                            rowCount++;
                        }
                        else
                        {
                            accountNumValue[accountNumName] = accountNumValue.GetValueOrDefault(accountNumName) + amountToConsider(customer, accountName, balance, weeklyBiWeekly, memo);
                        }//end of _401k if

                        currentRow = currentCustomerWorkSheet.Row(++rowCount);
                        columnCount = 1;
                        foreach (String column in columnList)
                        {
                            var currCell = currentRow.Cell(columnCount++);

                            switch (column)
                            {
                                case "balance":
                                    currCell.SetValue<double>(balance);
                                    break;
                                default:
                                    currCell.SetValue(transaction.Value<string>(column));
                                    break;
                            }
                        }//end of for each data write

                    }//end of transaction loop

                    foreach (KeyValuePair<String, Double> _401kIterator in _401kContribution)
                    {
                        if (!!!accountNumValue.ContainsKey(_401kIterator.Key))
                        {
                            accountNumValue.Add(_401kIterator.Key, 0);
                        }

                        accountNumValue[_401kIterator.Key] = accountNumValue.GetValueOrDefault(_401kIterator.Key) + _401kIterator.Value;
                    }//end of _401k iterator


                    double totalAccrualAmount = 0;

                    foreach (KeyValuePair<String, Double> accountIterator in accountNumValue)
                    {
                        currentRow = currConsolidatedSheet.Row(rowCount4ConsolidatedSheet++);
                        columnCount4ConsolidatedSheet = 1;

                        String currAccountNum = accountIterator.Key.Split(":|:")[0];
                        String currAccountName = accountIterator.Key.Split(":|:")[1];

                        double accruedAmount = ((double)daysInLastWeek / (double)daysInWeek) * accountIterator.Value;
                        totalAccrualAmount += accruedAmount;

                        currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue(customer);

                        currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue(currAccountNum);
                        currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue(currAccountName);

                        currentRow.Cell(columnCount4ConsolidatedSheet++)
                                .SetValue<double>(accountIterator.Value);

                        currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue<int>(daysInLastWeek);
                        currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue<int>(daysInWeek);

                        currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue<double>(accruedAmount);

                    }//end of accountIterator loop

                    columnCount4ConsolidatedSheet = 1;
                    currentRow = currConsolidatedSheet.Row(rowCount4ConsolidatedSheet++);
                    currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue(customer);
                    currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue("2900");
                    currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue("Accrued Wages");
                    columnCount4ConsolidatedSheet = columnCount4ConsolidatedSheet + 3;
                    currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue(totalAccrualAmount);

                    rowCount4ConsolidatedSheet += 2;

                    String currFileName = customer;
                    currFileName = currFileName.Replace("/", "_") + ".xlsx";
                    _logger.LogInformation("Curr File Name:" + currFileName);
                    //write excel file to filesystem
                    workbook.SaveAs(System.IO.Path.Combine(pathString, currFileName));
                }//end of customer loop

            }//end of company loop

            String consolidatedWorkBookFileName = "consolidated.xlsx";
            consolidatedWorkBook.SaveAs(System.IO.Path.Combine(pathString, consolidatedWorkBookFileName));


            String zipFile = System.IO.Path.Combine(folderName, "ZipFiles", milliseconds.ToString() + ".zip");

            return downloadZipFile(pathString, zipFile);

        }

        public static double amountToConsider(String currCustomerName, String accountName, double balance,
        String weeklyBiWeekly, String memo)
        {
            double amountToConsider = 0;

            switch (currCustomerName)
            {
                case "IT/Mapping/Admin":
                    if (accountName.ToLower().Contains("it salaries") && memo.ToLower().Contains("russell"))
                    {
                        amountToConsider = balance * 0.75;
                    }
                    else if (accountName.ToLower().Contains("management salaries") && memo.ToLower().Contains("rob"))
                    {
                        amountToConsider = balance * 0.2;
                    }
                    else
                    {
                        amountToConsider = balance;
                    }
                    break;
                case "Last Mile Network, LLC":
                    if (weeklyBiWeekly.ToLower().Contains("biweekly") && memo.ToLower().Contains("jaris"))
                    {
                        amountToConsider = balance;
                    }
                    else if (weeklyBiWeekly.ToLower().Contains("weekly") && !memo.ToLower().Contains("jaris"))
                    {
                        amountToConsider = balance;
                    }
                    break;
                default:
                    amountToConsider = balance;
                    break;
            }

            return amountToConsider;
        }

        [HttpPost]
        [Route("weekendaccrualfile/{startdate}/{enddate}/{weekstartdate}/{weekenddate}")]
        public IActionResult getWeekEndAccrualFile(String startDate, String endDate, String weekStartDate, String weekEndDate, [FromBody] System.Text.Json.JsonElement requestData)
        {

            System.Globalization.CultureInfo provider = System.Globalization.CultureInfo.InvariantCulture;

            String rawJson = requestData.ToString();
            JObject inputData = JsonConvert.DeserializeObject<JObject>(rawJson);

            // Specify a name for your top-level folder.
            string folderName = @_webHostEnvironment.ContentRootPath;

            long milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            string pathString = System.IO.Path.Combine(folderName, "ZipFiles", milliseconds.ToString());

            //create folder
            System.IO.Directory.CreateDirectory(pathString);


            _logger.LogInformation("Current Path:" + pathString);


            Dictionary<String, Dictionary<String, String>> customerData = getACICustomerList();

            Dictionary<String, List<String>> regionCustomer = new Dictionary<string, List<string>>();

            if (inputData.Count > 0)
            {
                foreach (var company in inputData)
                {
                    List<String> customerList = new List<String>();
                    foreach (var customer in company.Value)
                    {
                        customerList.Add(customer.ToString());
                    }
                    regionCustomer.Add(company.Key, customerList);
                }
            }


            if (regionCustomer.Count == 0)
            {
                List<String> customerList = new List<String>();
                customerList.Add("Shaw Media");
                customerList.Add("St Louis Post-Dispatch");
                customerList.Add("Cox Media Group Ohio");

                regionCustomer.Add("Midwest", customerList);

                customerList = new List<String>();


                customerList.Add("Houston Chronicle Media Group");
                customerList.Add("San Antonio Express-News");
                customerList.Add("Dallas Morning News, Inc.");

                regionCustomer.Add("Southwest", customerList);

                customerList = new List<String>();

                customerList.Add("Cox-Buyers Edge");
                customerList.Add("Cox-Evening Edge");
                customerList.Add("Palm Beach Post");
                customerList.Add("Sun-Sentinel Company, LLC");

                regionCustomer.Add("Southeast", customerList);

                customerList = new List<String>();

                customerList.Add("Santa Barbara Daily Press");
                customerList.Add("Acorn Newspapers");
                customerList.Add("Beach Reporter");
                customerList.Add("Easy Reader");
                customerList.Add("Dow Jones & Company");
                customerList.Add("Gazette-Daily News");
                customerList.Add("Norwalk Patriot");
                customerList.Add("Ventura County Star (HD)");
                customerList.Add("Ventura County Star (TMC)");
                customerList.Add("UT Community Press");
                customerList.Add("HOY- FDS");
                customerList.Add("LAT/OC HD");
                customerList.Add("San Diego News - Chula Vista Star News");
                customerList.Add("Victorville TMC");
                customerList.Add("UT San Diego");
                customerList.Add("Greenleaf Guardian, LLC");
                customerList.Add("The Downey Patriot");
                customerList.Add("SDUT HD-Other");
                customerList.Add("San Diego Neighborhood News - East County");
                customerList.Add("OCR - Rack and Stack");
                customerList.Add("LA Times Santa Barbara-Barrons");
                customerList.Add("Outlook/La Canada Flintridge");
                customerList.Add("SCNG");
                customerList.Add("San Bernardino Sun");
                customerList.Add("The Press Enterprises Company- Riverside");
                customerList.Add("San Diego Union-Tribune");
                customerList.Add("Valassis");
                customerList.Add("Hector Borboa - LAT SC");
                customerList.Add("Teak Santa Barbara");
                customerList.Add("Larchmont Chronicle");
                customerList.Add("San Pedro Today");

                regionCustomer.Add("California", customerList);


            }

            Dictionary<String, Dictionary<String, List<Double>>> finalData = new Dictionary<string, Dictionary<string, List<double>>>();

            var consolidatedWorkBook = new XLWorkbook();


            foreach (KeyValuePair<String, List<String>> regionCustomerKeyValue in regionCustomer)
            {
                String currRegion = regionCustomerKeyValue.Key;
                List<String> customerList = regionCustomer.GetValueOrDefault(currRegion);

                _logger.LogInformation("Company:" + currRegion);

                foreach (String customer in customerList)
                {

                    var currCustomerConsolidatedSheet = consolidatedWorkBook.AddWorksheet(customer.Replace("/", "_").Substring(0, customer.Length > 30 ? 30 : customer.Length));

                    int rowCount4ConsolidatedSheet = 1;
                    int columnCount4ConsolidatedSheet = 1;


                    var workbook = new XLWorkbook();

                    _logger.LogInformation("Customer:" + customer);
                    String[] accountName = new string[] {"Distribution Contract Revenue",
                            "Delivery Contract Expense"};
                    foreach (String account in accountName)
                    {
                        String currAccount = account;
                        _logger.LogInformation("Account Name:" + account);


                        String sheetName = "";

                        switch (currAccount)
                        {
                            case "Distribution Contract Revenue":
                                sheetName = "Revenue";
                                break;
                            case "Delivery Contract Expense":
                                sheetName = "Expense";
                                switch (customer)
                                {
                                    case "Dow Jones & Company":
                                        currAccount = "Newspapers Purchased";
                                        break;
                                }
                                break;
                        }

                        //create sheet
                        var currentCustomerWorkSheet = workbook.AddWorksheet(sheetName);

                        //add headers
                        int columnCount = 1;
                        int rowCount = 1;
                        String[] columnList = new String[] { "type", "account", "memo", "week", "balance" };
                        var currentRow = currentCustomerWorkSheet.Row(rowCount++);
                        foreach (String column in columnList)
                        {
                            currentRow.Cell(columnCount++).SetValue(column);
                        }

                        DateTime startDateObj = DateTime.ParseExact(startDate, "yyyy-MM-dd", provider);
                        DateTime endDateObj = DateTime.ParseExact(endDate, "yyyy-MM-dd", provider);

                        //get data
                        JArray transactionData = getTransactionDataFromMirror(currAccount, currRegion, customer, startDateObj, endDateObj);


                        Dictionary<String, Dictionary<String, Double>> transactionWeekType = new Dictionary<string, Dictionary<string, double>>();

                        String[] additionalMemos = new String[] { "carrier returns", "IC Expense",
                                "reclass exp from Hoy", "LA Times CIPS exp reclass" };

                        double totalGlValue = 0;

                        //write data to excel
                        foreach (JObject transaction in transactionData)
                        {
                            String week = transaction.Value<string>("week");
                            String type = transaction.Value<string>("type");
                            String memo = transaction.Value<string>("memo");
                            double balance = transaction.Value<double>("balance");



                            if (sheetName.Contains("Expense"))
                            {
                                balance = -1 * balance;
                                foreach (String additionalMemo in additionalMemos)
                                {
                                    if (transaction.Value<string>("memo").Contains(additionalMemo))
                                    {
                                        type = memo;
                                    }
                                }
                            }

                            if (!!!transactionWeekType.ContainsKey(week))
                            {
                                transactionWeekType.Add(week, new Dictionary<string, double>());
                            }

                            if (!!!transactionWeekType.GetValueOrDefault(week).ContainsKey(type))
                            {
                                rowCount++;
                                transactionWeekType.GetValueOrDefault(week).Add(type, 0);
                            }

                            transactionWeekType[week][type] = transactionWeekType.GetValueOrDefault(week).GetValueOrDefault(type) + balance;

                            currentRow = currentCustomerWorkSheet.Row(rowCount++);
                            columnCount = 1;
                            foreach (String column in columnList)
                            {
                                var currCell = currentRow.Cell(columnCount++);

                                switch (column)
                                {
                                    case "balance":
                                        currCell.SetValue(balance);
                                        break;
                                    default:
                                        currCell.SetValue(transaction.Value<string>(column));
                                        break;
                                }
                            }
                            totalGlValue = totalGlValue + balance;
                        }//end of transactionData


                        //get selected week gl balance
                        totalGlValue = 0;

                        DateTime weekStartDateObj = DateTime.ParseExact(weekStartDate, "yyyy-MM-dd", provider);
                        DateTime weekEndDateObj = DateTime.ParseExact(weekEndDate, "yyyy-MM-dd", provider);

                        transactionData = getTransactionDataFromMirror(currAccount, currRegion, customer, weekStartDateObj, weekEndDateObj);
                        foreach (JObject transaction in transactionData)
                        {
                            double balance = transaction.Value<double>("balance");
                            if (sheetName.Contains("Expense"))
                            {
                                balance = -1 * balance;
                            }
                            totalGlValue = totalGlValue + balance;
                        }

                        //TODO:
                        // Add formula instead of raw data
                        currentRow = currentCustomerWorkSheet.Row(rowCount++);
                        currentRow.Cell(columnList.Length - 1).SetValue<double>(totalGlValue);


                        columnCount4ConsolidatedSheet = 1;

                        int rowStartIndex = rowCount4ConsolidatedSheet;


                        int totalWeeks = 4;

                        int rowEndIndex = rowCount4ConsolidatedSheet;
                        //old method ends here

                        //new method starts here
                        rowCount4ConsolidatedSheet = rowStartIndex;

                        int newMethodStartIndex = columnCount4ConsolidatedSheet + 2;

                        columnCount4ConsolidatedSheet = newMethodStartIndex;
                        currentRow = currCustomerConsolidatedSheet.Row(rowCount4ConsolidatedSheet++);
                        currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue("NEW METHOD");

                        String[] columnList4ConsolidatedSheet = new String[] { "week", "value", "day" };
                        columnCount4ConsolidatedSheet = newMethodStartIndex;
                        currentRow = currCustomerConsolidatedSheet.Row(rowCount4ConsolidatedSheet++);
                        foreach (String column in columnList4ConsolidatedSheet)
                        {
                            currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue(column);
                        }

                        // numbers of days -> value
                        Dictionary<String, Double> daysValue = new Dictionary<String, Double>();

                        double totalInvoice = 0;



                        while (endDateObj.CompareTo(startDateObj) > 0)
                        {
                            DateTime weekEnd = new DateTime(startDateObj.Ticks);
                            weekEnd = weekEnd.AddDays(6);

                            _logger.LogInformation("Week End:" + weekEnd.Date);

                            List<Double> currWeekAmount = new List<Double>();
                            List<String> notAllowedTypes = new List<String>();

                            foreach (KeyValuePair<String, Dictionary<String, Double>> transWeekIterator in transactionWeekType)
                            {
                                DateTime currWeekObj = DateTime.Parse(transWeekIterator.Key);
                                if (currWeekObj.CompareTo(startDateObj) >= 0 && currWeekObj.CompareTo(endDateObj.CompareTo(weekEnd) > 0 ? weekEnd : endDateObj) <= 0)
                                {
                                    foreach (KeyValuePair<String, Double> typeIterator in transWeekIterator.Value)
                                    {
                                        _logger.LogInformation("Curr Week:" + currWeekObj.Date + " | Curr Type:" + typeIterator.Key);

                                        double value = typeIterator.Value;

                                        if (!notAllowedTypes.Contains(typeIterator.Key) && (typeIterator.Key.Contains("invoice") || typeIterator.Key.Contains("bill") || additionalMemos.Contains(typeIterator.Key)))
                                        {
                                            notAllowedTypes.Add(typeIterator.Key);
                                            currWeekAmount.Add(value);
                                            _logger.LogInformation("Allowed...");
                                        }
                                        else if (notAllowedTypes.Contains(typeIterator.Key))
                                        {
                                            currWeekAmount.Add(value);
                                        }
                                    }
                                }
                            }

                            Boolean isCalculated = false;

                            _logger.LogInformation("Curr Week Array size is:" + currWeekAmount.Count);
                            if (currWeekAmount.Count == 0 && daysValue.Count == totalWeeks - 1)
                            {
                                int day = 0;
                                double value = 0;

                                foreach (KeyValuePair<String, Double> dayIterator in daysValue)
                                {
                                    day += int.Parse(dayIterator.Key.Split("W")[1]);
                                    value += dayIterator.Value;
                                }

                                if (day > 0)
                                {
                                    currWeekAmount.Add((value / day) * 7);
                                }
                                else
                                {
                                    currWeekAmount.Add(0);
                                }

                                isCalculated = true;

                            }

                            columnCount4ConsolidatedSheet = newMethodStartIndex;
                            currentRow = currCustomerConsolidatedSheet.Row(rowCount4ConsolidatedSheet++);
                            currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue("Week of " + startDateObj.ToString("dd,MMM") + "" + (isCalculated ? " (calculated)" : ""));
                            currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue(currWeekAmount.AsQueryable<double>().Sum());
                            currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue(7);

                            totalInvoice += currWeekAmount.AsQueryable<double>().Sum();

                            weekEnd = weekEnd.AddDays(1);
                            startDateObj = endDateObj.CompareTo(weekEnd) > 0 ? weekEnd : endDateObj;

                            daysValue.Add((daysValue.Count) + "W" + 7, currWeekAmount.AsQueryable<double>().Sum());

                        }

                        columnCount4ConsolidatedSheet = newMethodStartIndex;
                        currentRow = currCustomerConsolidatedSheet.Row(rowCount4ConsolidatedSheet++);
                        currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue("Total");
                        currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue(totalInvoice);

                        columnCount4ConsolidatedSheet = newMethodStartIndex;
                        currentRow = currCustomerConsolidatedSheet.Row(rowCount4ConsolidatedSheet++);
                        currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue(
                                "GL(Week of " + (weekStartDateObj.ToString("dd,MMM")) + ")");
                        currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue(totalGlValue);


                        double newMethodValue = daysValue.GetValueOrDefault((daysValue.Count - 1) + "W7") - totalGlValue;
                        columnCount4ConsolidatedSheet = newMethodStartIndex;
                        currentRow = currCustomerConsolidatedSheet.Row(rowCount4ConsolidatedSheet++);
                        currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue("Need to Accrue(Week of "
                                + (weekStartDateObj.ToString("dd,MMM")) + " - GL)");
                        currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue(newMethodValue);






                    }//end of accountName loop



                    String currFileName = customer;
                    currFileName = currFileName.Replace("/", "_") + ".xlsx";
                    _logger.LogInformation("Curr File Name:" + currFileName);
                    //write excel file to filesystem
                    workbook.SaveAs(System.IO.Path.Combine(pathString, currFileName));
                }//end of customer loop


            }//end of company loop

            String consolidatedWorkBookFileName = "consolidated.xlsx";
            consolidatedWorkBook.SaveAs(System.IO.Path.Combine(pathString, consolidatedWorkBookFileName));

            String zipFile = System.IO.Path.Combine(folderName, "ZipFiles", milliseconds.ToString() + ".zip");

            return downloadZipFile(pathString, zipFile);

        }


        [HttpPost]
        [Route("monthendaccrualfile/{startDate}/{endDate}/{previousMonthStartDate}/{previousMonthEndDate}/{previousMonthNumOfWeeks}")]
        public IActionResult getMonthEndAccrualFile(String startDate, String endDate, String previousMonthStartDate, String previousMonthEndDate, int previousMonthNumOfWeeks, [FromBody] System.Text.Json.JsonElement requestData)
        {
            System.Globalization.CultureInfo provider = System.Globalization.CultureInfo.InvariantCulture;

            String rawJson = requestData.ToString();
            JObject inputData = JsonConvert.DeserializeObject<JObject>(rawJson);

            int daysInMonth = 31;



            int startDay = DateTime.ParseExact(startDate, "yyyy-MM-dd", provider).Day;


            // Specify a name for your top-level folder.
            string folderName = @_webHostEnvironment.ContentRootPath;

            long milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            string pathString = System.IO.Path.Combine(folderName, "ZipFiles", milliseconds.ToString());

            //create folder
            System.IO.Directory.CreateDirectory(pathString);


            _logger.LogInformation("Current Path:" + pathString);


            Dictionary<String, Dictionary<String, String>> customerData = getACICustomerList();

            Dictionary<String, List<String>> regionCustomer = new Dictionary<string, List<string>>();

            if (inputData.Count > 0)
            {
                foreach (var company in inputData)
                {
                    List<String> customerList = new List<String>();
                    foreach (var customer in company.Value)
                    {
                        customerList.Add(customer.ToString());
                    }
                    regionCustomer.Add(company.Key, customerList);
                }
            }

            if (regionCustomer.Count == 0)
            {
                List<String> customerList = new List<String>();
                customerList.Add("Shaw Media");
                customerList.Add("St Louis Post-Dispatch");
                customerList.Add("Cox Media Group Ohio");

                regionCustomer.Add("midwest", customerList);

                customerList = new List<String>();


                customerList.Add("Houston Chronicle Media Group");
                customerList.Add("San Antonio Express-News");
                customerList.Add("Dallas Morning News, Inc.");

                regionCustomer.Add("southwest", customerList);

                customerList = new List<String>();

                customerList.Add("Cox-Buyers Edge");
                customerList.Add("Cox-Evening Edge");
                customerList.Add("Palm Beach Post");
                customerList.Add("Sun-Sentinel Company, LLC");

                regionCustomer.Add("southeast", customerList);

                customerList = new List<String>();

                customerList.Add("Santa Barbara Daily Press");
                customerList.Add("Acorn Newspapers");
                customerList.Add("Beach Reporter");
                customerList.Add("Easy Reader");
                customerList.Add("Dow Jones & Company");
                customerList.Add("Gazette-Daily News");
                customerList.Add("Norwalk Patriot");
                customerList.Add("Ventura County Star (HD)");
                customerList.Add("Ventura County Star (TMC)");
                customerList.Add("UT Community Press");
                customerList.Add("HOY- FDS");
                customerList.Add("LAT/OC HD");
                customerList.Add("San Diego News - Chula Vista Star News");
                customerList.Add("Victorville TMC");
                customerList.Add("UT San Diego");
                customerList.Add("Greenleaf Guardian, LLC");
                customerList.Add("The Downey Patriot");
                customerList.Add("SDUT HD-Other");
                customerList.Add("San Diego Neighborhood News - East County");
                customerList.Add("OCR - Rack and Stack");
                customerList.Add("LA Times Santa Barbara-Barrons");
                customerList.Add("Outlook/La Canada Flintridge");
                customerList.Add("SCNG");
                customerList.Add("San Bernardino Sun");
                customerList.Add("The Press Enterprises Company- Riverside");
                customerList.Add("San Diego Union-Tribune");

                regionCustomer.Add("california", customerList);


            }

            Dictionary<String, Dictionary<String, List<Double>>> finalData = new Dictionary<string, Dictionary<string, List<double>>>();

            var consolidatedWorkBook = new XLWorkbook();


            foreach (KeyValuePair<String, List<String>> regionCustomerKeyValue in regionCustomer)
            {
                String currRegion = regionCustomerKeyValue.Key;
                List<String> customerList = regionCustomer.GetValueOrDefault(currRegion);

                _logger.LogInformation("Company:" + currRegion);

                foreach (String customer in customerList)
                {

                    var currCustomerConsolidatedSheet = consolidatedWorkBook.AddWorksheet(customer.Replace("/", "_").Substring(0, customer.Length > 30 ? 30 : customer.Length));

                    int rowCount4ConsolidatedSheet = 1;
                    int columnCount4ConsolidatedSheet = 1;


                    var workbook = new XLWorkbook();

                    _logger.LogInformation("Customer:" + customer);
                    String[] accountName = new string[] {"Distribution Contract Revenue",
                            "Delivery Contract Expense"};
                    foreach (String account in accountName)
                    {
                        String currAccount = account;
                        _logger.LogInformation("Account Name:" + account);


                        String sheetName = "";

                        switch (currAccount)
                        {
                            case "Distribution Contract Revenue":
                                sheetName = "Revenue";
                                break;
                            case "Delivery Contract Expense":
                                sheetName = "Expense";
                                switch (customer)
                                {
                                    case "Dow Jones & Company":
                                        currAccount = "Newspapers Purchased";
                                        break;
                                }
                                break;
                        }

                        //create sheet
                        var currentCustomerWorkSheet = workbook.AddWorksheet(sheetName);

                        //add headers
                        int columnCount = 1;
                        int rowCount = 1;
                        String[] columnList = new String[] { "type", "account", "memo", "week", "balance" };
                        var currentRow = currentCustomerWorkSheet.Row(rowCount++);
                        foreach (String column in columnList)
                        {
                            currentRow.Cell(columnCount++).SetValue(column);
                        }

                        //get data
                        JArray transactionData = getTransactionData(currAccount, customer, DateTime.ParseExact(startDate, "yyyy-MM-dd", provider).Year, DateTime.ParseExact(startDate, "yyyy-MM-dd", provider).Month, currRegion);

                        Dictionary<String, Dictionary<String, Double>> transactionWeekType = new Dictionary<string, Dictionary<string, double>>();

                        String[] additionalMemos = new String[] { "carrier returns", "IC Expense",
                                "reclass exp from Hoy", "LA Times CIPS exp reclass" };

                        double totalGlValue = 0;

                        //write data to excel
                        foreach (JObject transaction in transactionData)
                        {
                            String week = transaction.Value<string>("week");
                            String type = transaction.Value<string>("type");
                            String memo = transaction.Value<string>("memo");
                            double balance = transaction.Value<double>("balance");



                            if (sheetName.Contains("Expense"))
                            {
                                balance = -1 * balance;
                                foreach (String additionalMemo in additionalMemos)
                                {
                                    if (transaction.Value<string>("memo").Contains(additionalMemo))
                                    {
                                        type = memo;
                                    }
                                }
                            }

                            if (!!!transactionWeekType.ContainsKey(week))
                            {
                                transactionWeekType.Add(week, new Dictionary<string, double>());
                            }

                            if (!!!transactionWeekType.GetValueOrDefault(week).ContainsKey(type))
                            {
                                rowCount++;
                                transactionWeekType.GetValueOrDefault(week).Add(type, 0);
                            }

                            transactionWeekType[week][type] = transactionWeekType.GetValueOrDefault(week).GetValueOrDefault(type) + balance;

                            currentRow = currentCustomerWorkSheet.Row(rowCount++);
                            columnCount = 1;
                            foreach (String column in columnList)
                            {
                                var currCell = currentRow.Cell(columnCount++);

                                switch (column)
                                {
                                    case "balance":
                                        currCell.SetValue(balance);
                                        break;
                                    default:
                                        currCell.SetValue(transaction.Value<string>(column));
                                        break;
                                }
                            }
                            totalGlValue = totalGlValue + balance;
                        }//end of transactionData

                        //TODO:
                        // Add formula instead of raw data
                        currentRow = currentCustomerWorkSheet.Row(rowCount++);
                        currentRow.Cell(columnList.Length - 1).SetValue<double>(totalGlValue);


                        double lastMonthDailyAvg = getLastMonthDailyAvg(workbook, sheetName, currAccount, customer, previousMonthStartDate, previousMonthEndDate, previousMonthNumOfWeeks, currRegion);

                        _logger.LogInformation("Last Month Daily Avg:" + lastMonthDailyAvg);

                        columnCount4ConsolidatedSheet = 1;
                        currentRow = currCustomerConsolidatedSheet.Row(rowCount4ConsolidatedSheet++);
                        currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue("Last Month Daily Avg");
                        currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue<double>(lastMonthDailyAvg);


                        int rowStartIndex = rowCount4ConsolidatedSheet;


                        //old method start here

                        columnCount4ConsolidatedSheet = 1;
                        currentRow = currCustomerConsolidatedSheet.Row(rowCount4ConsolidatedSheet++);
                        currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue("OLD METHOD");

                        int totalWeeks = 4;

                        List<Double> invoiceList = getAccrualBasedOnOldMethod(currAccount, sheetName, customer, startDate, endDate, totalWeeks, currRegion);

                        columnCount4ConsolidatedSheet = 1;
                        currentRow = currCustomerConsolidatedSheet.Row(rowCount4ConsolidatedSheet++);
                        currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue("Week #");
                        currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue("Value");


                        double sumOfInvoice = 0;
                        int tempWeekNo = 1;

                        foreach (double invoice in invoiceList)
                        {
                            columnCount4ConsolidatedSheet = 1;
                            currentRow = currCustomerConsolidatedSheet.Row(rowCount4ConsolidatedSheet++);
                            currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue("Week " + (tempWeekNo++));
                            currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue<double>(invoice);
                            sumOfInvoice += invoice;
                        }

                        while (totalWeeks > invoiceList.Count)
                        {
                            double avgNumber = invoiceList.AsQueryable<double>().Sum() / invoiceList.Count;
                            columnCount4ConsolidatedSheet = 1;
                            currentRow = currCustomerConsolidatedSheet.Row(rowCount4ConsolidatedSheet++);
                            currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue("Week " + (tempWeekNo++) + " (calculated)");
                            currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue<double>(avgNumber);

                            sumOfInvoice += avgNumber;
                            invoiceList.Add(avgNumber);
                        }

                        columnCount4ConsolidatedSheet = 1;
                        currentRow = currCustomerConsolidatedSheet.Row(rowCount4ConsolidatedSheet++);
                        currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue("Total");
                        currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue<double>(sumOfInvoice);

                        columnCount4ConsolidatedSheet = 1;
                        currentRow = currCustomerConsolidatedSheet.Row(rowCount4ConsolidatedSheet++);
                        currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue("Avg Weekly");
                        currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue<double>(sumOfInvoice / invoiceList.Count);

                        columnCount4ConsolidatedSheet = 1;
                        currentRow = currCustomerConsolidatedSheet.Row(rowCount4ConsolidatedSheet++);
                        currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue("Daily Avg");
                        currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue<double>((sumOfInvoice / invoiceList.Count) / 7);

                        columnCount4ConsolidatedSheet = 1;
                        currentRow = currCustomerConsolidatedSheet.Row(rowCount4ConsolidatedSheet++);
                        currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue("Monthly Revenue");
                        currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue<double>(((sumOfInvoice / invoiceList.Count) / 7) * daysInMonth);

                        columnCount4ConsolidatedSheet = 1;
                        currentRow = currCustomerConsolidatedSheet.Row(rowCount4ConsolidatedSheet++);
                        currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue("GL");
                        currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue<double>(totalGlValue);


                        double oldMethodValue = (((sumOfInvoice / invoiceList.Count) / 7) * daysInMonth) - totalGlValue;


                        columnCount4ConsolidatedSheet = 1;
                        currentRow = currCustomerConsolidatedSheet.Row(rowCount4ConsolidatedSheet++);
                        currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue("Need to Accrue");
                        currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue<double>(oldMethodValue);

                        int rowEndIndex = rowCount4ConsolidatedSheet;
                        //old method ends here

                        //new method starts here
                        rowCount4ConsolidatedSheet = rowStartIndex;

                        int newMethodStartIndex = columnCount4ConsolidatedSheet + 2;

                        columnCount4ConsolidatedSheet = 1;
                        currentRow = currCustomerConsolidatedSheet.Row(rowCount4ConsolidatedSheet++);
                        currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue("NEW METHOD");

                        String[] columnList4ConsolidatedSheet = new String[] { "week", "value", "day" };
                        columnCount4ConsolidatedSheet = newMethodStartIndex;
                        currentRow = currCustomerConsolidatedSheet.Row(rowCount4ConsolidatedSheet++);
                        foreach (String column in columnList4ConsolidatedSheet)
                        {
                            currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue(column);
                        }

                        // numbers of days -> value
                        Dictionary<String, Double> daysValue = new Dictionary<String, Double>();

                        double totalInvoice = 0;


                        if (startDay > 1)
                        {
                            totalInvoice = lastMonthDailyAvg * (startDay - 1);

                            columnCount4ConsolidatedSheet = newMethodStartIndex;
                            currentRow = currCustomerConsolidatedSheet.Row(rowCount4ConsolidatedSheet++);
                            currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue("01-0" + (startDay - 1) + " (calculated)");
                            currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue(totalInvoice);
                            currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue(startDay - 1);


                            daysValue.Add((daysValue.Count) + "W" + (startDay - 1), totalInvoice);
                        }

                        DateTime startDateObj = DateTime.ParseExact(startDate, "yyyy-MM-dd", provider);
                        DateTime endDateObj = DateTime.ParseExact(endDate, "yyyy-MM-dd", provider);

                        while (endDateObj.CompareTo(startDateObj) > 0)
                        {
                            DateTime weekEnd = new DateTime(startDateObj.Ticks);
                            weekEnd = weekEnd.AddDays(6);

                            _logger.LogInformation("Week End:" + weekEnd.Date);

                            List<Double> currWeekAmount = new List<Double>();
                            List<String> notAllowedTypes = new List<String>();

                            foreach (KeyValuePair<String, Dictionary<String, Double>> transWeekIterator in transactionWeekType)
                            {
                                DateTime currWeekObj = DateTime.Parse(transWeekIterator.Key);
                                if (currWeekObj.CompareTo(startDateObj) >= 0 && currWeekObj.CompareTo(endDateObj.CompareTo(weekEnd) > 0 ? weekEnd : endDateObj) <= 0)
                                {
                                    foreach (KeyValuePair<String, Double> typeIterator in transWeekIterator.Value)
                                    {
                                        _logger.LogInformation("Curr Week:" + currWeekObj.Date + " | Curr Type:" + typeIterator.Key);

                                        double value = typeIterator.Value;

                                        if (!notAllowedTypes.Contains(typeIterator.Key) && (typeIterator.Key.Contains("invoice") || typeIterator.Key.Contains("bill") || additionalMemos.Contains(typeIterator.Key)))
                                        {
                                            notAllowedTypes.Add(typeIterator.Key);
                                            currWeekAmount.Add(value);
                                            _logger.LogInformation("Allowed...");
                                        }
                                        else if (notAllowedTypes.Contains(typeIterator.Key))
                                        {
                                            currWeekAmount[notAllowedTypes.IndexOf(typeIterator.Key)] = value;
                                        }
                                    }
                                }
                            }

                            Boolean isCalculated = false;

                            _logger.LogInformation("Curr Week Array size is:" + currWeekAmount.Count);
                            if (currWeekAmount.Count == 0)
                            {
                                int day = 0;
                                double value = 0;

                                foreach (KeyValuePair<String, Double> dayIterator in daysValue)
                                {
                                    day += int.Parse(dayIterator.Key.Split("W")[1]);
                                    value += dayIterator.Value;
                                }

                                if (day > 0)
                                {
                                    currWeekAmount.Add((value / day) * 7);
                                }
                                else
                                {
                                    currWeekAmount.Add(0);
                                }

                                isCalculated = true;

                            }

                            columnCount4ConsolidatedSheet = newMethodStartIndex;
                            currentRow = currCustomerConsolidatedSheet.Row(rowCount4ConsolidatedSheet++);
                            currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue("Week of " + startDateObj.ToString("dd,MMM") + "" + (isCalculated ? " (calculated)" : ""));
                            currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue(currWeekAmount.AsQueryable<double>().Sum());
                            currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue(7);

                            totalInvoice += currWeekAmount.AsQueryable<double>().Sum();

                            weekEnd = weekEnd.AddDays(1);
                            startDateObj = endDateObj.CompareTo(weekEnd) > 0 ? weekEnd : endDateObj;

                            daysValue.Add((daysValue.Count) + "W" + 7, currWeekAmount.AsQueryable<double>().Sum());

                        }

                        int additionalDays = daysInMonth - DateTime.ParseExact(endDate, "yyyy-MM-dd", provider).Day;
                        if (additionalDays > 0)
                        {
                            _logger.LogInformation("Additional Days:" + additionalDays);

                            int day = 0;
                            double value = 0;

                            foreach (KeyValuePair<String, Double> dayIterator in daysValue)
                            {
                                day += int.Parse(dayIterator.Key.Split("W")[1]);
                                value += dayIterator.Value;
                            }

                            double avgValue = 0;
                            if (day > 0)
                            {
                                avgValue = (value / day) * additionalDays;
                            }


                            columnCount4ConsolidatedSheet = newMethodStartIndex;

                            currentRow = currCustomerConsolidatedSheet.Row(rowCount4ConsolidatedSheet++);
                            currentRow.Cell(columnCount4ConsolidatedSheet++)
                                    .SetValue(DateTime.ParseExact(endDate, "yyyy-MM-dd", provider).Day + "-"
                                            + daysInMonth + " (calculated)");
                            currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue(avgValue);
                            currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue(additionalDays);

                            totalInvoice += avgValue;

                        }

                        columnCount4ConsolidatedSheet = newMethodStartIndex;
                        currentRow = currCustomerConsolidatedSheet.Row(rowCount4ConsolidatedSheet++);
                        currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue("Total");
                        currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue(totalInvoice);

                        columnCount4ConsolidatedSheet = newMethodStartIndex;
                        currentRow = currCustomerConsolidatedSheet.Row(rowCount4ConsolidatedSheet++);
                        currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue("GL");
                        currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue(totalGlValue);

                        double newMethodValue = totalInvoice - totalGlValue;
                        columnCount4ConsolidatedSheet = newMethodStartIndex;
                        currentRow = currCustomerConsolidatedSheet.Row(rowCount4ConsolidatedSheet++);
                        currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue("Need to Accrue");
                        currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue(newMethodValue);

                        rowCount4ConsolidatedSheet = rowEndIndex;
                        columnCount4ConsolidatedSheet = 1;
                        currentRow = currCustomerConsolidatedSheet.Row(rowCount4ConsolidatedSheet++);
                        currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue("Diff(OLD-NEW)");
                        currentRow.Cell(columnCount4ConsolidatedSheet++).SetValue(oldMethodValue - newMethodValue);






                    }//end of accountName loop



                    String currFileName = customer;
                    currFileName = currFileName.Replace("/", "_") + ".xlsx";
                    _logger.LogInformation("Curr File Name:" + currFileName);
                    //write excel file to filesystem
                    workbook.SaveAs(System.IO.Path.Combine(pathString, currFileName));
                }//end of customer loop


            }//end of company loop

            String consolidatedWorkBookFileName = "consolidated.xlsx";
            consolidatedWorkBook.SaveAs(System.IO.Path.Combine(pathString, consolidatedWorkBookFileName));

            String zipFile = System.IO.Path.Combine(folderName, "ZipFiles", milliseconds.ToString() + ".zip");

            return downloadZipFile(pathString, zipFile);

        }


        public List<Double> getAccrualBasedOnOldMethod(String currAccount, String sheetName, String customer, String startDate, String endDate, int numOfWeeks, String currRegion)
        {
            _logger.LogInformation("Entering getAccrualBasedOnOldMethod...");
            System.Globalization.CultureInfo provider = System.Globalization.CultureInfo.InvariantCulture;



            List<Double> invoiceList;

            JArray transactionData = getTransactionData(currAccount, customer, DateTime.ParseExact(startDate, "yyyy-MM-dd", provider).Year, DateTime.ParseExact(endDate, "yyyy-MM-dd", provider).Month, currRegion);


            double totalGlValue = 0;

            Dictionary<String, Dictionary<String, Double>> transactionWeekType = new Dictionary<string, Dictionary<string, double>>();

            String[] additionalMemos = new String[] { "carrier returns", "IC Expense",
                                "reclass exp from Hoy", "LA Times CIPS exp reclass" };



            //write data to excel
            foreach (JObject transaction in transactionData)
            {
                String week = transaction.Value<string>("week");
                String type = transaction.Value<string>("type");
                String memo = transaction.Value<string>("memo");
                double balance = transaction.Value<double>("balance");



                if (sheetName.Contains("Expense"))
                {
                    balance = -1 * balance;
                    foreach (String additionalMemo in additionalMemos)
                    {
                        if (transaction.Value<string>("memo").Contains(additionalMemo))
                        {
                            type = "bill";
                        }
                    }
                }

                if (!!!transactionWeekType.ContainsKey(type))
                {
                    transactionWeekType.Add(type, new Dictionary<string, double>());
                }

                if (!!!transactionWeekType.GetValueOrDefault(type).ContainsKey(week))
                {
                    transactionWeekType.GetValueOrDefault(type).Add(week, 0);
                }

                transactionWeekType[type][week] = transactionWeekType.GetValueOrDefault(type).GetValueOrDefault(week) + balance;

                totalGlValue = totalGlValue + balance;
            }//end of transactionData



            invoiceList = new List<double>();

            foreach (KeyValuePair<String, Dictionary<String, Double>> transTypeIterator in transactionWeekType)
            {
                if (transTypeIterator.Key.ToLower().Contains("invoice") || transTypeIterator.Key.ToLower().Contains("bill"))
                {
                    foreach (KeyValuePair<String, Double> weekIterator in transTypeIterator.Value)
                    {
                        invoiceList.Add(weekIterator.Value);
                    }
                }
            }

            if (invoiceList.Count > numOfWeeks)
            {
                invoiceList = invoiceList.GetRange(invoiceList.Count - numOfWeeks, numOfWeeks);
            }



            _logger.LogInformation("Exiting getAccrualBasedOnOldMethod\n...");
            return invoiceList;
        }



        public double getLastMonthDailyAvg(XLWorkbook currWorkBook, String sheetName, String currAccount, String customer, String previousMonthStartDate, String previousMonthEndDate, int previousMonthNumOfWeeks, String currRegion)
        {
            _logger.LogInformation("Entering getLastMonthDailyAvg...");
            System.Globalization.CultureInfo provider = System.Globalization.CultureInfo.InvariantCulture;



            double lastMonthDailyAvg = 0;

            JArray transactionData = getTransactionData(currAccount, customer, DateTime.ParseExact(previousMonthStartDate, "yyyy-MM-dd", provider).Year, DateTime.ParseExact(previousMonthEndDate, "yyyy-MM-dd", provider).Month, currRegion);
            //create sheet
            var currentCustomerWorkSheet = currWorkBook.AddWorksheet(sheetName + "_LastMonth");


            //add headers
            int columnCount = 1;
            int rowCount = 1;
            String[] columnList = new String[] { "type", "account", "memo", "week", "balance" };
            var currentRow = currentCustomerWorkSheet.Row(rowCount++);
            foreach (String column in columnList)
            {
                currentRow.Cell(columnCount++).SetValue(column);
            }


            double totalGlValue = 0;

            Dictionary<String, Dictionary<String, Double>> transactionWeekType = new Dictionary<string, Dictionary<string, double>>();

            String[] additionalMemos = new String[] { "carrier returns", "IC Expense",
                                "reclass exp from Hoy", "LA Times CIPS exp reclass" };



            //write data to excel
            foreach (JObject transaction in transactionData)
            {
                String week = transaction.Value<string>("week");
                String type = transaction.Value<string>("type");
                String memo = transaction.Value<string>("memo");
                double balance = transaction.Value<double>("balance");



                if (sheetName.Contains("Expense"))
                {
                    balance = -1 * balance;
                    foreach (String additionalMemo in additionalMemos)
                    {
                        if (transaction.Value<string>("memo").Contains(additionalMemo))
                        {
                            type = "bill";
                        }
                    }
                }

                if (!!!transactionWeekType.ContainsKey(type))
                {
                    transactionWeekType.Add(type, new Dictionary<string, double>());
                }

                if (!!!transactionWeekType.GetValueOrDefault(type).ContainsKey(week))
                {
                    rowCount++;
                    transactionWeekType.GetValueOrDefault(type).Add(week, 0);
                }

                transactionWeekType[type][week] = transactionWeekType.GetValueOrDefault(type).GetValueOrDefault(week) + balance;

                currentRow = currentCustomerWorkSheet.Row(rowCount++);
                columnCount = 1;
                foreach (String column in columnList)
                {
                    var currCell = currentRow.Cell(columnCount++);

                    switch (column)
                    {
                        case "balance":
                            currCell.SetValue(balance);
                            break;
                        default:
                            currCell.SetValue(transaction.Value<string>(column));
                            break;
                    }
                }
                totalGlValue = totalGlValue + balance;
            }//end of transactionData



            List<Double> invoiceList = new List<double>();

            foreach (KeyValuePair<String, Dictionary<String, Double>> transTypeIterator in transactionWeekType)
            {
                if (transTypeIterator.Key.ToLower().Contains("invoice") || transTypeIterator.Key.ToLower().Contains("bill"))
                {
                    foreach (KeyValuePair<String, Double> weekIterator in transTypeIterator.Value)
                    {
                        invoiceList.Add(weekIterator.Value);
                    }
                }
            }

            if (invoiceList.Count > previousMonthNumOfWeeks)
            {
                invoiceList = invoiceList.GetRange(invoiceList.Count - previousMonthNumOfWeeks, previousMonthNumOfWeeks);
            }
            double totalAmount = invoiceList.AsQueryable<double>().Sum();

            _logger.LogInformation("Total Amount:" + totalAmount);

            lastMonthDailyAvg = (totalAmount / previousMonthNumOfWeeks) / 7;

            _logger.LogInformation("Exiting getLastMonthDailyAvg\n...");
            return lastMonthDailyAvg;
        }

        public JArray getTransactionDataFromMirror(String accountName, String companyName, String customerName, DateTime startDate, DateTime endDate)
        {
            JArray transactionData = new JArray() as dynamic;

            using (aci_databaseContext db = new aci_databaseContext())
            {
                var data = (from transaction in db.Acipublisher
                            where transaction.AcitransactionDate >= startDate && transaction.AcitransactionDate <= endDate
                            join item in db.AciitemMaster
                            on transaction.AcilineItemId equals item.AcilineItemId
                            where item.AcilineItemName == accountName
                            join customer in db.AcicustomerMaster
                            on transaction.AcicustomerId equals customer.AcicustomerId
                            where customer.AcicustomerName == customerName
                            join company in db.AcicompanyMaster
                            on transaction.AcicompanyId equals company.AcicompanyId
                            where company.AcicompanyName == companyName
                            orderby transaction.AcitransactionDate
                            select new
                            {
                                Type = transaction.TransactionType,
                                Memo = transaction.Memo,
                                TransactionDate = transaction.AcitransactionDate,
                                Amount = transaction.Aciamount
                            });
                foreach (var d in data)
                {
                    dynamic currRecord = new JObject();
                    currRecord.type = d.Type;
                    currRecord.memo = d.Memo;
                    currRecord.week = d.TransactionDate;
                    currRecord.balance = d.Amount;
                    currRecord.account = accountName;

                    transactionData.Add(currRecord);
                }
            }

            using (aci_databaseContext db = new aci_databaseContext())
            {
                var data = (from transaction in db.AcimonthlyExpense
                            where transaction.AcitransactionDate >= startDate && transaction.AcitransactionDate <= endDate
                            join item in db.AciitemMaster
                            on transaction.AcilineItemId equals item.AcilineItemId
                            where item.AcilineItemName == accountName
                            join customer in db.AcicustomerMaster
                            on transaction.AcicustomerId equals customer.AcicustomerId
                            where customer.AcicustomerName == customerName
                            join company in db.AcicompanyMaster
                            on transaction.AcicompanyId equals company.AcicompanyId
                            where company.AcicompanyName == companyName
                            orderby transaction.AcitransactionDate
                            select new
                            {
                                Type = transaction.TransactionType,
                                Memo = transaction.Memo,
                                TransactionDate = transaction.AcitransactionDate,
                                Amount = transaction.Aciamount
                            });
                foreach (var d in data)
                {
                    dynamic currRecord = new JObject();
                    currRecord.type = d.Type;
                    currRecord.memo = d.Memo;
                    currRecord.week = d.TransactionDate;
                    currRecord.balance = d.Amount;
                    currRecord.account = accountName;

                    transactionData.Add(currRecord);
                }
            }

            return transactionData;
        }

        public JArray getPayrollData(String[] accountNums, String companyId, String customerName, DateTime startDate, DateTime endDate)
        {
            JArray transactionData = new JArray() as dynamic;

            using (aci_databaseContext db = new aci_databaseContext())
            {
                var data = (from transaction in db.Transaction
                            where transaction.TransactionDate >= startDate && transaction.TransactionDate <= endDate && transaction.RegionId == companyId
                            join account in db.Account
                            on transaction.AccountId equals account.AccountId
                            where accountNums.Contains(account.AccountNum)
                            join customer in db.Customer
                            on transaction.CustomerId equals customer.CustomerId
                            where customer.CustomerName == customerName
                            orderby transaction.TransactionDate
                            select new
                            {
                                AccountName = account.AccountName,
                                AccountNum = account.AccountNum,
                                Type = transaction.Type,
                                Memo = transaction.Memo,
                                TransactionDate = transaction.TransactionDate,
                                Amount = transaction.Amount
                            }
                          );

                foreach (var d in data)
                {
                    dynamic currRecord = new JObject();
                    currRecord.type = d.Type;
                    currRecord.memo = d.Memo;
                    currRecord.week = d.TransactionDate;
                    currRecord.balance = d.Amount;
                    currRecord.account_name = d.AccountName;
                    currRecord.account_num = d.AccountNum;

                    transactionData.Add(currRecord);
                }
            }

            return transactionData;
        }


        public JArray getHoyLatData(String accountNum, String companyId, String customerName, String transactionType, DateTime startDate, DateTime endDate)
        {
            JArray transactionData = new JArray() as dynamic;

            using (aci_databaseContext db = new aci_databaseContext())
            {
                var data = (from transaction in db.Transaction
                            where transaction.TransactionDate >= startDate && transaction.TransactionDate <= endDate && transaction.RegionId == companyId && transaction.Type == transactionType
                            join account in db.Account
                            on transaction.AccountId equals account.AccountId
                            where accountNum == account.AccountNum
                            join customer in db.Customer
                            on transaction.CustomerId equals customer.CustomerId
                            where customer.CustomerName == customerName
                            orderby transaction.TransactionDate, transaction.Memo
                            select new
                            {
                                AccountName = account.AccountName,
                                AccountNum = account.AccountNum,
                                Type = transaction.Type,
                                Memo = transaction.Memo,
                                TransactionDate = transaction.TransactionDate,
                                Amount = transaction.Amount
                            }
                          );

                foreach (var d in data)
                {
                    dynamic currRecord = new JObject();
                    currRecord.type = d.Type;
                    currRecord.memo = d.Memo;
                    currRecord.week = d.TransactionDate;
                    currRecord.balance = d.Amount;
                    currRecord.account_name = d.AccountName;
                    currRecord.account_num = d.AccountNum;

                    transactionData.Add(currRecord);
                }
            }

            return transactionData;
        }

        public JArray getTransactionData(String accountName, String customerName, int year, int month, String companyId)
        {
            JArray transactionData = new JArray() as dynamic;

            using (aci_databaseContext db = new aci_databaseContext())
            {
                var data = (from transaction in db.Transaction
                            where transaction.TransactionDate.Month == month && transaction.TransactionDate.Year == year
                            join account in db.Account
                            on transaction.AccountId equals account.AccountId
                            where account.AccountName == accountName
                            join customer in db.Customer
                            on transaction.CustomerId equals customer.CustomerId
                            where customer.CustomerName == customerName
                            orderby transaction.TransactionDate
                            select new
                            {
                                Type = transaction.Type,
                                Memo = transaction.Memo,
                                TransactionDate = transaction.TransactionDate,
                                Amount = transaction.Amount
                            }
                          );

                foreach (var d in data)
                {
                    dynamic currRecord = new JObject();
                    currRecord.type = d.Type;
                    currRecord.memo = d.Memo;
                    currRecord.week = d.TransactionDate;
                    currRecord.balance = d.Amount;
                    currRecord.account = accountName;

                    transactionData.Add(currRecord);
                }
            }

            return transactionData;
        }

        [HttpGet]
        [Route("get")]
        public String get()
        {
            String retVal = "";
            try
            {
                using (aci_databaseContext db = new aci_databaseContext())
                {
                    var p = (from t in db.Transaction
                             join a in db.Account
                             on t.AccountId equals a.AccountId
                             where a.AccountName == "American Express - Credit Card"
                             select new
                             {
                                 TransactionDate = t.TransactionDate,
                                 Memo = t.Memo,
                                 Amount = t.Amount
                             }
                             ).ToList();
                    foreach (var x in p)
                    {
                        Console.WriteLine("{0} {1} {2}", x.TransactionDate, x.Memo, x.Amount);
                    }
                }


            }
            catch (Exception e)
            {
                Console.WriteLine(e.GetType().ToString());
            }
            return retVal;
        }

    }

}
