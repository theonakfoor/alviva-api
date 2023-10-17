namespace PlayMakerAPI.Models.Response
{
    public class MatchResponse
    {
        public int State { get; set; }
        public User? Owner { get; set; }
        public Teams? Teams { get; set; }
        public TimeInfo? GameTime { get; set; }
        public string? VenueAddress { get; set; }
        public string? VenueNumber { get; set; }
        public List<Referee>? Referees { get; set; }
        public MatchData? Match { get; set; }
    }
    public class MatchData
    {
        public TeamAttendance? Team1 { get; set; }
        public TeamAttendance? Team2 { get; set; }
        public List<Event>? Events { get; set; }
    }

    public class TimeInfo
    {
        public int Period { get; set; }
        public long? HalfLength { get; set; }
        public long? HalftimeLength { get; set; }
        public long? FirstHalfLength { get; set; }
        public long? ActualHalftimeLength { get; set; }
        public long? SecondHalfLength { get; set; }
        public long StartTime { get; set; }
        public long? EndTime { get; set; }
    }

    public class Teams
    {
        public TeamResponse Team1 { get; set; }
        public TeamResponse Team2 { get; set; }
    }

    public class TeamAttendance
    {
        public int OverallScore { get; set; }
        public int Score { get; set; }
        public int PenaltyKickScore { get; set; }
        public List<Attendance>? Attendance { get; set; }
    }

    public class Attendance
    {
        public int PlayerId { get; set; }
        public string? AttendanceStatus { get; set; }

    }

    public class Event
    {
        public string? EventID { get; set; }
        public int? Team { get; set; }
        public ScoreObj? Score { get; set; }
        public string? GameTime { get; set; }
        public int? Second { get; set; }
        public int? PlayerId { get; set; }
        public string EventType { get; set; }
    }

    public class ScoreObj
    {
        public int Team1 { get; set; }
        public int Team2 { get; set; }
        public int Team1Overall { get; set; }
        public int Team2Overall { get; set; }
        public int Team1PenaltyKicks { get; set; }
        public int Team2PenaltyKicks { get; set; }
    }

    public class GameSettings
    {
        public long? HalfLength { get; set; }
        public long? HalftimeLength { get; set; }
        public long? FirstHalfLength { get; set; }
        public long? ActualHalftimeLength { get; set; }
        public long? SecondHalfLength { get; set; }
    }

    public class Referee
    {
        public string? Type { get; set; }
        public string? Name { get; set; }
    }
}
