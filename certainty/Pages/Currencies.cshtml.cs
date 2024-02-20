using certainty.Injections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace certainty.Pages
{
    [Authorize]
    public class CurrenciesModel : PageModel
    {

        private readonly IAPIService _apiService;
        private readonly ISqlClient _sqlClient;
        private readonly IOnPostVerification _onPostVerification;

        public CurrenciesModel(IAPIService APIService, ISqlClient sqlClient, IOnPostVerification onPostVerification)
        {
            _apiService = APIService;
            _sqlClient = sqlClient;
            _onPostVerification = onPostVerification;
        }

        [BindProperty] 
        public Currency currency { get; set; }

        public async Task OnGet()
        {
            
            currencies = await _apiService.GetCurrenciesDictionaryAsync();

            HttpContext.Session.SetString("currencyCheck", "");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            bool continueCode = _onPostVerification.checkTries(HttpContext);

            if (continueCode == false)
            {
                return Page();
            }

            if (ModelState.IsValid)
            {
               
                currencies = await _apiService.GetCurrenciesDictionaryAsync();

                bool curExists = false;

                foreach (var cur in currencies)
                {
                    if(currency.name == cur.Key)
                    {
                        curExists = true;
                        break;
                    }
                    else if(currency.name == cur.Value)
                    {
                        currency.name = cur.Key;
                        curExists = true;
                        break;
                    }
                    else
                    {
                        
                        continue;
                    }
                    
                }



                if(curExists) {
                    List<double> values = _sqlClient.getValues(User.Identity.Name);

                    string start = _sqlClient.ogCurrency(User.Identity.Name);
                    string end = currency.name;

                    if(start == end)
                    {
                        HttpContext.Session.SetString("currencyCheck", $"Your currency is already set to {end}");
                    }
                    else
                    {
                        foreach (var value in values)
                        {
                            double newValue = await _apiService.convertCurrency(value, end, start);
                            _sqlClient.finalCurrencyUpdate(value, newValue, User.Identity.Name);
                        }

                        bool changed = _sqlClient.setCurrency(end, User.Identity.Name);

                        HttpContext.Session.SetString("currencyCheck", $"Your currency has been successfully changed to {end}");
                    }
                    
                }
                else
                {
                    HttpContext.Session.SetString("currencyCheck", "Currency not found - Enter either shortcut or full name of the currency you want to use - Example: 'USD' / 'United States Dollar'");
                }

                
                

            }


            return Page();
        }


        public string success {  get; set; }
        public Dictionary<string, string> currencies {  get; set; }

        
    }

    public class Currency
    {
        public string name { get; set; }
    }
}
