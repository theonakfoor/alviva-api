namespace PlayMakerAPI.Models.Response
{
    public class ListLeaderboardResponse
    {
        public int? Total { get; set; }
        public bool HasMore { get; set; }
        public List<LeaderboardOverview>? Results { get; set; }
        public int? Offset { get; set; }
    }
    public class LeaderboardOverview
    {
        public string? Position { get; set; }
        public int PlayerID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PlayerImage { get; set; }
        public string? TeamName { get; set; }
        public string? ClubImage { get; set; }
        public Statistics? Statistics { get ; set; }

    }
    public class Statistics
    {
        public int? Goals { get; set; }
        public int? OwnGoals { get; set; }
        public int? PenaltyKicks { get; set; }
        public int? YellowCards { get; set; }
        public int? RedCards { get; set; }
        public int? Ejections { get; set; }
        public int? Fouls { get; set; }
    }
}
