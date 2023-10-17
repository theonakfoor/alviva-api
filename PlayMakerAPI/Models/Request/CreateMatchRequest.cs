using System.ComponentModel.DataAnnotations;

namespace PlayMakerAPI.Models.Request
{
    public class CreateMatchRequest
    {
        [Required]
        public Int64 StartTime { get; set; }
        [Required]
        public int Team1ID { get; set; }
        [Required]
        public int Team2ID { get; set; }
        [Required]
        public string VenueName { get; set; }
        [Required]
        public string VenueAddress { get; set; }
        [Required]
        public string VenueNumber { get; set; }
        public List<int>? SharedWith { get; set; }
        public string? ExternalID { get; set; }
    }
}
