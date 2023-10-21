namespace PlayMakerAPI.Models.Response
{
    public class ListRecapResponse
    {
        public int? Total { get; set; }
        public bool HasMore { get; set; }
        public List<RecapOverview> Results { get; set; }
        public int? Offset { get; set; }
    }

    public class RecapOverview
    {
        public string? RecapID { get; set; }
        public int? Period { get; set; }
        public Int64? StartTime { get; set; }
        public string? ThumbImage { get; set; }
    }
}
