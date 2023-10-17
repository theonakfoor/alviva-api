namespace PlayMakerAPI.Models.Response
{
    public class MatchesResponse
    {
        public int? Total { get; set; }
        public bool HasMore { get; set; }
        public List<Match>? Results { get; set; }
        public int? Offset { get; set; }
    }
    public class Match
    {
        public string? MatchID { get; set; }
        public User? Owner { get; set; }
        public Int64? StartTime { get; set;}
        public Int64? EndTime { get; set;}
        public string? VenueName { get; set; }
        public string? VenueAddress { get; set; }
        public string? VenueNumber { get; set; }
        public int? State { get; set; }
        public int? Period { get; set; }
        public TeamsInfo? Teams { get; set; }
    }
    public class TeamsInfo
    {
        public TeamInfo? Team1 { get; set; }
        public TeamInfo? Team2 { get; set; }
    }
    public class TeamInfo
    {
        public int? TeamID { get; set; }
        public char? Gender { get; set; }
        public int? Division { get; set; }
        public string? TeamName { get; set; }
        public string? CoachLastName { get; set; }
        public string? ClubImage { get; set; }
        public int? OverallScore { get; set; }
        public int? PenaltyKickScore { get; set; }
        public int? Score { get; set; }
    }
}
