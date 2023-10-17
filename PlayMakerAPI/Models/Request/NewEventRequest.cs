namespace PlayMakerAPI.Models.Request
{
    public class NewEventRequest
    {
        public int Team { get; set; }
        public string EventType { get; set; }
        public int PlayerID { get; set; }
        public string? GameTime { get; set; }
        public int? Second { get; set; }

    }
}
