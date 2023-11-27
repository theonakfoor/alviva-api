using MySql.Data.MySqlClient;
using PlayMakerAPI.Models.Request;
using PlayMakerAPI.Models.Response;
using Newtonsoft.Json;
using PusherServer;
using MySqlX.XDevAPI.Common;

namespace PlayMakerAPI.Services
{
    public class MatchService
    {
        private DatabaseService _databaseService = new DatabaseService();
        private AdminService _adminService = new AdminService();
        private TeamService _teamService = new TeamService();
        public Response CreateMatch(string owner, CreateMatchRequest request)
        {
            var matchID = Guid.NewGuid().ToString().Substring(0, 8);

            _databaseService.Initialize();
            MySqlCommand cmd = new MySqlCommand($"INSERT INTO Matches VALUES (@MatchID, @Owner, @StartTime, null, @VenueName, @VenueAddress, @VenueNumber, 0, 0, @Team1, @Team2, '{{}}', '{{\"HalfLength\": 2400, \"HalftimeLength\": 600, \"FirstHalfLength\": null, \"SecondHalfLength\": null, \"ActualHalftimeLength\": null}}', '[]', '[]', 1, @ExternalID)", _databaseService.Connection);
            cmd.Parameters.AddWithValue("@MatchID", matchID);
            cmd.Parameters.AddWithValue("@Owner", owner);
            cmd.Parameters.AddWithValue("@StartTime", request.StartTime + 300000);
            cmd.Parameters.AddWithValue("@VenueName", request.VenueName);
            cmd.Parameters.AddWithValue("@VenueAddress", request.VenueAddress);
            cmd.Parameters.AddWithValue("@VenueNumber", request.VenueNumber);
            cmd.Parameters.AddWithValue("@Team1", request.Team1ID);
            cmd.Parameters.AddWithValue("@Team2", request.Team2ID);
            cmd.Parameters.AddWithValue("@ExternalID", request.ExternalID);

            var result = cmd.ExecuteNonQuery();
            _databaseService.Disconnect();

            if (result != 0)
            {
                return new Response
                {
                    StatusCode = 201,
                    Data = new NewMatchResponse
                    {
                        MatchID = matchID,
                        Links = new MatchLinks
                        {
                            Viewer = $"https://alviva.live/{matchID}",
                            Scorekeeper = $"https://alviva.live/s/{matchID}",
                            Report = $"https://alviva.live/r/{matchID}"
                        }
                    }
                };
            }

            return new Response
            {
                StatusCode = 500,
                Data = null
            };
        }
        public Response GetAllMatches(int tzOffset, int offset = 0, string type = "upcoming", string? owner = null, ListMatchesRequest? request = null)
        {
            string teamParams = (request != null && request.SearchTeams != null && request.SearchTeams.Count == 1) ? $"AND (M.Team1ID = {request.SearchTeams[0]} OR M.Team2ID = {request.SearchTeams[0]})" : (request != null && request.SearchTeams != null && request.SearchTeams.Count == 2) ? $"AND ( (M.Team1ID = {request.SearchTeams[0]} AND M.Team2ID = {request.SearchTeams[1]}) OR (M.Team1ID = {request.SearchTeams[1]} AND M.Team2ID = {request.SearchTeams[0]}) )" : "";

            ListMatchesResponse response = new ListMatchesResponse();
            List<MatchDay> matches = new List<MatchDay>();
            List<MatchDay> completed = new List<MatchDay>();

            DateTime now = DateTime.UtcNow;
            DateTime today = now.AddMinutes(tzOffset).Date.AddMinutes(-tzOffset);

            var nowMs = ((DateTimeOffset)now).ToUnixTimeMilliseconds();
            var todayMs = ((DateTimeOffset)today).ToUnixTimeMilliseconds();

            MySqlDataReader? result = null;

            _databaseService.Initialize();

            switch (type.ToLower())
            {
                case "upcoming":
                    MySqlCommand upcomingCmd = new MySqlCommand($"SELECT COUNT(*) OVER(), M.MatchID, M.StartTime, M.EndTime, M.VenueName, M.VenueAddress, M.VenueNumber, M.State, M.Period, T1.TeamID, T1.Gender, T1.Division, CONCAT(C1.ClubName,' ',L1.LeagueName,' ', UPPER(T1.Gender), SUBSTR(T1.Division, 3)) as 'Team1Name', CH1.LastName as 'CoachLastName', C1.Image, JSON_EXTRACT(M.Data, '$.Team1.OverallScore'), JSON_EXTRACT(M.Data, '$.Team1.Score'), JSON_EXTRACT(M.data, '$.Team1.PenaltyKickScore'), T2.TeamID, T2.Gender, T2.Division, CONCAT(C2.ClubName,' ',L2.LeagueName,' ',UPPER(T2.Gender),SUBSTR(T2.Division, 3)) as 'Team2Name', CH2.LastName as 'CoachLastName', C2.Image, JSON_EXTRACT(M.Data, '$.Team2.OverallScore'), JSON_EXTRACT(M.Data, '$.Team2.Score'),  JSON_EXTRACT(M.data, '$.Team2.PenaltyKickScore'), O.UserID, O.FirstName, O.LastName, O.Image, O.AuthID, COALESCE(R.RatingSum/R.RatingCount, NULL) FROM Matches M LEFT JOIN Teams T1 ON (M.Team1ID = T1.TeamID) LEFT JOIN Clubs C1 ON (T1.ClubID = C1.ClubID) LEFT JOIN Leagues L1 ON (T1.LeagueID = L1.LeagueID) LEFT JOIN Users CH1 ON (T1.CoachUserID = CH1.UserID) LEFT JOIN Teams T2 on (M.Team2ID = T2.TeamID) LEFT JOIN Clubs C2 ON (T2.ClubID = C2.ClubID) LEFT JOIN Leagues L2 ON (T2.LeagueID = L2.LeagueID) LEFT JOIN Users CH2 ON (T2.CoachUserID = CH2.UserID) LEFT JOIN Users O ON (M.Owner = O.AuthID) LEFT JOIN ( SELECT HostUserID, SUM(Rating) RatingSum, COUNT(*) RatingCount FROM Ratings GROUP BY HostUserID ) R ON O.UserID = R.HostUserID WHERE M.Show = 1 AND M.StartTime >= @TodayMS {teamParams} {((owner != null) ? "AND M.Owner = @OwnerID" : "")} ORDER BY M.StartTime ASC LIMIT @Offset,50", _databaseService.Connection);
                    upcomingCmd.Parameters.AddWithValue("@TodayMS", todayMs);
                    upcomingCmd.Parameters.AddWithValue("@Offset", offset);
                    if (owner != null)
                        upcomingCmd.Parameters.AddWithValue("@OwnerID", owner);
                    result = upcomingCmd.ExecuteReader();
                    break;
                case "completed":
                    MySqlCommand completedCmd = new MySqlCommand($"SELECT COUNT(*) OVER(), M.MatchID, M.StartTime, M.EndTime, M.VenueName, M.VenueAddress, M.VenueNumber, M.State, M.Period, T1.TeamID, T1.Gender, T1.Division, CONCAT(C1.ClubName,' ',L1.LeagueName,' ', UPPER(T1.Gender), SUBSTR(T1.Division, 3)) as 'Team1Name', CH1.LastName as 'CoachLastName', C1.Image, JSON_EXTRACT(M.Data, '$.Team1.OverallScore'), JSON_EXTRACT(M.Data, '$.Team1.Score'), JSON_EXTRACT(M.data, '$.Team1.PenaltyKickScore'), T2.TeamID, T2.Gender, T2.Division, CONCAT(C2.ClubName,' ',L2.LeagueName,' ',UPPER(T2.Gender),SUBSTR(T2.Division, 3)) as 'Team2Name', CH2.LastName as 'CoachLastName', C2.Image, JSON_EXTRACT(M.Data, '$.Team2.OverallScore'), JSON_EXTRACT(M.Data, '$.Team2.Score'),  JSON_EXTRACT(M.data, '$.Team2.PenaltyKickScore'), O.UserID, O.FirstName, O.LastName, O.Image, O.AuthID, COALESCE(R.RatingSum/R.RatingCount, NULL) FROM Matches M LEFT JOIN Teams T1 ON (M.Team1ID = T1.TeamID) LEFT JOIN Clubs C1 ON (T1.ClubID = C1.ClubID) LEFT JOIN Leagues L1 ON (T1.LeagueID = L1.LeagueID) LEFT JOIN Users CH1 ON (T1.CoachUserID = CH1.UserID) LEFT JOIN Teams T2 on (M.Team2ID = T2.TeamID) LEFT JOIN Clubs C2 ON (T2.ClubID = C2.ClubID) LEFT JOIN Leagues L2 ON (T2.LeagueID = L2.LeagueID) LEFT JOIN Users CH2 ON (T2.CoachUserID = CH2.UserID) LEFT JOIN Users O ON (M.Owner = O.AuthID) LEFT JOIN ( SELECT HostUserID, SUM(Rating) RatingSum, COUNT(*) RatingCount FROM Ratings GROUP BY HostUserID ) R ON O.UserID = R.HostUserID WHERE M.Show = 1 AND (M.Period >= 4 AND M.StartTime < @NowMS) {teamParams} {((owner != null) ? "AND M.Owner = @OwnerID" : "")} ORDER BY M.StartTime DESC LIMIT @Offset,50", _databaseService.Connection);
                    completedCmd.Parameters.AddWithValue("@NowMS", nowMs);
                    completedCmd.Parameters.AddWithValue("@Offset", offset);
                    if (owner != null)
                        completedCmd.Parameters.AddWithValue("@OwnerID", owner);
                    result = completedCmd.ExecuteReader();
                    break;
            }

            while (result.Read())
            {
                response.Total = result.GetInt16(0);
                string day = UnixToDateString(result.GetInt64(2), tzOffset);

                Match match = new Match
                {
                    MatchID = (result.IsDBNull(1)) ? null : result.GetString(1),
                    Owner = (result.IsDBNull(27)) ? null : new User
                    {
                        UserID = (result.IsDBNull(27)) ? null : result.GetInt32(27),
                        AuthID = (result.IsDBNull(31)) ? null : result.GetString(31),
                        FirstName = (result.IsDBNull(28)) ? null : result.GetString(28),
                        LastName = (result.IsDBNull(29)) ? null : result.GetString(29),
                        UserImage = (result.IsDBNull(30)) ? null : result.GetString(30),
                        HostRating = (result.IsDBNull(32)) ? null : result.GetFloat(32)
                    },
                    StartTime = (result.IsDBNull(2)) ? null : result.GetInt64(2),
                    EndTime = (result.IsDBNull(3)) ? null : result.GetInt64(3),
                    VenueName = (result.IsDBNull(4)) ? null : result.GetString(4),
                    VenueAddress = (result.IsDBNull(5)) ? null : result.GetString(5),
                    VenueNumber = (result.IsDBNull(6)) ? null : result.GetString(6),
                    State = (result.IsDBNull(7)) ? null : result.GetInt16(7),
                    Period = (result.IsDBNull(8)) ? null : result.GetInt16(8),
                    Teams = new TeamsInfo
                    {
                        Team1 = new TeamInfo
                        {
                            TeamID = (result.IsDBNull(9)) ? null : result.GetInt16(9),
                            Gender = (result.IsDBNull(10)) ? null : result.GetChar(10),
                            Division = (result.IsDBNull(11)) ? null : result.GetInt16(11),
                            TeamName = (result.IsDBNull(12)) ? null : result.GetString(12),
                            CoachLastName = (result.IsDBNull(13)) ? null : result.GetString(13),
                            ClubImage = (result.IsDBNull(14)) ? null : result.GetString(14),
                            OverallScore = (result.IsDBNull(15)) ? null : result.GetInt16(15),
                            Score = (result.IsDBNull(16)) ? null : result.GetInt16(16),
                            PenaltyKickScore = (result.IsDBNull(17)) ? null : result.GetInt16(17)
                        },
                        Team2 = new TeamInfo
                        {
                            TeamID = (result.IsDBNull(18)) ? null : result.GetInt16(18),
                            Gender = (result.IsDBNull(19)) ? null : result.GetChar(19),
                            Division = (result.IsDBNull(20)) ? null : result.GetInt16(20),
                            TeamName = (result.IsDBNull(21)) ? null : result.GetString(21),
                            CoachLastName = (result.IsDBNull(22)) ? null : result.GetString(22),
                            ClubImage = (result.IsDBNull(23)) ? null : result.GetString(23),
                            OverallScore = (result.IsDBNull(24)) ? null : result.GetInt16(24),
                            Score = (result.IsDBNull(25)) ? null : result.GetInt16(25),
                            PenaltyKickScore = (result.IsDBNull(26)) ? null : result.GetInt16(26)
                        }
                    }
                };

                if (type.ToLower() == "upcoming" && match.Period >= 4)
                {
                    MatchDay? matchDay = completed.SingleOrDefault(item => item.Date == day);
                    if (matchDay != null)
                    {
                        matchDay.Matches.Add(match);
                    }
                    else
                    {
                        MatchDay newMatchDay = new MatchDay
                        {
                            Date = day,
                            Matches = new List<Match>()
                        };
                        newMatchDay.Matches.Add(match);
                        completed.Add(newMatchDay);
                    }
                }
                else
                {
                    MatchDay? matchDay = matches.SingleOrDefault(item => item.Date == day);
                    if (matchDay != null)
                    {
                        matchDay.Matches.Add(match);
                    }
                    else
                    {
                        MatchDay newMatchDay = new MatchDay
                        {
                            Date = day,
                            Matches = new List<Match>()
                        };
                        newMatchDay.Matches.Add(match);
                        matches.Add(newMatchDay);
                    }
                }
            }

            _databaseService.Disconnect();

            if (type == "upcoming")
            {
                completed.ForEach((completedDay) =>
                {
                    var matchDay = matches.SingleOrDefault(match => match.Date == completedDay.Date);
                    if (matchDay != null)
                    {
                        matchDay.Matches = matchDay.Matches.Concat(completedDay.Matches).ToList();
                    }
                    else
                    {
                        matches.Insert(0, completedDay);
                    }
                });
            }

            response.HasMore = (response.Total > 50 && (offset + 50) < response.Total);
            response.Results = matches;
            response.Offset = response.HasMore ? offset + 50 : null;

            return new Response
            {
                StatusCode = 200,
                Data = response
            };

        }
        public Response GetMatchById(string id)
        {
            MatchResponse response = null;

            _databaseService.Initialize();
            MySqlCommand cmd = new MySqlCommand("SELECT StartTime, EndTime, VenueAddress, VenueNumber, State, Period, Team1ID, Team2ID, Data, Settings, Referees, SharedWith,O.UserID, O.FirstName, O.LastName, O.Image, O.AuthID FROM Matches LEFT JOIN Users O ON (Matches.Owner = O.AuthID) WHERE MatchID=@MatchID", _databaseService.Connection);
            cmd.Parameters.AddWithValue("@MatchID", id);
            var result = cmd.ExecuteReader();

            while (result.Read())
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

                GameSettings? settings = JsonConvert.DeserializeObject<GameSettings>(result.GetString(9));

                response = new MatchResponse
                {
                    State = result.GetInt16(4),
                    Owner = (result.IsDBNull(12)) ? null : new User
                    {
                        UserID = result.IsDBNull(12) ? null : result.GetInt32(12),
                        AuthID = result.IsDBNull(16) ? null : result.GetString(16),
                        FirstName = result.IsDBNull(13) ? null : result.GetString(13),
                        LastName = result.IsDBNull(14) ? null : result.GetString(14),
                        UserImage = result.IsDBNull(15) ? null : result.GetString(15)
                    },
                    Teams = new Teams
                    {
                        Team1 = team1,
                        Team2 = team2
                    },
                    VenueAddress = result.IsDBNull(2) ? null : result.GetString(2),
                    VenueNumber = result.IsDBNull(3) ? null : result.GetString(3),
                    GameTime = new TimeInfo
                    {
                        Period = result.GetInt16(5),
                        HalfLength = settings.HalfLength,
                        HalftimeLength = settings.HalftimeLength,
                        FirstHalfLength = settings.FirstHalfLength,
                        SecondHalfLength = settings.SecondHalfLength,
                        ActualHalftimeLength = settings.ActualHalftimeLength,
                        StartTime = result.GetInt64(0),
                        EndTime = result.IsDBNull(1) ? null : result.GetInt64(1)
                    },
                    Referees = result.IsDBNull(10) ? null : JsonConvert.DeserializeObject<List<Referee>>(result.GetString(10)),
                    Match = JsonConvert.DeserializeObject<MatchData>(result.GetString(8))
                };
            }

