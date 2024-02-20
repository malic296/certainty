using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using certainty.Injections;

namespace certainty.Pages
{
    public class finalVerificationModel : PageModel
    {
        private readonly IOnPostVerification _onPostVerification;

        public finalVerificationModel(IOnPostVerification onPostVerification)
        {
            _onPostVerification = onPostVerification;

        }

        [BindProperty]
        public int Code { get; set; }


        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            bool continueCode = _onPostVerification.checkTries(HttpContext);

            if (continueCode == false)
            {
                return Page();
            }

            int code = (int)HttpContext.Session.GetInt32("code");
            int UserInput = Code;
            string cookieValue = Request.Cookies["MyCookie"];
            if (cookieValue != null && code == UserInput)
            {
                
                string uname = HttpContext.Session.GetString("username");
                string email = HttpContext.Session.GetString("email");
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, uname),
                    new Claim(ClaimTypes.Email, email)
                };

                var identity = new ClaimsIdentity(claims, "MyCookieAuth");

                ClaimsPrincipal principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync("MyCookieAuth", principal);



                return RedirectToPage("/WebIndex");
            }

            else if(cookieValue == null && code == UserInput)
            {
                HttpContext.Session.SetString("loginMess", "Time Ran Out!");
                return RedirectToPage("/Login");
            }
            else
            {
                HttpContext.Session.SetString("loginMess", "Wrong Code!");
                return RedirectToPage("/Login");
            }



        }
    }

}
