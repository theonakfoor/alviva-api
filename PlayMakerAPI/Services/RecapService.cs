using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using PlayMakerAPI.Models.Response;

namespace PlayMakerAPI.Services
{
    public class RecapService
    {
        private DatabaseService _databaseService = new DatabaseService();
        private TeamService _teamService = new TeamService();
        public Response GetAllRecaps(int offset = 0)
        {
            _databaseService.Initialize(); 

            ListRecapResponse response = new ListRecapResponse();
            MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) OVER(), M.MatchID, M.StartTime, M.Period, COALESCE(JSON_EXTRACT(M.Data, '$.Team1.OverallScore'), JSON_EXTRACT(M.Data, '$.Team1.Score')), COALESCE(JSON_EXTRACT(M.Data, '$.Team2.OverallScore'), JSON_EXTRACT(M.Data, '$.Team2.Score')), C1.Image, C2.Image, M.Data FROM Matches M LEFT JOIN Teams T1 ON (M.Team1ID = T1.TeamID) LEFT JOIN Clubs C1 ON (T1.ClubID = C1.ClubID) LEFT JOIN Teams T2 ON (M.Team2ID = T2.TeamID) LEFT JOIN Clubs C2 ON (T2.ClubID = C2.ClubID) WHERE M.State >= 2 AND M.Show = 1 ORDER BY M.StartTime DESC LIMIT @Offset,500", _databaseService.Connection);
            cmd.Parameters.AddWithValue("@Offset", offset);
            MySqlDataReader result = cmd.ExecuteReader();

            response.Results = new List<RecapOverview>();

            while (result.Read())
            {
                var events = JsonConvert.DeserializeObject<MatchData>(result.GetString(8)).Events;

                if(events != null)
                {
                    response.Results.Add(new RecapOverview
                    {
                        RecapID = result.IsDBNull(1) ? null : result.GetString(1),
                        StartTime = result.IsDBNull(2) ? null : result.GetInt64(2),
                        Period = result.IsDBNull(3) ? null : result.GetInt16(3),
                        ThumbImage = (result.GetInt16(4) > result.GetInt16(5)) ? ( (result.IsDBNull(6)) ? null : result.GetString(6)) : ( (result.IsDBNull(7) ? null : result.GetString(7)) )
                    });
                }
            }

            _databaseService.Disconnect();

            response.Total = response.Results.Count;
            response.HasMore = (response.Total > 500 && (offset + 500) < response.Total);
            response.Offset = response.HasMore ? offset + 500 : null;

            return new Response
            {
                StatusCode = 200,
                Data = response
            };
        }
        public Response FetchRecaps(List<string> recapIDs)
        {
            FetchRecapsResult response = new FetchRecapsResult();

            response.Result = new List<Recap>();

            foreach (string recapID in recapIDs)
            {
                _databaseService.Initialize();

                MySqlCommand cmd = new MySqlCommand("SELECT O.Image, VenueName, VenueAddress, VenueNumber, StartTime, Period, Team1ID, Team2ID, Data FROM Matches LEFT JOIN Users O ON (Matches.Owner = O.AuthID) WHERE MatchID=@MatchID", _databaseService.Connection);
                cmd.Parameters.AddWithValue("@MatchID", recapID);
                var result = cmd.ExecuteReader();

                while (result.Read())
                {

                    var events = JsonConvert.DeserializeObject<MatchData>(result.GetString(8)).Events;

                    if(events != null)
                    {
                        TeamResponse? team1 = null;
                        TeamResponse? team2 = null;

                        try
                        {
                            team1 = (TeamResponse?)_teamService.GetTeamByID(result.GetInt16(6), true).Data;
                        }
                        catch (Exception e) { }

                        try
                        {
                            team2 = (TeamResponse?)_teamService.GetTeamByID(result.GetInt16(7), true).Data;
                        }
                        catch (Exception e) { }

                        response.Result.Add(new Recap
                        {
                            RecapID = recapID,
                            OwnerImage = result.IsDBNull(0) ? null : result.GetString(0),
                            VenueName = result.IsDBNull(1) ? null : result.GetString(1),
                            VenueAddress = result.IsDBNull(2) ? null : result.GetString(2),
                            VenueNumber = result.IsDBNull(3) ? null : result.GetString(3),
                            StartTime = result.IsDBNull(4) ? null : result.GetInt64(4),
                            Period = result.IsDBNull(5) ? null : result.GetInt16(5),
                            Team1 = team1,
                            Team1Score = 0,
                            Team2 = team2,
                            Team2Score = 0,
                            Events = JsonConvert.DeserializeObject<MatchData>(result.GetString(8)).Events,
                            Display = new List<Event>()
                        });
                    }

                }

                _databaseService.Disconnect();
            }

            return new Response
            {
                StatusCode = 200,
                Data = response
            };
        }
    }
}