            _databaseService.Disconnect();

            if (response != null)
                return new Response
                {
                    StatusCode = 200,
                    Data = response
                };

            return new Response
            {
                StatusCode = 500,
                Data = null
            };
        }
        public Response GetMatchById(string owner, string id)
        {
            //if (VerifyOwner(owner, id))
            //{
                MatchResponse response = null;

                _databaseService.Initialize();
                MySqlCommand cmd = new MySqlCommand("SELECT StartTime, EndTime, VenueAddress, VenueNumber, State, Period, Team1ID, Team2ID, Data, Settings, Referees, SharedWith,O.UserID, O.FirstName, O.LastName, O.Image, O.AuthID FROM Matches LEFT JOIN Users O ON (Matches.Owner = O.AuthID) WHERE MatchID=@MatchID", _databaseService.Connection);
                cmd.Parameters.AddWithValue("@MatchID", id);
                var result = cmd.ExecuteReader();

                while (result.Read())
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

                    GameSettings? settings = JsonConvert.DeserializeObject<GameSettings>(result.GetString(9));

                    response = new MatchResponse
                    {
                        State = result.GetInt16(4),
                        Owner = (result.IsDBNull(12)) ? null : new User
                        {
                            UserID = result.IsDBNull(12) ? null : result.GetInt32(12),
                            AuthID = result.IsDBNull(16) ? null : result.GetString(16),
                            FirstName = result.IsDBNull(13) ? null : result.GetString(13),
                            LastName = result.IsDBNull(14) ? null : result.GetString(14),
                            UserImage = result.IsDBNull(15) ? null : result.GetString(15)
                        },
                        Teams = new Teams
                        {
                            Team1 = team1,
                            Team2 = team2
                        },
                        VenueAddress = result.IsDBNull(2) ? null : result.GetString(2),
                        VenueNumber = result.IsDBNull(3) ? null : result.GetString(3),
                        GameTime = new TimeInfo
                        {
                            Period = result.GetInt16(5),
                            HalfLength = settings.HalfLength,
                            HalftimeLength = settings.HalftimeLength,
                            FirstHalfLength = settings.FirstHalfLength,
                            SecondHalfLength = settings.SecondHalfLength,
                            ActualHalftimeLength = settings.ActualHalftimeLength,
                            StartTime = result.GetInt64(0),
                            EndTime = result.IsDBNull(1) ? null : result.GetInt64(1)
                        },
                        Referees = result.IsDBNull(10) ? null : JsonConvert.DeserializeObject<List<Referee>>(result.GetString(10)),
                        Match = JsonConvert.DeserializeObject<MatchData>(result.GetString(8))
                    };
                }

