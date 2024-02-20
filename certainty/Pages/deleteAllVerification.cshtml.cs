using certainty.Injections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace certainty.Pages
{
    [Authorize]
    public class deleteAllVerificationModel : PageModel
    {
        private readonly ISqlClient _client;

        public deleteAllVerificationModel(ISqlClient client)
        {
            _client = client;
        }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            _client.deleteAllRecords(User.Identity.Name);

            return RedirectToPage("Records");
        }
    }
}
