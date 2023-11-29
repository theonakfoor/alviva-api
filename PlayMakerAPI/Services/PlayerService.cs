using AlvivaAPI.Models.Request;
using MySql.Data.MySqlClient;
using PlayMakerAPI.Models.Request;
using PlayMakerAPI.Models.Response;

namespace PlayMakerAPI.Services
{
    public class PlayerService
    {
        private DatabaseService _databaseService = new DatabaseService();
        private AdminService _adminService = new AdminService();
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
        public Response GetPlayerStatistics(int playerId)
        {
            PlayerStatisticsResponse response = new PlayerStatisticsResponse();

            _databaseService.Initialize();
            MySqlCommand cmd = new MySqlCommand("SELECT Goals, OwnGoals, PenaltyKicks, YellowCards, RedCards, Ejections, Fouls FROM Statistics WHERE PlayerID=@PlayerID", _databaseService.Connection);
            cmd.Parameters.AddWithValue("@PlayerID", playerId);
            var result = cmd.ExecuteReader();

            while (result.Read())
            {
                response = new PlayerStatisticsResponse
                {
                    Goals = (result.IsDBNull(0)) ? 0 : result.GetInt16(0),
                    OwnGoals = (result.IsDBNull(1)) ? 0 : result.GetInt16(1),
                    PenaltyKicks = (result.IsDBNull(2)) ? 0 : result.GetInt16(2),
                    YellowCards = (result.IsDBNull(3)) ? 0 : result.GetInt16(3),
                    RedCards = (result.IsDBNull(4)) ? 0 : result.GetInt16(4),
                    Ejections = (result.IsDBNull(5)) ? 0 : result.GetInt16(5),
                    Fouls = (result.IsDBNull(6)) ? 0 : result.GetInt16(6)
                };
            }

            _databaseService.Disconnect();

            return new Response
            {
                StatusCode = 200,
                Data = response
            };
        }
        public bool UpdatePlayerByID(int playerId, string user, UpdatePlayerRequest request)
        {
            if(_adminService.VerifyUserIsAdmin(user))
            {
                _databaseService.Initialize();
                MySqlCommand cmd = new MySqlCommand("UPDATE Players SET FirstName=@FirstName, LastName=@LastName, Image=@UserImage, PlayerNumber=@PlayerNumber, DOB=@DOB, Position=@Position, TeamID=@TeamID WHERE PlayerID=@PlayerID", _databaseService.Connection);
                cmd.Parameters.AddWithValue("@PlayerID", playerId);
                cmd.Parameters.AddWithValue("@FirstName", request.FirstName);
                cmd.Parameters.AddWithValue("@LastName", request.LastName);
                cmd.Parameters.AddWithValue("@UserImage", request.UserImage);
                cmd.Parameters.AddWithValue("@PlayerNumber", request.PlayerNumber);
                cmd.Parameters.AddWithValue("@DOB", request.DOB);
                cmd.Parameters.AddWithValue("@Position", request.Position);
                cmd.Parameters.AddWithValue("@TeamID", request.TeamID);
                var result = cmd.ExecuteNonQuery();
                _databaseService.Disconnect();

                if (result > 0)
                    return true;
            }

            return false;
        }
        public bool DeletePlayerByID(int playerId, string user)
        {
            if(_adminService.VerifyUserIsAdmin(user))
            {
                _databaseService.Initialize();
                MySqlCommand cmd = new MySqlCommand("DELETE FROM Players WHERE PlayerID=@PlayerID", _databaseService.Connection);
                cmd.Parameters.AddWithValue("@PlayerID", playerId);
                var result = cmd.ExecuteNonQuery();
                _databaseService.Disconnect();

                if (result > 0)
                    return true;
            }

            return false;
        }

        public Response CreatePlayer(string user, UpdatePlayerRequest request)
        {
            if(_adminService.VerifyUserIsAdmin(user))
            {
                _databaseService.Initialize();
                MySqlCommand cmd = new MySqlCommand("INSERT INTO Players VALUES(null, @TeamID, @User, @FirstName, @LastName, @UserImage, @PlayerNumber, @DOB, @Position, null); SELECT LAST_INSERT_ID();", _databaseService.Connection);
                cmd.Parameters.AddWithValue("@FirstName", request.FirstName);
                cmd.Parameters.AddWithValue("@LastName", request.LastName);
                cmd.Parameters.AddWithValue("@UserImage", request.UserImage);
                cmd.Parameters.AddWithValue("@PlayerNumber", request.PlayerNumber);
                cmd.Parameters.AddWithValue("@DOB", request.DOB);
                cmd.Parameters.AddWithValue("@Position", request.Position);
                cmd.Parameters.AddWithValue("@User", user);
                cmd.Parameters.AddWithValue("@TeamID", request.TeamID);
                var result = cmd.ExecuteReader();

                Int32 createdID = -1;

                while(result.Read())
                {
                    createdID = (result.IsDBNull(0)) ? -1 : result.GetInt32(0);
                }

                _databaseService.Disconnect();

                if (createdID > -1)
                    return new Response
                    {
                        StatusCode = 200,
                        Data = new
                        {
                            PlayerId = createdID
                        }
                    };

                return new Response
                {
                    StatusCode = 500,
                    Data = null
                };
            }

            return new Response
            {
                StatusCode = 401,
                Data = null
            };
        }
    }
}
