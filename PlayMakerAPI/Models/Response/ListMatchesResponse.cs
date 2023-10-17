namespace PlayMakerAPI.Models.Response
{
    public class ListMatchesResponse
    {
        public int? Total { get; set; }
        public bool HasMore { get; set; }
        public List<MatchDay>? Results { get; set; }
        public int? Offset { get; set; }
    }
    public class MatchDay
    {
        public string? Date { get; set; }
        public List<Match> Matches { get; set; }
    }
}
