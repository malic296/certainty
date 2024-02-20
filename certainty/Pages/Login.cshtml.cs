using certainty.Injections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using System.Net;
using System.Reflection.Emit;

namespace certainty.Pages
{
    public class LoginModel : PageModel
    {
        private readonly ISqlClient _client;
        private readonly IEmailSenderService _emailSenderService;
        private readonly IOnPostVerification _onPostVerification;
        public LoginModel(ISqlClient sqlClient, IEmailSenderService emailSenderService, IOnPostVerification onPostVerification)
        {
            _client = sqlClient;
            _emailSenderService = emailSenderService;
            _onPostVerification = onPostVerification;
        }


        [BindProperty]
        public loginCredentials Credentials { get; set; }

        public string message { get; private set; } = "";

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost()
        {
            //checks number of tries 
            bool continueCode = _onPostVerification.checkTries(HttpContext);

            if(continueCode == false)
            {
                return Page();
            }
            

            bool validation = _client.validateLogin(Credentials.Username, Credentials.Password);
            if (validation == true)
            {
                //sending code to mail
                string code = _emailSenderService.CreateCode();
                string Message = $"Your code is: {code}";

                string email = _client.getColumn(Credentials.Username);
                
                await _emailSenderService.SendEmail(email, "Certainty - Security Code", Message);

                //creating session
                HttpContext.Session.SetInt32("code", int.Parse(code));
                HttpContext.Session.SetString("username", Credentials.Username);
                HttpContext.Session.SetString("email", email);

                //creating cookie
                string cookieName = "MyCookie";
                string cookievalue = "timer";

                Response.Cookies.Append(cookieName, cookievalue, 
                    new CookieOptions { Expires = DateTimeOffset.Now.AddMinutes(5)} 
                );

                return RedirectToPage("/finalVerification");
            }
            else
            {
                HttpContext.Session.SetString("loginMess", "Wrong Credentials!");
                return Page();
                
            }

            
        }


    }

    public class loginCredentials
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
