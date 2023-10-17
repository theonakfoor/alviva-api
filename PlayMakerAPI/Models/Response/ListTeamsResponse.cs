namespace PlayMakerAPI.Models.Response
{
    public class ListTeamsResponse
    {
        public int? Total { get; set; }
        public bool HasMore { get; set; }
        public List<TeamOverview>? Results { get; set; }
        public int? Offset { get; set; }
    }
    public class TeamOverview
    {
        public int? TeamID { get; set; }
        public string? TeamName { get; set; }
        public string? ClubImage { get; set; }
    }
}
