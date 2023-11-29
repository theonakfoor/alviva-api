namespace AlvivaAPI.Models.Request
{
    public class UpdatePlayerRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set;}
        public string? UserImage { get; set; }
        public int? PlayerNumber { get; set; }
        public string? DOB { get; set; }
        public string? Position { get; set; }
        public int? TeamID { get; set; }
    }
}
