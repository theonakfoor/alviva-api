using MySql.Data.MySqlClient;
using PlayMakerAPI.Models.Request;
using PlayMakerAPI.Models.Response;

namespace PlayMakerAPI.Services
{
    public class PlayerService
    {
        private DatabaseService _databaseService = new DatabaseService();
        public Response GetLeaderboard(string type = "goals", int offset = 0)
        {
            ListLeaderboardResponse response = new ListLeaderboardResponse();
            List<LeaderboardOverview> results = new List<LeaderboardOverview>();

            _databaseService.Initialize();
            MySqlCommand cmd = new MySqlCommand($"SELECT COUNT(*) OVER(), P.PlayerID, P.Image, P.FirstName, P.LastName, CONCAT(C.ClubName,' ', L.LeagueName,' ',UPPER(T.Gender),SUBSTR(T.Division, 3)) as 'TeamName', C.Image, S.Goals, S.OwnGoals, S.PenaltyKicks, S.YellowCards, S.RedCards, S.Ejections, S.Fouls FROM Players P JOIN Statistics S on (S.PlayerID = P.PlayerID) JOIN Teams T ON (T.TeamID = P.TeamID) JOIN Clubs C ON (C.ClubID = T.ClubID) JOIN Leagues L ON (L.LeagueID = T.LeagueID) ORDER BY {((type.ToLower() == "goals") ? "S.Goals" : "S.PenaltyKicks")} DESC LIMIT @Offset,100", _databaseService.Connection);
            cmd.Parameters.AddWithValue("@Offset", offset);

            var result = cmd.ExecuteReader();

            while(result.Read())
            {
                response.Total = result.GetInt16(0);
                results.Add(new LeaderboardOverview
                {
                    PlayerID = result.GetInt16(1),
                    PlayerImage = (result.IsDBNull(2)) ? null : result.GetString(2),
                    FirstName = result.GetString(3),
                    LastName = result.GetString(4),
                    TeamName = result.GetString(5),
                    ClubImage = (result.IsDBNull(6)) ? null : result.GetString(6),
                    Statistics = new Statistics
                    {
                        Goals = (result.IsDBNull(7)) ? 0 : result.GetInt16(7),
                        OwnGoals = (result.IsDBNull(8)) ? 0 : result.GetInt16(8),
                        PenaltyKicks = (result.IsDBNull(9)) ? 0 : result.GetInt16(9),
                        YellowCards = (result.IsDBNull(10)) ? 0 : result.GetInt16(10),
                        RedCards = (result.IsDBNull(11)) ? 0 : result.GetInt16(11),
                        Ejections = (result.IsDBNull(12)) ? 0 : result.GetInt16(12),
                        Fouls = (result.IsDBNull(13)) ? 0 : result.GetInt16(13)
                    }
                });
            }

            _databaseService.Disconnect();

            response.Results = results;
            response.HasMore = (response.Total > 100 && (offset + 100) < response.Total);
            response.Offset = response.HasMore ? offset + 100 : null;

            return new Response
            {
                StatusCode = 200,
                Data = response
            };
        }
    }
}
