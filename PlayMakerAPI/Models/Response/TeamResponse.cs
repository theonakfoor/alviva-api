namespace PlayMakerAPI.Models.Response
{
    public class TeamResponse
    {
        public int? TeamId { get; set; }
        public string? TeamName { get; set; }
        public char? Gender { get; set; }
        public int? AgeDivision { get; set; }
        public string? TeamIdentifier { get; set; }
        public User? Manager { get; set; }
        public User? Coach { get; set; }
        public List<User>? AssistantCoaches { get; set; }
        public Club? Club { get; set; }
        public League? League { get; set; }
        public List<Player>? Players { get; set; }
    }

    public class User
    {
        public int? UserID { get; set; }
        public string? AuthID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? UserImage { get; set; }
        public float? HostRating { get; set; }
    }

    public class Player
    {
        public int? PlayerId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? UserImage { get; set; }
        public string? DOB { get; set; }
        public int? PlayerNumber { get; set; }
    }

    public class Club
    {
        public int? ClubId { get; set; }
        public string? ClubName { get; set; }
        public string? ClubIdentifier { get; set; }
        public string? AssociationIdentifier { get; set; }
        public string? ClubImage { get; set; }
    }

    public class League
    {
        public int? LeagueId { get; set; }
        public string? LeagueName { get; set; }
        public string? LeagueIdentifier { get; set; }
        public string? LeagueImage { get; set; }
    }
}
