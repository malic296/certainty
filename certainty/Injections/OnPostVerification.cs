namespace certainty.Injections
{
    public interface IOnPostVerification
    {
        bool checkTries(HttpContext httpContext);
    }

    public class OnPostVerification: IOnPostVerification
    {
        public bool checkTries(HttpContext httpContext)
        {
            //Checks for number of tries by user (just refreshes it if there was more than 3 attempts during the last 20 seconds)
            string cookieValue;
            if (httpContext.Request.Cookies.TryGetValue("numberOfTries", out cookieValue))
            {
                if (int.TryParse(cookieValue, out int intValue))
                {
                    if (intValue > 2)
                    {

                        return false;
                    }
                    else
                    {
                        DateTime now = DateTime.Now;

                        int seconds = now.Second;
                        int expiresSeconds = Convert.ToInt32(httpContext.Session.GetInt32("expires"));

                        int difference = expiresSeconds - seconds;

                        httpContext.Response.Cookies.Append("numberOfTries", (intValue + 1).ToString(),
                            new CookieOptions { Expires = DateTimeOffset.Now.AddSeconds(difference), HttpOnly = true }
                        );

                        return true;
                    }
                }

                else
                {
                    return true;
                }

            }
            else
            {
                httpContext.Response.Cookies.Append("numberOfTries", "0",
                    new CookieOptions { Expires = DateTimeOffset.Now.AddSeconds(15), HttpOnly = true }
                );

                DateTime Expires = DateTime.Now.AddSeconds(20);
                httpContext.Session.SetInt32("expires", Expires.Second);

                return true;

            }
            //----------------------------------------------------------------------------------------------------------
        }
    }
}
