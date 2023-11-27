namespace AlvivaAPI.Models.Response.TGS
{
    public class TGS_TeamInfoResponse
    {
        public string? Result { get; set; }
        public TGS_TeamInfoData? Data { get; set; }
        public TGS_ClubInfoObj? TeamInfo { get; set; }
    }
    public class TGS_TeamInfoData
    {
        public string? Place { get; set; }
        public int? Win { get; set; }
        public int? Lose { get; set; }
        public int? Draw { get; set; }
        public List<TGS_TeamOpponent>? OpponentList { get; set; }
        public List<TGS_TeamPlayer>? PlayerList { get; set; }
        public List<TGS_TeamStaff>? StaffList { get; set; }
    }

    public class TGS_TeamOpponent
    {
        public int? MatchID { get; set; }
        public string? MatchDate { get; set; }
        public string? MatchTime { get; set; }
        public string? HomeOrAway { get; set; }
        public string? ClubLogo { get; set; }
        public int? ClubID { get; set; }
        public int? TeamID { get; set; }
        public string? TeamName { get; set; }
        public string? Complex { get; set; }
        public string? Venue { get; set; }
        public string? Result { get; set; }
        public int? TeamScore { get; set; }
        public int? OppScore { get; set; }
        public int? FlightID { get; set; }
        public string? Flag { get; set; }
        public string? Zip { get; set; }
        public int? ComplexID { get; set; }
        public int? VenueID { get; set; }
        public int? EventTypeID { get; set; }
        public int? IncludeOrgStandings { get; set; }
        public int? EventID { get; set; }
        public string? EventName { get; set; }
        public string? EventLogo { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public int? ExistingTeam { get; set; }
        public bool? MatchExists { get; set; }
        public string? ExistingMatch { get; set; }
    }
    public class TGS_TeamPlayer
    {
        public int? UserID { get; set; }
        public string? UserImage { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Position { get; set; }
        public int? GradYear { get; set; }
        public string? JerseyNumber { get; set; }
        public int? PositionID { get; set; }
    }

    public class TGS_TeamStaff
    {
        public int? UserID { get; set; }
        public string? Avatar { get; set; }
        public string? FullName { get; set; }
        public string? CellPhone { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
    }

}