                _databaseService.Disconnect();

                if (response != null)
                    return new Response
                    {
                        StatusCode = 200,
                        Data = response
                    };
            //}

            return new Response
            {
                StatusCode = 401,
                Data = null
            };
        }
        public Response GetHostRatingByOwner(string owner)
        {
            _databaseService.Initialize();
            MySqlCommand cmd = new MySqlCommand($"SELECT ROUND(SUM(Ratings.Rating)/COUNT(Ratings.Rating), 2) FROM Ratings JOIN Users ON (Ratings.HostUserID = Users.UserID) WHERE Users.AuthID = @OwnerID", _databaseService.Connection);
            cmd.Parameters.AddWithValue("@OwnerID", owner);
            var result = cmd.ExecuteReader();

            HostRatingResponse response = new HostRatingResponse();

            while (result.Read())
            {
                response.Rating = (result.IsDBNull(0)) ? null : result.GetFloat(0);
            }

            _databaseService.Disconnect();

            return new Response
            {
                StatusCode = 200,
                Data = response
            };
        }
        public async Task<bool> UpdateMatchById(string owner, string id, UpdateMatchRequest request)
        {
            //if (VerifyOwner(owner, id) || _adminService.VerifyUserIsAdmin(owner))
            //{
            var pusher = new Pusher("1689539", "68ee48eaf887ec8b4684", "0c532358ae5c87877612", new PusherOptions
                {
                    Cluster = "us3",
                    Encrypted = true
                });

                if (request.State != null)
                {
                    _databaseService.Initialize();
                    MySqlCommand cmd = new MySqlCommand("UPDATE Matches SET State=@State WHERE MatchID=@MatchID", _databaseService.Connection);
                    cmd.Parameters.AddWithValue("@State", request.State);
                    cmd.Parameters.AddWithValue("@MatchID", id);
                    cmd.ExecuteNonQuery();
                    _databaseService.Disconnect();
                }

                if (request.Team1ID != null)
                {
                    _databaseService.Initialize();
                    MySqlCommand cmd = new MySqlCommand("UPDATE Matches SET Team1ID=@TeamID WHERE MatchID=@MatchID", _databaseService.Connection);
                    cmd.Parameters.AddWithValue("@TeamID", request.Team1ID);
                    cmd.Parameters.AddWithValue("@MatchID", id);
                    cmd.ExecuteNonQuery();
                    _databaseService.Disconnect();
                }

                if (request.Team2ID != null)
                {
                    _databaseService.Initialize();
                    MySqlCommand cmd = new MySqlCommand("UPDATE Matches SET Team2ID=@TeamID WHERE MatchID=@MatchID", _databaseService.Connection);
                    cmd.Parameters.AddWithValue("@TeamID", request.Team2ID);
                    cmd.Parameters.AddWithValue("@MatchID", id);
                    cmd.ExecuteNonQuery();
                    _databaseService.Disconnect();
                }

                if (request.StartTime != null)
                {
                    _databaseService.Initialize();
                    MySqlCommand cmd = new MySqlCommand("UPDATE Matches SET StartTime=@StartTime WHERE MatchID=@MatchID", _databaseService.Connection);
                    cmd.Parameters.AddWithValue("@StartTime", request.StartTime);
                    cmd.Parameters.AddWithValue("@MatchID", id);
                    cmd.ExecuteNonQuery();
                    _databaseService.Disconnect();

                    await pusher.TriggerAsync(id, "GameStart", new { StartTime = request.StartTime });
                }

                if (request.EndTime != null)
                {
                    _databaseService.Initialize();
                    MySqlCommand cmd = new MySqlCommand("UPDATE Matches SET EndTime=@EndTime WHERE MatchID=@MatchID", _databaseService.Connection);
                    cmd.Parameters.AddWithValue("@EndTime", request.EndTime);
                    cmd.Parameters.AddWithValue("@MatchID", id);
                    cmd.ExecuteNonQuery();
                    _databaseService.Disconnect();

                    if (request.Period != 3)
                        await pusher.TriggerAsync(id, "GameEnd", new { EndTime = request.EndTime });
                }

                if (request.Period != null)
                {
                    _databaseService.Initialize();
                    MySqlCommand cmd = new MySqlCommand("UPDATE Matches SET Period=@Period WHERE MatchID=@MatchID", _databaseService.Connection);
                    cmd.Parameters.AddWithValue("@Period", request.Period);
                    cmd.Parameters.AddWithValue("@MatchID", id);
                    cmd.ExecuteNonQuery();
                    _databaseService.Disconnect();

                    await pusher.TriggerAsync(id, "PeriodChange", new { Period = request.Period });

                    if (request.Period == 3)
                    {
                        MatchData match = ((MatchResponse)GetMatchById(id).Data).Match;
                        PlayMakerAPI.Models.Response.Event newEvent = new PlayMakerAPI.Models.Response.Event
                        {
                            EventID = Guid.NewGuid().ToString().Substring(0, 8),
                            EventType = "PENALTY_KICK_PERIOD_INITIATED",
                            GameTime = null,
                            Team = null,
                            Second = request.Second,
                            Score = null,
                            PlayerId = null
                        };

                        if (match.Events == null)
                            match.Events = new List<PlayMakerAPI.Models.Response.Event>();

                        match.Events.Add(newEvent);
                        var updated = JsonConvert.SerializeObject(match).Replace("'", @"''");

                        _databaseService.Initialize();
                        MySqlCommand eventCmd = new MySqlCommand($"UPDATE Matches SET Data='{updated}' WHERE MatchID=@MatchID", _databaseService.Connection);
                        eventCmd.Parameters.AddWithValue("@MatchID", id);
                        eventCmd.ExecuteNonQuery();
                        _databaseService.Disconnect();

                        await pusher.TriggerAsync(id, "Event", new { Event = newEvent });
                    }
                }

                if (request.HalftimeLength != null)
                {
                    _databaseService.Initialize();
                    MySqlCommand cmd = new MySqlCommand("UPDATE Matches SET Settings=JSON_SET(Settings, '$.HalftimeLength', @HalftimeLength) WHERE MatchID=@MatchID", _databaseService.Connection);
                    cmd.Parameters.AddWithValue("@HalftimeLength", request.HalftimeLength);
                    cmd.Parameters.AddWithValue("@MatchID", id);
                    cmd.ExecuteNonQuery();
                    _databaseService.Disconnect();

                    await pusher.TriggerAsync(id, "HalftimeLength", new { HalftimeLength = request.HalftimeLength });
                }

                if (request.HalfLength != null)
                {
                    _databaseService.Initialize();
                    MySqlCommand cmd = new MySqlCommand("UPDATE Matches SET Settings=JSON_SET(Settings, '$.HalfLength', @HalfLength) WHERE MatchID=@MatchID", _databaseService.Connection);
                    cmd.Parameters.AddWithValue("@HalfLength", request.HalfLength);
                    cmd.Parameters.AddWithValue("@MatchID", id);
                    cmd.ExecuteNonQuery();
                    _databaseService.Disconnect();

                    await pusher.TriggerAsync(id, "HalfLength", new { HalfLength = request.HalfLength });
                }

                if (request.FirstHalfLength != null)
                {
                    _databaseService.Initialize();
                    MySqlCommand cmd = new MySqlCommand("UPDATE Matches SET Settings=JSON_SET(Settings, '$.FirstHalfLength', @FirstHalfLength) WHERE MatchID=@MatchID", _databaseService.Connection);
                    cmd.Parameters.AddWithValue("@FirstHalfLength", request.FirstHalfLength);
                    cmd.Parameters.AddWithValue("@MatchID", id);
                    cmd.ExecuteNonQuery();
                    _databaseService.Disconnect();

                    await pusher.TriggerAsync(id, "FirstHalfLength", new { FirstHalfLength = request.FirstHalfLength });
                }

                if (request.SecondHalfLength != null)
                {
                    _databaseService.Initialize();
                    MySqlCommand cmd = new MySqlCommand("UPDATE Matches SET Settings=JSON_SET(Settings, '$.SecondHalfLength', @SecondHalfLength) WHERE MatchID=@MatchID", _databaseService.Connection);
                    cmd.Parameters.AddWithValue("@SecondHalfLength", request.SecondHalfLength);
                    cmd.Parameters.AddWithValue("@MatchID", id);
                    cmd.ExecuteNonQuery();
                    _databaseService.Disconnect();

                    await pusher.TriggerAsync(id, "SecondHalfLength", new { SecondHalfLength = request.SecondHalfLength });
                }

                if (request.ActualHalftimeLength != null)
                {
                    _databaseService.Initialize();
                    MySqlCommand cmd = new MySqlCommand("UPDATE Matches SET Settings=JSON_SET(Settings, '$.ActualHalftimeLength', @ActualHalftimeLength) WHERE MatchID=@MatchID", _databaseService.Connection);
                    cmd.Parameters.AddWithValue("@ActualHalftimeLength", request.ActualHalftimeLength);
                    cmd.Parameters.AddWithValue("@MatchID", id);
                    cmd.ExecuteNonQuery();
                    _databaseService.Disconnect();

                    await pusher.TriggerAsync(id, "ActualHalftimeLength", new { ActualHalftimeLength = request.ActualHalftimeLength });
                }

                if (request.VenueName != null)
                {
                    _databaseService.Initialize();
                    MySqlCommand cmd = new MySqlCommand("UPDATE Matches SET VenueName=@VenueName WHERE MatchID=@MatchID", _databaseService.Connection);
                    cmd.Parameters.AddWithValue("@VenueName", request.VenueName);
                    cmd.Parameters.AddWithValue("@MatchID", id);
                    cmd.ExecuteNonQuery();
                    _databaseService.Disconnect();
                }

                if (request.VenueAddress != null)
                {
                    _databaseService.Initialize();
                    MySqlCommand cmd = new MySqlCommand("UPDATE Matches SET VenueAddress=@VenueAddress WHERE MatchID=@MatchID", _databaseService.Connection);
                    cmd.Parameters.AddWithValue("@VenueAddress", request.VenueAddress);
                    cmd.Parameters.AddWithValue("@MatchID", id);
                    cmd.ExecuteNonQuery();
                    _databaseService.Disconnect();
                }

                if (request.VenueNumber != null)
                {
                    _databaseService.Initialize();
                    MySqlCommand cmd = new MySqlCommand("UPDATE Matches SET VenueNumber=@VenueNumber WHERE MatchID=@MatchID", _databaseService.Connection);
                    cmd.Parameters.AddWithValue("@VenueNumber", request.VenueNumber);
                    cmd.Parameters.AddWithValue("@MatchID", id);
                    cmd.ExecuteNonQuery();
                    _databaseService.Disconnect();
                }

                if(request.State != null || request.Period != null || request.StartTime != null)
                {
                    await pusher.TriggerAsync("global-matches", "StateUpdate", new { 
                        MatchID = id,
                        State = request.State,
                        Period = request.Period,
                        StartTime = request.StartTime
                    });
                }

                return true;
            //}

            //return false;
        }
        public async Task<bool> ProcessAttendance(string owner, string id, AttendanceRequest request)
        {
            //if (VerifyOwner(owner, id))
            //{
                var pusher = new Pusher("1689539", "68ee48eaf887ec8b4684", "0c532358ae5c87877612", new PusherOptions
                {
                    Cluster = "us3",
                    Encrypted = true
                });

                MatchData? match = ((MatchResponse)GetMatchById(id).Data).Match;
                int? state = null;

                if (request.Team == 0)
                {
                    match.Team1 = new TeamAttendance
                    {
                        Score = 0,
                        OverallScore = 0,
                        PenaltyKickScore = 0,
                        Attendance = request.Attendance
                    };

                    state = 1;
                }
                else if (request.Team == 1)
                {

                    match.Team2 = new TeamAttendance
                    {
                        Score = 0,
                        OverallScore = 0,
                        PenaltyKickScore = 0,
                        Attendance = request.Attendance
                    };

                    state = 2;
                }

                var updated = JsonConvert.SerializeObject(match);

                _databaseService.Initialize();
                MySqlCommand cmd = new MySqlCommand($"UPDATE Matches SET State='{state}', Data='{updated}' WHERE MatchID=@MatchID", _databaseService.Connection);
                cmd.Parameters.AddWithValue("@MatchID", id);

                var result = cmd.ExecuteNonQuery();
                _databaseService.Disconnect();

                if (result != 0)
                {
                    await pusher.TriggerAsync(id, "AttendanceCheckIn", new
                    {
                        Team = request.Team,
                        Event = new TeamAttendance
                        {
                            Score = 0,
                            Attendance = request.Attendance
                        }
                    });
                    return true;
                }
            //}

            return false;
        }
        public async Task<Response> AddMatchEvent(string owner, string id, NewEventRequest request)
        {
            //if (VerifyOwner(owner, id))
            //{
                var pusher = new Pusher("1689539", "68ee48eaf887ec8b4684", "0c532358ae5c87877612", new PusherOptions
                {
                    Cluster = "us3",
                    Encrypted = true
                });

                MatchData? match = ((MatchResponse)GetMatchById(id).Data).Match;

                if (match.Events == null)
                    match.Events = new List<PlayMakerAPI.Models.Response.Event>();

                var newEvent = new PlayMakerAPI.Models.Response.Event
                {
                    EventID = Guid.NewGuid().ToString().Substring(0, 8),
                    EventType = request.EventType,
                    GameTime = request.GameTime,
                    Second = request.Second,
                    Team = request.Team,
                    PlayerId = request.PlayerID
                };

                if (request.EventType == "GOAL" || request.EventType == "PENALTY_KICK" || request.EventType == "OWN_GOAL" || request.EventType == "HEADER_GOAL")
                {
                    ScoreObj score = null;
                    if (request.Team == 0)
                    {
                        if (request.EventType == "OWN_GOAL" && request.GameTime != "PK")
                        {
                            match.Team2.Score += 1;
                            match.Team2.OverallScore += 1;
                        }
                        else if (request.EventType != "OWN_GOAL" && request.GameTime != "PK")
                        {
                            match.Team1.Score += 1;
                            match.Team1.OverallScore += 1;
                        }
                        else if (request.EventType == "OWN_GOAL" && request.GameTime == "PK")
                        {
                            match.Team2.PenaltyKickScore += 1;
                            match.Team2.OverallScore += 1;
                        }
                        else if (request.EventType != "OWN_GOAL" && request.GameTime == "PK")
                        {
                            match.Team1.PenaltyKickScore += 1;
                            match.Team1.OverallScore += 1;
                        }
                    }
                    else if (request.Team == 1)
                    {
                        if (request.EventType == "OWN_GOAL" && request.GameTime != "PK")
                        {
                            match.Team1.Score += 1;
                            match.Team1.OverallScore += 1;
                        }
                        else if (request.EventType != "OWN_GOAL" && request.GameTime != "PK")
                        {
                            match.Team2.Score += 1;
                            match.Team2.OverallScore += 1;
                        }
                        else if (request.EventType == "OWN_GOAL" && request.GameTime == "PK")
                        {
                            match.Team1.PenaltyKickScore += 1;
                            match.Team1.OverallScore += 1;
                        }
                        else if (request.EventType != "OWN_GOAL" && request.GameTime == "PK")
                        {
                            match.Team2.PenaltyKickScore += 1;
                            match.Team2.OverallScore += 1;
                        }
                    }

                    score = new ScoreObj
                    {
                        Team1 = match.Team1.Score,
                        Team2 = match.Team2.Score,
                        Team1Overall = match.Team1.OverallScore,
                        Team2Overall = match.Team2.OverallScore,
                        Team1PenaltyKicks = match.Team1.PenaltyKickScore,
                        Team2PenaltyKicks = match.Team2.PenaltyKickScore
                    };

                    newEvent.Score = score;
                }

                match.Events.Add(newEvent);

                var updated = JsonConvert.SerializeObject(match).Replace("'", @"''");

                _databaseService.Initialize();
                MySqlCommand cmd = new MySqlCommand($"UPDATE Matches SET Data='{updated}' WHERE MatchID=@MatchID", _databaseService.Connection);
                cmd.Parameters.AddWithValue("@MatchID", id);

                var result = cmd.ExecuteNonQuery();
                _databaseService.Disconnect();

                if (result != 0)
                {
                    UpdateStatistics("add", request.PlayerID, request.EventType);
                    await pusher.TriggerAsync(id, "Event", new { Event = newEvent });
                    await pusher.TriggerAsync("global-matches", "EventUpdate", new
                    {
                        MatchID = id,
                        Team1 = match.Team1,
                        Team2 = match.Team2
                    });
                    return new Response
                    {
                        StatusCode = 201,
                        Data = new 
                        {
                            EventID = newEvent.EventID
                        }
                    };
                }
            //}

            return new Response
            {
                StatusCode = 401,
                Data = null
            };
        }
        public async Task<bool> UpdateMatchEvent(string owner, string id, string eventId, NewEventRequest request)
        {
            //if(VerifyOwner(owner, id))
            //{
                var pusher = new Pusher("1689539", "68ee48eaf887ec8b4684", "0c532358ae5c87877612", new PusherOptions
                {
                    Cluster = "us3",
                    Encrypted = true
                });

                MatchData? match = ((MatchResponse)GetMatchById(id).Data).Match;

                var newEvent = new PlayMakerAPI.Models.Response.Event
                {
                    EventID = eventId,
                    EventType = request.EventType,
                    GameTime = request.GameTime,
                    Second = request.Second,
                    Team = request.Team,
                    PlayerId = request.PlayerID
                };

                if(match.Events != null)
                {
                    // Locate existing event in match data
                    var existingIdx = match.Events.FindIndex(evnt => evnt.EventID == eventId);

                    if(existingIdx != -1)
                    {
                        // Remove statistic point from stats table
                        UpdateStatistics("remove", request.PlayerID, match.Events[existingIdx].EventType);

                        // Remove event from event list
                        match.Events.RemoveAt(existingIdx);

                        // Append updated event
                        match.Events.Add(newEvent);

                        // Sort event array by game second
                        match.Events = match.Events.OrderBy(evnt => evnt.Second).ToList();

                        // Re-evaluate scores at each individual event
                        match.Team1.Score = 0;
                        match.Team2.Score = 0;
                        match.Team1.OverallScore = 0;
                        match.Team2.OverallScore = 0;
                        match.Team1.PenaltyKickScore = 0;
                        match.Team2.PenaltyKickScore = 0;

                        match.Events.ForEach(evnt =>
                        {
                            if (evnt.EventType == "GOAL" || evnt.EventType == "PENALTY_KICK" || evnt.EventType == "OWN_GOAL" || evnt.EventType == "HEADER_GOAL")
                            {
                                ScoreObj score = null;
                                if (evnt.Team == 0)
                                {
                                    if (evnt.EventType == "OWN_GOAL" && evnt.GameTime != "PK")
                                    {
                                        match.Team2.Score += 1;
                                        match.Team2.OverallScore += 1;
                                    }
                                    else if (evnt.EventType != "OWN_GOAL" && evnt.GameTime != "PK")
                                    {
                                        match.Team1.Score += 1;
                                        match.Team1.OverallScore += 1;
                                    }
                                    else if (evnt.EventType == "OWN_GOAL" && evnt.GameTime == "PK")
                                    {
                                        match.Team2.PenaltyKickScore += 1;
                                        match.Team2.OverallScore += 1;
                                    }
                                    else if (evnt.EventType != "OWN_GOAL" && evnt.GameTime == "PK")
                                    {
                                        match.Team1.PenaltyKickScore += 1;
                                        match.Team1.OverallScore += 1;
                                    }
                                }
                                else if (evnt.Team == 1)
                                {
                                    if (evnt.EventType == "OWN_GOAL" && evnt.GameTime != "PK")
                                    {
                                        match.Team1.Score += 1;
                                        match.Team1.OverallScore += 1;
                                    }
                                    else if (evnt.EventType != "OWN_GOAL" && evnt.GameTime != "PK")
                                    {
                                        match.Team2.Score += 1;
                                        match.Team2.OverallScore += 1;
                                    }
                                    else if (evnt.EventType == "OWN_GOAL" && evnt.GameTime == "PK")
                                    {
                                        match.Team1.PenaltyKickScore += 1;
                                        match.Team1.OverallScore += 1;
                                    }
                                    else if (evnt.EventType != "OWN_GOAL" && evnt.GameTime == "PK")
                                    {
                                        match.Team2.PenaltyKickScore += 1;
                                        match.Team2.OverallScore += 1;
                                    }
                                }

                                score = new ScoreObj
                                {
                                    Team1 = match.Team1.Score,
                                    Team2 = match.Team2.Score,
                                    Team1Overall = match.Team1.OverallScore,
                                    Team2Overall = match.Team2.OverallScore,
                                    Team1PenaltyKicks = match.Team1.PenaltyKickScore,
                                    Team2PenaltyKicks = match.Team2.PenaltyKickScore
                                };

                                evnt.Score = score;
                            }
                        });

                        var updated = JsonConvert.SerializeObject(match).Replace("'", @"''");

                        _databaseService.Initialize();
                        MySqlCommand cmd = new MySqlCommand($"UPDATE Matches SET Data='{updated}' WHERE MatchID=@MatchID", _databaseService.Connection);
                        cmd.Parameters.AddWithValue("@MatchID", id);

                        var result = cmd.ExecuteNonQuery();
                        _databaseService.Disconnect();

                        if(result > 0)
                        {
                            UpdateStatistics("add", request.PlayerID, request.EventType);
                            await pusher.TriggerAsync(id, "UpdateEvent", new { Event = match });
                            await pusher.TriggerAsync("global-matches", "EventUpdate", new
                            {
                                MatchID = id,
                                Team1 = match.Team1,
                                Team2 = match.Team2
                            });
                            return true;
                        }
                    }
                }
            //}
            return false;
        }
        public async Task<bool> DeleteMatchEvent(string owner, string id, string eventId)
        {
            //if(VerifyOwner(owner, id))
            //{
                var pusher = new Pusher("1689539", "68ee48eaf887ec8b4684", "0c532358ae5c87877612", new PusherOptions
                {
                    Cluster = "us3",
                    Encrypted = true
                });

                MatchData? match = ((MatchResponse)GetMatchById(id).Data).Match;

                if (match.Events != null)
                {
                    // Locate existing event in match data
                    var existingIdx = match.Events.FindIndex(evnt => evnt.EventID == eventId);

                    if (existingIdx != -1)
                    {
                        // Remove statistic point from stats table
                        UpdateStatistics("remove", match.Events[existingIdx].PlayerId, match.Events[existingIdx].EventType);

                        // Remove event from event list
                        match.Events.RemoveAt(existingIdx);

                        // Sort event array by game second
                        match.Events = match.Events.OrderBy(evnt => evnt.Second).ToList();

                        // Re-evaluate scores at each individual event
                        match.Team1.Score = 0;
                        match.Team2.Score = 0;
                        match.Team1.OverallScore = 0;
                        match.Team2.OverallScore = 0;
                        match.Team1.PenaltyKickScore = 0;
                        match.Team2.PenaltyKickScore = 0;

                        match.Events.ForEach(evnt =>
                        {
                            if (evnt.EventType == "GOAL" || evnt.EventType == "PENALTY_KICK" || evnt.EventType == "OWN_GOAL" || evnt.EventType == "HEADER_GOAL")
                            {
                                ScoreObj score = null;
                                if (evnt.Team == 0)
                                {
                                    if (evnt.EventType == "OWN_GOAL" && evnt.GameTime != "PK")
                                    {
                                        match.Team2.Score += 1;
                                        match.Team2.OverallScore += 1;
                                    }
                                    else if (evnt.EventType != "OWN_GOAL" && evnt.GameTime != "PK")
                                    {
                                        match.Team1.Score += 1;
                                        match.Team1.OverallScore += 1;
                                    }
                                    else if (evnt.EventType == "OWN_GOAL" && evnt.GameTime == "PK")
                                    {
                                        match.Team2.PenaltyKickScore += 1;
                                        match.Team2.OverallScore += 1;
                                    }
                                    else if (evnt.EventType != "OWN_GOAL" && evnt.GameTime == "PK")
                                    {
                                        match.Team1.PenaltyKickScore += 1;
                                        match.Team1.OverallScore += 1;
                                    }
                                }
                                else if (evnt.Team == 1)
                                {
                                    if (evnt.EventType == "OWN_GOAL" && evnt.GameTime != "PK")
                                    {
                                        match.Team1.Score += 1;
                                        match.Team1.OverallScore += 1;
                                    }
                                    else if (evnt.EventType != "OWN_GOAL" && evnt.GameTime != "PK")
                                    {
                                        match.Team2.Score += 1;
                                        match.Team2.OverallScore += 1;
                                    }
                                    else if (evnt.EventType == "OWN_GOAL" && evnt.GameTime == "PK")
                                    {
                                        match.Team1.PenaltyKickScore += 1;
                                        match.Team1.OverallScore += 1;
                                    }
                                    else if (evnt.EventType != "OWN_GOAL" && evnt.GameTime == "PK")
                                    {
                                        match.Team2.PenaltyKickScore += 1;
                                        match.Team2.OverallScore += 1;
                                    }
                                }

                                score = new ScoreObj
                                {
                                    Team1 = match.Team1.Score,
                                    Team2 = match.Team2.Score,
                                    Team1Overall = match.Team1.OverallScore,
                                    Team2Overall = match.Team2.OverallScore,
                                    Team1PenaltyKicks = match.Team1.PenaltyKickScore,
                                    Team2PenaltyKicks = match.Team2.PenaltyKickScore
                                };

                                evnt.Score = score;
                            }
                        });

                        var updated = JsonConvert.SerializeObject(match).Replace("'", @"''");

                        _databaseService.Initialize();
                        MySqlCommand cmd = new MySqlCommand($"UPDATE Matches SET Data='{updated}' WHERE MatchID=@MatchID", _databaseService.Connection);
                        cmd.Parameters.AddWithValue("@MatchID", id);

                        var result = cmd.ExecuteNonQuery();
                        _databaseService.Disconnect();

                        if (result > 0)
                        {
                            await pusher.TriggerAsync(id, "DeleteEvent", new { Event = match });
                            await pusher.TriggerAsync("global-matches", "EventUpdate", new
                            {
                                MatchID = id,
                                Team1 = match.Team1,
                                Team2 = match.Team2
                            });
                            return true;
                        }
                    }
                }
            //}
            return false;
        } 
        public async Task<bool> ResetMatchById(string owner, string id, ResetMatchRequest request)
        {
            //if (VerifyOwner(owner, id))
            //{
                _databaseService.Initialize();
                MySqlCommand cmd = new MySqlCommand("UPDATE Matches SET State=0, Period=0, Data='{}', StartTime=@StartTime WHERE MatchID=@MatchID", _databaseService.Connection);
                cmd.Parameters.AddWithValue("@StartTime", request.StartTime);
                cmd.Parameters.AddWithValue("@MatchID", id);
                cmd.ExecuteNonQuery();
                _databaseService.Disconnect();

                var pusher = new Pusher("1689539", "68ee48eaf887ec8b4684", "0c532358ae5c87877612", new PusherOptions
                {
                    Cluster = "us3",
                    Encrypted = true
                });

                await pusher.TriggerAsync(id, "ResetGame", new { StartTime = request.StartTime });

                return true;
            //}

            //return false;
        }
        public bool DeleteMatchById(string owner, string id)
        {
            if(VerifyOwner(owner, id) || _adminService.VerifyUserIsAdmin(owner))
            {
                _databaseService.Initialize();
                MySqlCommand deleteCmd = new MySqlCommand("DELETE FROM Matches WHERE MatchID=@MatchID", _databaseService.Connection);
                deleteCmd.Parameters.AddWithValue("@MatchID", id);
                int deleteCount = deleteCmd.ExecuteNonQuery();
                _databaseService.Disconnect();

                if (deleteCount > 0)
                    return true;
            }

            return false;
        }
        private string UnixToDateString(long timestamp, double tzOffset)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddMilliseconds(timestamp).AddMinutes(tzOffset);
            var result = dateTime.ToString("MMMM d");

            switch (int.Parse(dateTime.ToString("dd")))
            {
                case 1:
                case 21:
                case 31:
                    result += "st";
                    break;
                case 2:
                case 22:
                    result += "nd";
                    break;
                case 3:
                case 23:
                    result += "rd";
                    break;
                default:
                    result += "th";
                    break;
            }

            result += ", " + dateTime.ToString("yyyy");
            return result;
        }
        private bool VerifyOwner(string owner, string id)
        {
            int count = 0;

            _databaseService.Initialize();
            MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM Matches WHERE MatchID=@MatchID AND (Owner=@OwnerID)", _databaseService.Connection);
            cmd.Parameters.AddWithValue("@OwnerID", owner);
            cmd.Parameters.AddWithValue("@MatchID", id);
            var result = cmd.ExecuteReader();

            while (result.Read())
            {
                count = (result.IsDBNull(0)) ? 0 : result.GetInt16(0);
            }

            _databaseService.Disconnect();

            return count > 0;
        }
        private void UpdateStatistics(string action, int? playerId, string eventType)
        {
            string colName = "";

            switch (eventType)
            {
                case "GOAL":
                    colName = "Goals";
                    break;
                case "OWN_GOAL":
                    colName = "OwnGoals";
                    break;
                case "RED_CARD":
                    colName = "RedCards";
                    break;
                case "YELLOW_CARD":
                    colName = "YellowCards";
                    break;
                case "EJECTION":
                    colName = "Ejections";
                    break;
                case "PENALTY_KICK":
                    colName = "PenaltyKicks";
                    break;
                case "FOUL":
                    colName = "Fouls";
                    break;
            }

            if (colName != "" && action == "add")
            {
                var searchResult = _databaseService.ExecuteQuery($"SELECT * FROM Statistics WHERE PlayerID={playerId}");

                int resultCount = 0;
                while (searchResult.Read())
                {
                    resultCount += 1;
                }

                _databaseService.Disconnect();

                if (resultCount == 0)
                {
                    _databaseService.ExecuteNonQuery($"INSERT INTO Statistics (PlayerID, Goals, OwnGoals, RedCards, YellowCards, Ejections, PenaltyKicks, Fouls) VALUES({playerId}, 0, 0, 0, 0, 0, 0, 0)");
                    _databaseService.Disconnect();
                }

                _databaseService.ExecuteNonQuery($"UPDATE Statistics SET {colName} = {colName} + 1 WHERE PlayerID={playerId}");
                _databaseService.Disconnect();


                if (eventType == "PENALTY_KICK")
                {
                    _databaseService.ExecuteNonQuery($"UPDATE Statistics SET Goals = Goals + 1 WHERE PlayerID={playerId}");
                    _databaseService.Disconnect();
                }

            }
            else if (colName != "" && action == "remove")
            {
                _databaseService.ExecuteNonQuery($"UPDATE Statistics SET {colName} = {colName} - 1 WHERE PlayerID={playerId}");
                _databaseService.Disconnect();

                if (eventType == "PENALTY_KICK")
                {
                    _databaseService.ExecuteNonQuery($"UPDATE Statistics SET Goals = Goals - 1 WHERE PlayerID={playerId}");
                    _databaseService.Disconnect();
                }
            }
        }
    }
}
