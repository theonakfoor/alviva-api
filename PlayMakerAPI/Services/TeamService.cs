using Microsoft.Net.Http.Headers;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1;
using PlayMakerAPI.Models.Request;
using PlayMakerAPI.Models.Response;

namespace PlayMakerAPI.Services
{
    public class TeamService
    {
        private DatabaseService _databaseService = new DatabaseService();
        private UserService _userService = new UserService();
        public Response GetAllTeams(int offset = 0)
        {
            ListTeamsResponse response = new ListTeamsResponse();
            List<TeamOverview> results = new List<TeamOverview>();

            _databaseService.Initialize();
            MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) OVER(), CONCAT(C.ClubName, ' ', L.LeagueName, ' ', UPPER(T.Gender),SUBSTR(T.Division, 3)) as 'TeamName', T.TeamID, C.Image, U.LastName, COUNT(P.PlayerID) FROM Teams T LEFT JOIN Clubs C ON (T.ClubID = C.ClubID) LEFT JOIN Leagues L on (T.LeagueID = L.LeagueID) LEFT JOIN Users U ON (T.CoachUserID = U.UserID) LEFT JOIN Players P on (T.TeamID = P.TeamID) GROUP BY T.TeamID LIMIT @Offset,100", _databaseService.Connection);
            cmd.Parameters.AddWithValue("@Offset", offset);

            MySqlDataReader result = cmd.ExecuteReader();

