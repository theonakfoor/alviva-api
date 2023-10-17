using System.ComponentModel.DataAnnotations;

namespace PlayMakerAPI.Models.Request
{
    public class UpdateMatchRequest
    {
        public int? State { get; set; }
        public int? Period { get; set; }
        public Int64? StartTime { get; set; }
        public Int64? EndTime { get; set; }
        public int? Team1ID { get; set; }
        public int? Team2ID { get; set; }
        public long? HalfLength { get; set; }
        public long? HalftimeLength { get; set; }
        public long? FirstHalfLength { get; set; }
        public long? ActualHalftimeLength { get; set; }
        public long? SecondHalfLength { get; set; }
        public int? Second { get; set; }
        public string? VenueName { get; set; }
        public string? VenueAddress { get; set; }
        public string? VenueNumber { get; set; }
        public List<int>? SharedWith { get; set; }
        public string? ExternalID { get; set; }
    }
}
