using certainty.Injections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Client;

namespace certainty.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly ISqlClient _client;
        private readonly IOnPostVerification _onPostVerification;
        public RegisterModel(ISqlClient sqlClient, IOnPostVerification onPostVerification)
        {
            _client = sqlClient;
            _onPostVerification = onPostVerification;

        }



        [BindProperty]
        public User User { get; set; }

        public string message { get; private set; } = "";
        
        public void OnGet()
        {
            
        }

        public IActionResult OnPost() {

            bool continueCode = _onPostVerification.checkTries(HttpContext);

            if (continueCode == false)
            {
                return Page();
            }

            message = _client.registerUser(User.username, User.email, User.password);
            if(message == "created")
            {
                return RedirectToPage("Login");
            }
            else
            {
                return Page();
            }
            
            
            
        }
    }

    public class User
    {

        public string username { get; set; }
        public string password { get; set; }
        public string email { get; set; }
    }
}
