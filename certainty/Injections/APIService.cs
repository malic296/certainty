using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Routing.Constraints;


namespace certainty.Injections
{
    public interface IAPIService
    {
        Task<Dictionary<string, string>> GetCurrenciesDictionaryAsync();
        Task<double> convertCurrency(double value, string end, string start);
    }

    public class APIService : IAPIService
    {
        //změna hodnoty podle momentálních kurzů
        public async Task<double> convertCurrency(double value, string end, string start)
        {


            string urlString = "https://openexchangerates.org/api/latest.json?app_id=578a76a389f44ccb92cdb137e124026f";

            try
            {
                using(var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetStringAsync(urlString);

                    dynamic jsonData = JsonConvert.DeserializeObject<dynamic>(response);

                    Dictionary<string, double> ratesDictionary = JsonConvert.DeserializeObject<Dictionary<string, double>>(jsonData.rates.ToString());

                    double rateStart = 0;
                    double rateEnd = 0;

                    foreach (var rate in ratesDictionary)
                    {
                        if(rate.Key == start)
                        {
                            rateStart = rate.Value;
                        }
                        else if(rate.Key == end)
                        {
                            rateEnd = rate.Value;
                        }
                        
                    }

                    value = value / rateStart;
                    value = value * rateEnd;

                   
                    value = Math.Round(value, 2);

                }

                return value;
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Error fetching data from the API: {ex.Message}");
                throw;
            }
        }
        //vytvoření dictionary z dostupných měn API
        public async Task<Dictionary<string, string>> GetCurrenciesDictionaryAsync()
        {
            string urlString = "https://openexchangerates.org/api/currencies.json";

            try
            {
                using (var httpClient = new HttpClient())
                {
                    // Make the GET request asynchronously using HttpClient
                    var response = await httpClient.GetStringAsync(urlString);

                    // Deserialize the JSON response into a Dictionary<string, string>
                    Dictionary<string, string> currenciesDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);

                    return currenciesDictionary;
                }
            }
            catch (Exception ex)
            {
                // Handle the exception (log or rethrow)
                Console.WriteLine($"Error fetching data from the API: {ex.Message}");
                throw;
            }
        }



    }
}