using certainty.Injections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using certainty.Pages.models;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;

namespace certainty.Pages
{

    [Authorize]
    public class RecordsModel : PageModel
    {
        private readonly ISqlClient _sqlClient;
        private readonly IOnPostVerification _onPostVerification;

        public RecordsModel(ISqlClient sqlClient, IOnPostVerification onPostVerification)
        {
            _sqlClient = sqlClient;
            _onPostVerification = onPostVerification;
        }

        [BindProperty]
        public Record record { get; set; }

        [BindProperty]
        public Sort sort { get; set; }

        public List<Record> records { get; set; }
        public int delete {  get; set; }
        public int edit { get; set; }

        public List<string> categories { get; set; }

        public string message {  get; set; }

        public string currency {  get; set; }

        public void OnGet()
        {
            categories = _sqlClient.getCategories(User.Identity.Name);
            records = _sqlClient.getRecords(User.Identity.Name);
            currency = _sqlClient.ogCurrency(User.Identity.Name);
        }

        public IActionResult OnPostExportToExcel()
        {
            categories = _sqlClient.getCategories(User.Identity.Name);
            records = _sqlClient.getRecords(User.Identity.Name);
            currency = _sqlClient.ogCurrency(User.Identity.Name);

            bool continueCode = _onPostVerification.checkTries(HttpContext);

            if (continueCode == false)
            {
                return Page();
            }

            List<Record> recordsData = _sqlClient.getRecords(User.Identity.Name);
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Records");

                for (int i = 1; i<= 3; i++)
                {
                    var headerCellStyle = worksheet.Cell(1, i).Style;
                    headerCellStyle.Font.Bold = true;
                    headerCellStyle.Fill.BackgroundColor = XLColor.LightGray;
                }
                

                worksheet.Cell(1, 1).Value = "Category";
                worksheet.Cell(1, 2).Value = "Expense [" + _sqlClient.ogCurrency(User.Identity.Name) + "]";
                worksheet.Cell(1, 3).Value = "Date";

                worksheet.Column(2).AdjustToContents();


                int row = 2;
                foreach (var record in records)
                {
                    worksheet.Cell(row, 1).Value = record.category;
                    worksheet.Cell(row, 2).Value = record.value;
                    worksheet.Cell(row, 3).Value = record.recordDate.Date.ToString("d");
                    row++;
                }

                var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "records.xlsx");
            }


        }

        public IActionResult OnPostDeleteAll()
        {
            return RedirectToPage("deleteAllVerification");
        }
        public IActionResult OnPostCreation()
        {

            bool continueCode = _onPostVerification.checkTries(HttpContext);

            if (continueCode == false)
            {
                categories = _sqlClient.getCategories(User.Identity.Name);
                records = _sqlClient.getRecords(User.Identity.Name);
                currency = _sqlClient.ogCurrency(User.Identity.Name);
                return Page();
            }

            record.userID = User.Identity.Name;
            bool recordAdded = false;
            if(record.value <= 0)
            {
                message = "Value has to be greater then 0 and for decimal use ',' not '.' !";
            }

            else
            {
                recordAdded = _sqlClient.createRecord(record.category, record.value, record.userID, record.recordDate);
       
            }

            if (recordAdded)
            {
                message = "Record was created successfully!";
            }

            categories = _sqlClient.getCategories(User.Identity.Name);
            records = _sqlClient.getRecords(User.Identity.Name);
            currency = _sqlClient.ogCurrency(User.Identity.Name);

            return Page();

        }

        public IActionResult OnPostSort()
        {
            categories = _sqlClient.getCategories(User.Identity.Name);
            records = _sqlClient.getRecordsSort(User.Identity.Name, sort.sort, sort.sortBy);
            currency = _sqlClient.ogCurrency(User.Identity.Name);

            return Page();
        }

        public IActionResult OnPostData()
        {

            int edit = Convert.ToInt32(Request.Form["edit"]);
            int delete = Convert.ToInt32(Request.Form["delete"]);

            if(edit != 0)
            {
                records = _sqlClient.getRecords(User.Identity.Name);

                HttpContext.Session.SetInt32("editId", edit);

                return RedirectToPage("/editRecord");
            }
            else
            {
                _sqlClient.deleteRecord(delete);

                categories = _sqlClient.getCategories(User.Identity.Name);
                records = _sqlClient.getRecords(User.Identity.Name);
                currency = _sqlClient.ogCurrency(User.Identity.Name);
                return Page();
            }
        }

    }



    public class Sort
    {
        public string sort { get; set; }
        public string sortBy { get; set; }

    }
}

