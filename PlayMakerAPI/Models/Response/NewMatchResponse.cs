namespace PlayMakerAPI.Models.Response
{
    public class NewMatchResponse
    {
        public string MatchID { get; set; }
        public MatchLinks Links { get; set; }
    }

    public class MatchLinks
    {
        public string Viewer { get; set; }
        public string Scorekeeper { get; set; }
        public string Report { get; set; }
    }
}
