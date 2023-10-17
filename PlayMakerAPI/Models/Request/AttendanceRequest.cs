using PlayMakerAPI.Models.Response;

namespace PlayMakerAPI.Models.Request
{
    public class AttendanceRequest
    {
        public int Team { get; set; }
        public List<Attendance> Attendance { get; set; }
    }
}
