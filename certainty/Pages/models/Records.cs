namespace certainty.Pages.models
{
    public class Record
    {
        public int recordID { get; set; }
        public string category { get; set; }
        public string userID { get; set; }
        public double value { get; set; }
        public DateTime recordDate { get; set; }
    }
}