            while (result.Read())
            {
                response.Total = result.GetInt16(0);
                results.Add(new TeamOverview
                {
                    TeamID = (result.IsDBNull(2)) ? null : result.GetInt16(2),
                    TeamName = (result.IsDBNull(1)) ? null : result.GetString(1),
                    ClubImage = (result.IsDBNull(3)) ? null : result.GetString(3),
                    Coach = (result.IsDBNull(4)) ? null : result.GetString(4),
                    PlayerCount = (result.IsDBNull(5)) ? null : result.GetInt16(5)
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
        public Response GetTeamByID(int? id, bool includePlayers)
        {
            TeamResponse? response = null;
            List<Player>? players = new List<Player>();
            int? coach = null;
            int? manager = null;
            List<int>? assistantCoaches = null;

            _databaseService.Initialize();
            MySqlCommand cmd = new MySqlCommand("SELECT T.TeamID, CONCAT(C.ClubName, ' ', L.LeagueName, ' ', UPPER(T.Gender),SUBSTR(T.Division, 3)), T.Gender, T.Division, T.Identifier, T.ManagerUserID, T.CoachUserID, T.AssistantCoachUserIDs, C.ClubID, C.ClubName, C.Identifier, C.AssociationIdentifier, C.Image, L.LeagueID, L.LeagueName, L.Identifier, L.Image FROM Teams T JOIN Clubs C on T.ClubID = C.ClubID JOIN Leagues L on T.LeagueID = L.LeagueID WHERE T.TeamID = @TeamID", _databaseService.Connection);
            cmd.Parameters.AddWithValue("@TeamID", id);

            MySqlDataReader result = cmd.ExecuteReader();

            while (result.Read())
            {
                response = new TeamResponse
                {
                    TeamId = id,
                    TeamName = (result.IsDBNull(1)) ? null : result.GetString(1),
                    Gender = (result.IsDBNull(2)) ? null : result.GetChar(2),
                    AgeDivision = (result.IsDBNull(3)) ? null : result.GetInt16(3),
                    TeamIdentifier = (result.IsDBNull(4)) ? null : result.GetString(4),
                    Club = (result.IsDBNull(8)) ? null : new Club
                    {
                        ClubId = (result.IsDBNull(8)) ? null : result.GetInt32(8),
                        ClubName = (result.IsDBNull(9)) ? null : result.GetString(9),
                        ClubIdentifier = (result.IsDBNull(10)) ? null : result.GetString(10),
                        AssociationIdentifier = (result.IsDBNull(11)) ? null : result.GetString(11),
                        ClubImage = (result.IsDBNull(12)) ? null : result.GetString(12)
                    },
                    League = (result.IsDBNull(13)) ? null : new League
                    {
                        LeagueId = (result.IsDBNull(13)) ? null : result.GetInt32(13),
                        LeagueName = (result.IsDBNull(14)) ? null : result.GetString(14),
                        LeagueIdentifier = (result.IsDBNull(15)) ? null : result.GetString(15),
                        LeagueImage = (result.IsDBNull(16)) ? null : result.GetString(16)
                    }
                };

                assistantCoaches = result.IsDBNull(7) ? null : JsonConvert.DeserializeObject<List<int>>(result.GetString(7));
                manager = result.IsDBNull(5) ? null : result.GetInt16(5);
                coach = result.IsDBNull(6) ? null : result.GetInt16(6);

            }

            _databaseService.Disconnect();

            if (includePlayers)
            {
                _databaseService.Initialize();
                MySqlCommand pCmd = new MySqlCommand("SELECT P.PlayerID, P.FirstName, P.LastName, P.Image, P.DOB, P.PlayerNumber, P.Position FROM Players P WHERE P.TeamID = @TeamID", _databaseService.Connection);
                pCmd.Parameters.AddWithValue("@TeamID", id);

                MySqlDataReader playerResult = pCmd.ExecuteReader();

                while(playerResult.Read())
                {
                    players.Add(new Player
                    {
                        PlayerId = (playerResult.IsDBNull(0)) ? null : playerResult.GetInt32(0),
                        FirstName = (playerResult.IsDBNull(1)) ? null : playerResult.GetString(1),
                        LastName = (playerResult.IsDBNull(2)) ? null : playerResult.GetString(2),
                        UserImage = (playerResult.IsDBNull(3)) ? null : playerResult.GetString(3),
                        DOB = (playerResult.IsDBNull(4)) ? null : playerResult.GetString(4),
                        PlayerNumber = (playerResult.IsDBNull(5)) ? null : playerResult.GetInt16(5),
                        Position = (playerResult.IsDBNull(6)) ? null : playerResult.GetString(6)
                    });
                }

                _databaseService.Disconnect();
            }

            if(response != null)
            {
                response.Manager = (manager == null) ? null : _userService.GetUserByID(manager);
                response.Coach = (coach == null) ? null : _userService.GetUserByID(coach);
                response.Players = (players == null || !includePlayers) ? null : players;

                if(assistantCoaches != null)
                {
                    response.AssistantCoaches = new List<User>();
                    foreach(int acid in assistantCoaches) {
                        response.AssistantCoaches.Add(_userService.GetUserByID(acid));
                    }
                }

                return new Response
                {
                    StatusCode = 200,
                    Data = response
                };
            }

            return new Response
            {
                StatusCode = 500,
                Data = null
            };
        }
        public Response GetPlayerCardsByID(int? id)
        {
            List<PlayerCard> cards = new List<PlayerCard>();

            _databaseService.Initialize();
            MySqlCommand cmd = new MySqlCommand("SELECT P.FirstName, P.LastName, P.Image, P.DOB, P.PlayerNumber, P.PlayerID, T.Gender, T.Division, T.Identifier, C.ClubName, C.Identifier, C.AssociationIdentifier, L.LeagueName, CH.LastName FROM Players P LEFT JOIN Teams T ON (P.TeamID = T.TeamID) LEFT JOIN Clubs C on (T.ClubID = C.ClubID) LEFT JOIN Leagues L on (T.LeagueID = L.LeagueID) LEFT JOIN Users CH ON (T.CoachUserID = CH.UserID) WHERE P.TeamID = @TeamID", _databaseService.Connection);
            cmd.Parameters.AddWithValue("@TeamID", id);

            MySqlDataReader result = cmd.ExecuteReader();

            while(result.Read())
            {
                cards.Add(new PlayerCard
                {
                    PlayerId = (result.IsDBNull(5)) ? null : result.GetInt32(5),
                    FirstName = (result.IsDBNull(0)) ? null : result.GetString(0),
                    LastName = (result.IsDBNull(1)) ? null : result.GetString(1),
                    UserImage = (result.IsDBNull(2)) ? null : result.GetString(2),
                    DOB = (result.IsDBNull(3)) ? null : result.GetString(3),
                    TeamGender = (result.IsDBNull(6)) ? null : result.GetChar(6),
                    TeamAgeDivision = (result.IsDBNull(7)) ? null : result.GetInt16(7),
                    ClubName = (result.IsDBNull(9)) ? null : result.GetString(9),
                    LeagueName = (result.IsDBNull(12)) ? null : result.GetString(12),
                    CoachName = (result.IsDBNull(13)) ? null : result.GetString(13),
                    PlayerNumber = (result.IsDBNull(4)) ? null : result.GetInt16(4),
                    TeamIdentifier = (result.IsDBNull(8)) ? null : result.GetString(8),
                    ClubIdentifier = (result.IsDBNull(10)) ? null : result.GetString(10),
                    AssociationIdentifier = (result.IsDBNull(11)) ? null : result.GetString(11)
                });
            }

            _databaseService.Disconnect();

            if (cards.Count > 0)
                return new Response
                {
                    StatusCode = 200,
                    Data = cards
                };

            return new Response
            {
                StatusCode = 500,
                Data = null
            };

        }
    }
}
