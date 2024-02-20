using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using certainty.Injections;
using certainty.Pages.models;
using Microsoft.Identity.Client;
using System;

namespace certainty.Pages
{


    [Authorize]
    public class WebIndexModel : PageModel
    {
        private readonly ISqlClient _sqlClient;

        public WebIndexModel(ISqlClient sqlClient)
        {
            _sqlClient = sqlClient;
        }



        public DateTime today { get; set; }

        //Grid Info
        public int recordsTotal {  get; set; }
        public int recordsLastYear { get; set; }
        public int recordsCurrentMonth {  get; set; }
        public double biggestExpense {  get; set; }
        public double averageExpense { get; set; }
        public double lowestExpense { get; set; }

        //Year Graph
        public int yearAgo { get; set; }
        public int currentYear { get; set; }
        public List<YearRecord> yearRecords { get; set; }
        public string Currency {  get; set; }

        //Month Graph
        public List<int> daysInMonth { get; set; }
        public List<double> valuesMonth { get; set; }

        //Category graph
        public List<Record> categoryValue { get; set; }
        public List<string> colors { get; set; }




        public void OnGet()
        {

            today = DateTime.Now;
            DateTime yearAgoDateTime = today.AddYears(-1);

            //Grid info
            List<Record> records = _sqlClient.getRecords(User.Identity.Name);
            recordsTotal = records.Count;
            List<YearRecord> recordsLastYearList = _sqlClient.getYearRecords(yearAgoDateTime, User.Identity.Name);
            recordsLastYear = 0;
            foreach (YearRecord record in recordsLastYearList)
            {
                recordsLastYear = recordsLastYear + record.recordsCount;
            }
            DateTime firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            List<YearRecord> recordsCurrentMonthList = _sqlClient.getYearRecords(firstDayOfMonth ,User.Identity.Name);
            recordsCurrentMonth = 0;
            foreach (YearRecord record in recordsCurrentMonthList)
            {
                recordsCurrentMonth = recordsCurrentMonth + record.recordsCount;
            }
           

            List<double> values = new List<double>();
            if(records.Count > 0)
            {
                lowestExpense = records[0].value;
                biggestExpense = records[0].value;

                foreach (var record in records)
                {
                    double value = record.value;

                    values.Add(value);
                    if (value <= lowestExpense)
                    {
                        lowestExpense = value;
                    }
                    else if (value >= biggestExpense)
                    {
                        biggestExpense = value;
                    }
                    else
                    {
                        continue;
                    }
                }
                double nmb = values.Average();
                averageExpense = Math.Round(nmb, 2);

            }
            else
            {
                lowestExpense = 0;
                biggestExpense = 0;
                averageExpense = 0;
            }
            


            //first graph
            Currency = _sqlClient.ogCurrency(User.Identity.Name);


            currentYear = today.Year;
            yearAgo = today.AddYears(-1).Year;

            Dictionary<int, int> monthDict = GenerateMonths();

            yearRecords = new List<YearRecord>();
            List<YearRecord> userRecords = _sqlClient.getYearRecords(today.AddYears(-1).Date, User.Identity.Name);


            foreach (var i in monthDict) {
                int month = i.Key;
                int year = i.Value;

                int increase = 0;

                foreach(var record in userRecords)
                {
                    if(month == record.month && year == record.year)
                    {
                        yearRecords.Add(record);
                        increase++;
                        break;
                    }
                    else
                    {
                     
                        continue;

                    }

                }
                if(increase == 0)
                {
                    YearRecord recordDefault = new YearRecord()
                    {
                        month = month,
                        year = year,
                        value = 0,
                        recordsCount = 0
                    };
                    yearRecords.Add(recordDefault);
                }
            }
            yearRecords.Reverse();

            //second graph
            
            daysInMonth = daysInMonthList(today);

            List<Record> recordsSql = _sqlClient.getRecordsThisMonth(User.Identity.Name, today);

            valuesMonth = new List<double>();

            foreach(var day in daysInMonth)
            {
                int increase = 0;

                foreach(var record in recordsSql)
                {
                    if(record.recordDate.Day == day)
                    {
                        valuesMonth.Add(record.value);
                        increase++;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if(increase == 0)
                {
                    valuesMonth.Add(0);
                }
            }

            List<double> idk = valuesMonth;

            //third graph
            categoryValue = _sqlClient.getCategoryValue(User.Identity.Name);
            int count = categoryValue.Count;
            colors = GenerateRandomColors(count);
            
        }
        static List<string> GenerateRandomColors(int numColors)
        {
            Random random = new Random();
            List<string> colors = new List<string>();

            for (int i = 0; i < numColors; i++)
            {
                int red = random.Next(256);
                int green = random.Next(256);
                int blue = random.Next(256);

                string colorString = $"rgba({red},{green},{blue},0.5)";
                colors.Add(colorString);
            }

            return colors;
        }


        public static List<int> daysInMonthList(DateTime day)
        {
            int year = day.Year;
            int month = day.Month;

            List<int> daysInMonth = new List<int>();

            int days = DateTime.DaysInMonth(year, month);

            for(int i = 1; i <= days; i++)
            {
                daysInMonth.Add(i);
            }
            return daysInMonth;

        }

        public static Dictionary<int, int> GenerateMonths()
        {
            Dictionary<int, int> resultMonths = new Dictionary<int, int>();

            DateTime currentDate = DateTime.Now;

            for (int i = 0; i < 12; i++)
            {
                int currentMonth = currentDate.Month - i;
                int currentYear = currentDate.Year;

                if (currentMonth <= 0)
                {
                    currentMonth += 12;
                    currentYear--;
                }

                resultMonths[currentMonth] = currentYear;
            }

            return resultMonths;
        }

    }
}
