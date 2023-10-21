namespace PlayMakerAPI.Models.Response
{
    public class FetchRecapsResult
    {
        public List<Recap>? Result { get; set; }
    }
    public class Recap
    {
        public string? RecapID { get; set; }
        public string? OwnerImage { get; set; }
        public string? VenueName { get; set; }
        public string? VenueAddress { get; set; }
        public string? VenueNumber { get; set; }
        public Int64? StartTime { get; set; }
        public int? Period { get; set;  }
        public TeamResponse Team1 { get; set; }
        public int? Team1Score { get; set; }
        public TeamResponse Team2 { get; set; }
        public int? Team2Score { get; set; }
        public List<Event>? Events { get; set; }
        public List<Event>? Display { get; set; }
    }
}
