using Org.BouncyCastle.Asn1.Mozilla;

namespace PlayMakerAPI.Models.Response
{
    public class PlayerStatisticsResponse
    {
        public int Goals { get; set; }
        public int OwnGoals { get; set; }
        public int PenaltyKicks { get; set; }
        public int YellowCards { get; set; }
        public int RedCards { get; set; }
        public int Ejections { get; set; }
        public int Fouls { get; set; }
    }
}
