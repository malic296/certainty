using certainty.Injections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using certainty.Pages.models;

namespace certainty.Pages
{
    [Authorize]
    public class editRecordModel : PageModel
    {
        private readonly ISqlClient _client;

        public editRecordModel(ISqlClient client)
        {
            _client = client;
        }

        [BindProperty]
        public Record record { get; set; }

        public List<string> categories { get; set; }
        public string currency { get; set; }

        public IActionResult OnGet()
        {
            if (HttpContext.Session.TryGetValue("editId", out byte[] value)) {
                int ID = Convert.ToInt32(HttpContext.Session.GetInt32("editId"));
                categories = _client.getCategories(User.Identity.Name);
                currency = _client.ogCurrency(User.Identity.Name);


                record = _client.getRecord(ID);
                return Page();
            }
            else
            {
                return RedirectToPage("Records");
            }
        }

        public IActionResult OnPost()
        {

            int id = Convert.ToInt32(HttpContext.Session.GetInt32("editId"));

            bool update = _client.updateRecord(id, record.category, record.value, record.recordDate);

            HttpContext.Session.Remove("editId");

            

            return RedirectToPage("Records");
        }
    }

}
