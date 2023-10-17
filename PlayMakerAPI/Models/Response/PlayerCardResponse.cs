namespace PlayMakerAPI.Models.Response
{
    public class PlayerCardResponse
    {
        public List<PlayerCard> PlayerCards { get; set; }
    }

    public class PlayerCard
    {
        public int? PlayerId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? UserImage { get; set; }
        public string? DOB { get; set; }
        public char? TeamGender { get; set; }
        public int? TeamAgeDivision { get; set; }
        public string? ClubName { get; set; }
        public string? LeagueName { get; set; }
        public string? CoachName { get; set; }
        public int? PlayerNumber { get; set; }
        public string? TeamIdentifier { get; set; }
        public string? ClubIdentifier { get; set; }
        public string? AssociationIdentifier { get; set; }
    }
}
