using PlayMakerAPI.Models.Request;

namespace PlayMakerAPI.Services
{
    public class VisitorService
    {
        private DatabaseService _databaseService = new DatabaseService();

        public bool LogVisitByIP(VisitRequest request)
        {

            String date = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");

            _databaseService.Initialize();
            _databaseService.ExecuteNonQuery($"INSERT INTO Visits VALUES (null, '{request.IpAddress}', '{request.State}', '{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}')");
            _databaseService.Disconnect();

            return true;
            
        }

    }
}
