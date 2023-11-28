using AlvivaAPI.Models.Request.TGS;
using AlvivaAPI.Models.Response.TGS;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using PlayMakerAPI.Models.Response;
using System.Net;

namespace PlayMakerAPI.Services
{
    public class AdminService
    {
        private DatabaseService _databaseService = new DatabaseService();
        private TeamService _teamService = new TeamService();
        public async Task<Response> CopyTeamByID(string user, string? orgId, int? eventId, string? teamId, string? clubId)
        {
            if (VerifyUserIsAdmin(user))
            {
                TGS_TeamInfoResponse teamData = await GetTeamInfo(orgId, eventId, teamId, clubId);

                if (teamData == null || teamData.Data == null || teamData.TeamInfo == null)
                    return new Response
                    {
                        StatusCode = 500,
                        Data = null
                    };

                // Cache IDs
                int? teamClubId = null;
                int? teamCoachId = null;
                List<int?> asstCoachIds = new List<int?>();
                int? teamManagerId = null;
                int? division = (teamData.TeamInfo.DivisionName.Contains('/')) ? int.Parse(teamData.TeamInfo.DivisionName.Substring(1, 2) + teamData.TeamInfo.DivisionName.Substring(4, 1) + teamData.TeamInfo.DivisionName.Substring(9, 1)) : int.Parse(teamData.TeamInfo.DivisionName.Substring(1));
                int? tid = null;

                string? coachLastName = null;

                // Club
                if (teamData.TeamInfo.ClubID != null && teamData.TeamInfo.ClubName != null)
                {
                    var clubSearchResult = _databaseService.ExecuteQuery($"SELECT ClubID FROM Clubs WHERE ClubName = '{teamData.TeamInfo.ClubName.Trim().Replace("'", @"\'").Trim()}' OR ExternalID='{teamData.TeamInfo.ClubID}';");

                    while (clubSearchResult.Read())
                    {
                        teamClubId = (clubSearchResult.IsDBNull(0)) ? null : clubSearchResult.GetInt16(0);
                    }

                    _databaseService.Disconnect();

                    if (teamClubId == null)
                    {
                        var insertClubResult = _databaseService.ExecuteQuery($"INSERT INTO Clubs VALUES (null, '{teamData.TeamInfo.ClubName.Trim().Replace("'", @"\'").Trim()}', '{teamData.TeamInfo.ClubName.Trim().ToUpper().Replace("'", @"\'").Trim()}', '{teamData.TeamInfo.ClubName.Trim().ToUpper().Replace("'", @"\'").Trim()}', '{teamData.TeamInfo.ClubLogo}', '{teamData.TeamInfo.ClubID}'); SELECT LAST_INSERT_ID();");

                        while (insertClubResult.Read())
                        {
                            teamClubId = (insertClubResult.IsDBNull(0)) ? null : insertClubResult.GetInt16(0);
                        }

                        _databaseService.Disconnect();
                    }

                    _databaseService.ExecuteNonQuery($"UPDATE Clubs SET ExternalID={teamData.TeamInfo.ClubID} WHERE ClubID={teamClubId}");
                    _databaseService.Disconnect();
                }

                // Coach / Assistant Coaches / Manager
                if (teamData.Data.StaffList != null)
                {
                    foreach (TGS_TeamStaff staff in teamData.Data.StaffList)
                    {

                        if ((staff.Role == "Team Admin" && teamManagerId == null) || (staff.Role == "Head Coach" && teamCoachId == null) || staff.Role == "Assistant Coach")
                        {
                            string[] parts = staff.FullName.Split(", ");

                            int? existingUser = null;
                            var userSearchResult = _databaseService.ExecuteQuery($"SELECT UserID FROM Users WHERE (FirstName = '{parts[1].Replace("'", @"\'").Trim()}' AND LastName = '{parts[0].Replace("'", @"\'").Trim()}') OR ExternalID={staff.UserID};");

                            while (userSearchResult.Read())
                            {
                                existingUser = (userSearchResult.IsDBNull(0)) ? null : userSearchResult.GetInt16(0);
                            }

                            _databaseService.Disconnect();

                            if (existingUser != null)
                            {
                                switch (staff.Role)
                                {
                                    case "Team Admin":
                                        // Manager
                                        teamManagerId = existingUser;
                                        break;
                                    case "Assistant Coach":
                                        // Assistant Coach
                                        asstCoachIds.Add(existingUser);
                                        break;
                                    case "Head Coach":
                                        // Coach
                                        teamCoachId = existingUser;
                                        coachLastName = parts[0];
                                        break;
                                }
                            }
                            else
                            {

                                var insertUserResult = _databaseService.ExecuteQuery($"INSERT INTO Users VALUES (null, null, '{parts[1].Replace("'", @"\'").Trim()}', '{parts[0].Replace("'", @"\'").Trim()}', '{staff.Avatar.Replace("'", @"\'")}', null, {staff.UserID}); SELECT LAST_INSERT_ID();");

                                while (insertUserResult.Read())
                                {
                                    int? newId = (insertUserResult.IsDBNull(0)) ? null : insertUserResult.GetInt16(0);
                                    switch (staff.Role)
                                    {
                                        case "Team Admin":
                                            // Manager
                                            teamManagerId = newId;
                                            break;
                                        case "Assistant Coach":
                                            // Assistant Coach
                                            asstCoachIds.Add(newId);
                                            break;
                                        case "Head Coach":
                                            // Coach
                                            teamCoachId = newId;
                                            coachLastName = parts[0];
                                            break;
                                    }
                                }

                                _databaseService.Disconnect();
                            }

                        }

                    }
                }

                // Team
                if (teamClubId != null && coachLastName != null && division != null)
                {
                    var teamSearchResult = _databaseService.ExecuteQuery($"SELECT TeamID FROM Teams WHERE (ClubId={((teamClubId == null) ? "null" : teamClubId)} AND Division={division}) OR ExternalID={teamData.TeamInfo.TeamID}");

                    while (teamSearchResult.Read())
                    {
                        tid = (teamSearchResult.IsDBNull(0)) ? null : teamSearchResult.GetInt16(0);
                    }

                    _databaseService.Disconnect();

                    if (tid == null)
                    {
                        var insertTeamResult = _databaseService.ExecuteQuery($"INSERT INTO Teams VALUES (null, {((teamClubId == null) ? "null" : teamClubId)}, 1, '{user}', '{((teamData.TeamInfo.TeamGender == "f") ? "G" : "B")}', {division}, {((teamManagerId == null) ? "null" : teamManagerId)}, {((teamCoachId == null) ? "null" : teamCoachId)}, '{JsonConvert.SerializeObject(asstCoachIds)}', '{division.ToString().Trim().Substring(2)} {teamData.TeamInfo.ClubName.Trim().ToUpper().Replace("'", @"\'").Trim()} {coachLastName.Trim().ToUpper().Replace("'", @"\'").Trim()}', {teamData.TeamInfo.TeamID}); SELECT LAST_INSERT_ID();");

                        while (insertTeamResult.Read())
                        {
                            tid = (insertTeamResult.IsDBNull(0)) ? null : insertTeamResult.GetInt16(0);
                        }

                        _databaseService.Disconnect();
                    }

                    _databaseService.ExecuteNonQuery($"UPDATE Teams SET ExternalID={teamData.TeamInfo.TeamID} WHERE TeamID={tid}");
                    _databaseService.Disconnect();

                    // Add logic to force update coach, asst coach, etc. to latest even if matched to existing
                    // Change button to say update instead of create if existing already
                }

                // Players
                if (teamData.Data.PlayerList != null)
                {
                    foreach (TGS_TeamPlayer player in teamData.Data.PlayerList)
                    {
                        int? existingPlayer = null;
                        var playerSearchResult = _databaseService.ExecuteQuery($"SELECT PlayerID FROM Players WHERE (FirstName = '{player.FirstName.Replace("'", @"\'").Trim()}' AND LastName = '{player.LastName.Replace("'", @"\'").Trim()}' AND TeamID = {tid}) OR ExternalID = {player.UserID};");

                        while (playerSearchResult.Read())
                        {
                            existingPlayer = (playerSearchResult.IsDBNull(0)) ? null : playerSearchResult.GetInt16(0);
                        }

                        _databaseService.Disconnect();

                        if (existingPlayer == null)
                        {
                            var insertPlayerHeaderResult = _databaseService.ExecuteNonQuery($"INSERT INTO Players VALUES (null, {tid}, '{user}', '{player.FirstName.Trim().Replace("'", @"\'").Trim()}', '{player.LastName.Trim().Replace("'", @"\'").Trim()}', '{((player.UserImage != null) ? player.UserImage.Trim().Replace("'", @"\'").Trim() : "")}', {((player.JerseyNumber == null) ? "null" : int.Parse(player.JerseyNumber))}, '{division}-01-01', '{player.Position.Trim().Replace("'", @"\'").Trim()}', {player.UserID}); SELECT LAST_INSERT_ID();");
                            _databaseService.Disconnect();
                        }

                    }
                }

                return new Response
                {
                    StatusCode = 201,
                    Data = (TeamResponse)_teamService.GetTeamByID(tid, true).Data
                };
            }

            return new Response
            {
                StatusCode = 401,
                Data = null
            };
        }
        public async Task<Response> GetFullTeam(string user, string? orgId, int? eventId, string? teamId, string? clubId)
        {
            if(VerifyUserIsAdmin(user))
            {
                TGS_TeamInfoResponse response = await GetTeamInfo(orgId, eventId, teamId, clubId);

                // Check if team already exists and append to response object
                TGS_TeamStaff? hc = response.Data.StaffList.FirstOrDefault(staff => staff.Role == "Head Coach", null);
                string coachLastName = (hc == null) ? "" : hc.FullName.Split(", ")[0].Trim().ToUpper().Replace("'", @"\'");

                int? existingTeam = null;
                var teamSearchResult = _databaseService.ExecuteQuery($"SELECT TeamID FROM Teams WHERE Identifier='{response.TeamInfo.DivisionName.Substring(3)} {response.TeamInfo.ClubName.Trim().ToUpper().Replace("'", @"\'").Trim()} {coachLastName}' OR ExternalID={response.TeamInfo.TeamID}");

                Console.WriteLine($"SELECT TeamID FROM Teams WHERE Identifier='{response.TeamInfo.DivisionName.Substring(3)} {response.TeamInfo.ClubName.Trim().ToUpper().Replace("'", @"\'").Trim()} {coachLastName}' OR ExternalID={response.TeamInfo.TeamID}");

                while (teamSearchResult.Read())
                {
                    existingTeam = (teamSearchResult.IsDBNull(0)) ? null : teamSearchResult.GetInt16(0);
                }

                _databaseService.Disconnect();

                response.TeamInfo.ExistingTeam = existingTeam;

                // Check if teams / matches already exist and append to response object
                List<TGS_TeamOpponent> newOpps = new List<TGS_TeamOpponent>();

                foreach (TGS_TeamOpponent opp in response.Data.OpponentList)
                {
                    try
                    {
                        TGS_TeamOpponent newOpp = opp;
                        TGS_TeamInfoResponse? oppTeam = await GetTeamInfo(orgId, opp.EventID, opp.TeamID.ToString(), opp.ClubID.ToString());

                        TGS_TeamStaff? headCoach = oppTeam.Data.StaffList.FirstOrDefault(staff => staff.Role == "Head Coach", null);

                        string hcName = (headCoach == null) ? "" : headCoach.FullName.Split(", ")[0].Trim().ToUpper().Replace("'", @"\'");

                        int? et = null;
                        var tsr = _databaseService.ExecuteQuery($"SELECT TeamID FROM Teams WHERE Identifier='{oppTeam.TeamInfo.DivisionName.Substring(3)} {oppTeam.TeamInfo.ClubName.Trim().ToUpper().Replace("'", @"\'").Trim()} {hcName}' OR ExternalID={oppTeam.TeamInfo.TeamID}");

                        while (tsr.Read())
                        {
                            et = (tsr.IsDBNull(0)) ? null : tsr.GetInt16(0);
                        }

                        _databaseService.Disconnect();
                        opp.MatchExists = false;

                        if (et != null)
                        {
                            int? em = null;
                            var msr = _databaseService.ExecuteQuery($"SELECT MatchID FROM Matches WHERE (Team1ID={((existingTeam == null) ? "null" : existingTeam)} AND Team2ID={((et == null) ? "null" : et)} AND VenueName='{opp.Complex}' AND VenueNumber='{opp.Venue}') OR (Team1ID={((et == null) ? "null" : et)} AND Team2ID={((existingTeam == null) ? "null" : existingTeam)} AND VenueName='{opp.Complex}' AND VenueNumber='{opp.Venue}') OR ExternalID={opp.MatchID}");

                            while (msr.Read())
                            {
                                opp.MatchExists = !msr.IsDBNull(0);
                                opp.ExistingMatch = (msr.IsDBNull(0) ? null : msr.GetString(0));
                            }

                            _databaseService.Disconnect();
                        }
                        opp.ExistingTeam = et;

                        newOpps.Add(newOpp);
                    }
                    catch (NullReferenceException ex)
                    {
                        continue;
                    }

                }

                response.Data.OpponentList = newOpps;

                return new Response
                {
                    StatusCode = 200,
                    Data = response
                };
            }

            return new Response
            {
                StatusCode = 401,
                Data = null
            };
        }
        public async Task<TGS_TeamInfoResponse> GetTeamInfo(string? orgId, int? eventId, string? teamId, string? clubId)
        {
            HttpClient client = new HttpClient();

            var json = JsonConvert.SerializeObject(new CopyTeamRequest
            {
                OrgID = orgId,
                EventID = eventId,
                TeamID = teamId,
                ClubID = clubId
            });

            var httpData = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var teamInfo = await client.PostAsync("https://public.totalglobalsports.com/api/Event/get-individual-team-info", httpData);

            var clubTeamInfo = await client.GetAsync($"https://public.totalglobalsports.com/api/Event/get-individual-club-info-with-teams-and-staff/{orgId}/{clubId}/2837"); // Event ID 2837 is hard coded to work around non-posed club listings on latest events. Might need to change later if functionality is acting up.

            TGS_TeamInfoResponse response = new TGS_TeamInfoResponse();

            if (teamInfo.StatusCode == HttpStatusCode.OK && clubTeamInfo.StatusCode == HttpStatusCode.OK)
            {
                var ts = await teamInfo.Content.ReadAsStringAsync();
                var cs = await clubTeamInfo.Content.ReadAsStringAsync();

                response = JsonConvert.DeserializeObject<TGS_TeamInfoResponse>(ts);
                TGS_ClubInfoResponse clubTeamResponse = JsonConvert.DeserializeObject<TGS_ClubInfoResponse>(cs);
                response.TeamInfo = clubTeamResponse.Data.ClubTeamList.FirstOrDefault(team => (team.TeamID == teamId)); // Removed && team.EventID == eventId. Follows suit with listings issue
            }

            return response;
        }
        public Response UploadTeamAsCSV(string user, IFormFileCollection files)
        {
            if (VerifyUserIsAdmin(user))
            {
                List<TeamResponse> teams = new List<TeamResponse>();

                // Iterate upload files
                foreach (IFormFile file in files)
                {
                    Stream fstream = file.OpenReadStream();
                    StreamReader sr = new StreamReader(fstream);

                    int? clubId = null;
                    int? coachId = null;
                    List<int?> asstCoachIds = new List<int?>();
                    int? managerId = null;
                    int? teamId = null;

                    string? ageDivision = null;
                    string? coachLastName = null;
                    string? clubName = null;

                    // Loop file data
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        var columns = line.Split(",");

                        switch (columns[0].ToLower())
                        {
                            case "club":

                                clubName = columns[1];

                                _databaseService.Initialize();
                                MySqlCommand clubSearchCmd = new MySqlCommand("SELECT ClubID FROM Clubs WHERE ClubName=@ClubName", _databaseService.Connection);
                                clubSearchCmd.Parameters.AddWithValue("@ClubName", columns[1].Trim().Replace("'", @"\'").Trim());

                                MySqlDataReader clubSearchResult = clubSearchCmd.ExecuteReader();

                                while (clubSearchResult.Read())
                                {
                                    clubId = (clubSearchResult.IsDBNull(0)) ? null : clubSearchResult.GetInt16(0);
                                }

                                _databaseService.Disconnect();

                                if (clubId == null)
                                {
                                    _databaseService.Initialize();
                                    MySqlCommand insertClubCmd = new MySqlCommand("INSERT INTO Clubs VALUES (null, @ClubName, @Identifier, @AssociationIdentifier, @Image, null); SELECT LAST_INSERT_ID();", _databaseService.Connection);
                                    insertClubCmd.Parameters.AddWithValue("@ClubName", columns[1].Trim().Replace("'", @"\'").Trim());
                                    insertClubCmd.Parameters.AddWithValue("@Identifier", columns[1].Trim().ToUpper().Replace("'", @"\'").Trim());
                                    insertClubCmd.Parameters.AddWithValue("@AssociationIdentifier", columns[1].Trim().ToUpper().Replace("'", @"\'").Trim());
                                    insertClubCmd.Parameters.AddWithValue("@Image", columns[2]);

                                    MySqlDataReader insertClubResult = insertClubCmd.ExecuteReader();

                                    while (insertClubResult.Read())
                                    {
                                        clubId = (insertClubResult.IsDBNull(0)) ? null : insertClubResult.GetInt16(0);
                                    }

                                    _databaseService.Disconnect();

                                    if (clubId == null)
                                        continue;
                                }

                                break;
                            case "coach":
                            case "assistant coach":
                            case "manager":

                                int? existingUser = null;

                                _databaseService.Initialize();
                                MySqlCommand userSearchCmd = new MySqlCommand("SELECT UserID FROM Users WHERE FirstName=@FirstName AND LastName=@LastName", _databaseService.Connection);
                                userSearchCmd.Parameters.AddWithValue("@FirstName", columns[1].Replace("'", @"\'").Trim());
                                userSearchCmd.Parameters.AddWithValue("@LastName", columns[2].Replace("'", @"\'").Trim());

                                MySqlDataReader userSearchResult = userSearchCmd.ExecuteReader();

                                while (userSearchResult.Read())
                                {
                                    existingUser = (userSearchResult.IsDBNull(0)) ? null : userSearchResult.GetInt16(0);
                                }

                                _databaseService.Disconnect();

                                if (existingUser != null)
                                {
                                    switch (columns[0].ToLower())
                                    {
                                        case "coach":
                                            coachLastName = columns[2];
                                            coachId = existingUser;
                                            break;
                                        case "assistant coach":
                                            asstCoachIds.Add(existingUser);
                                            break;
                                        case "manager":
                                            managerId = existingUser;
                                            break;
                                    }
                                }
                                else
                                {
                                    _databaseService.Initialize();
                                    MySqlCommand insertUserCmd = new MySqlCommand("INSERT INTO Users VALUES (null, null, @FirstName, @LastName, @Image, null, null); SELECT LAST_INSERT_ID();", _databaseService.Connection);
                                    insertUserCmd.Parameters.AddWithValue("@FirstName", columns[1].Replace("'", @"\'").Trim());
                                    insertUserCmd.Parameters.AddWithValue("@LastName", columns[2].Replace("'", @"\'").Trim());
                                    insertUserCmd.Parameters.AddWithValue("@Image", columns[3].Replace("'", @"\'").Trim());

                                    MySqlDataReader insertUserResult = insertUserCmd.ExecuteReader();

                                    while (insertUserResult.Read())
                                    {
                                        switch (columns[0].ToLower())
                                        {
                                            case "coach":
                                                coachLastName = columns[2];
                                                coachId = (insertUserResult.IsDBNull(0)) ? null : insertUserResult.GetInt16(0);
                                                break;
                                            case "assistant coach":
                                                asstCoachIds.Add((insertUserResult.IsDBNull(0)) ? null : insertUserResult.GetInt16(0));
                                                break;
                                            case "manager":
                                                managerId = (insertUserResult.IsDBNull(0)) ? null : insertUserResult.GetInt16(0);
                                                break;
                                        }
                                    }

                                    _databaseService.Disconnect();
                                }

                                break;
                            case "team":

                                ageDivision = columns[2];

                                _databaseService.Initialize();
                                MySqlCommand teamSearchCmd = new MySqlCommand("SELECT TeamID FROM Teams WHERE ClubID=@ClubID AND Division=@Division", _databaseService.Connection);
                                teamSearchCmd.Parameters.AddWithValue("@ClubID", clubId);
                                teamSearchCmd.Parameters.AddWithValue("@Division", columns[2]);

                                MySqlDataReader teamSearchResult = teamSearchCmd.ExecuteReader();

                                while (teamSearchResult.Read())
                                {
                                    teamId = (teamSearchResult.IsDBNull(0)) ? null : teamSearchResult.GetInt16(0);
                                }

                                _databaseService.Disconnect();

                                if (teamId == null && coachId != null)
                                {                                     
                                    _databaseService.Initialize();
                                    MySqlCommand insertTeamCmd = new MySqlCommand("INSERT INTO Teams VALUES (null, @ClubID, 1, @Owner, @Gender, @Division, @ManagerUserID, @CoachUserID, @AssistantCoachUserIDs, @Identifier, null); SELECT LAST_INSERT_ID();", _databaseService.Connection);
                                    insertTeamCmd.Parameters.AddWithValue("@ClubID", clubId);
                                    insertTeamCmd.Parameters.AddWithValue("@Owner", user);
                                    insertTeamCmd.Parameters.AddWithValue("@Gender", columns[1].Trim().Replace("'", @"\'").Trim());
                                    insertTeamCmd.Parameters.AddWithValue("@Division", columns[2].Trim().Replace("'", @"\'").Trim());
                                    insertTeamCmd.Parameters.AddWithValue("@ManagerUserID", managerId);
                                    insertTeamCmd.Parameters.AddWithValue("@CoachUserID", coachId);
                                    insertTeamCmd.Parameters.AddWithValue("@AssistantCoachUserIDs", JsonConvert.SerializeObject(asstCoachIds));
                                    insertTeamCmd.Parameters.AddWithValue("@Identifier", $"{columns[2].Trim().Substring(2).Replace("'", @"\'").Trim()} {clubName.Trim().ToUpper().Replace("'", @"\'").Trim()} {coachLastName.Trim().ToUpper().Replace("'", @"\'").Trim()}");

                                    MySqlDataReader insertTeamResult = insertTeamCmd.ExecuteReader();

                                    while (insertTeamResult.Read())
                                    {
                                        teamId = (insertTeamResult.IsDBNull(0)) ? null : insertTeamResult.GetInt16(0);
                                    }

                                    _databaseService.Disconnect();
                                }

                                if (teamId == null)
                                    continue;

                                break;
                            case "player":

                                int? existingPlayer = null;

                                _databaseService.Initialize();
                                MySqlCommand playerSearchCmd = new MySqlCommand("SELECT PlayerID FROM Players WHERE FirstName=@FirstName AND LastName=@LastName AND TeamID=@TeamID", _databaseService.Connection);
                                playerSearchCmd.Parameters.AddWithValue("@FirstName", columns[1].Trim().Replace("'", @"\'").Trim());
                                playerSearchCmd.Parameters.AddWithValue("@LastName", columns[2].Trim().Replace("'", @"\'").Trim());
                                playerSearchCmd.Parameters.AddWithValue("@TeamID", teamId);

                                MySqlDataReader playerSearchResult = playerSearchCmd.ExecuteReader();

                                while (playerSearchResult.Read())
                                {
                                    existingPlayer = (playerSearchResult.IsDBNull(0)) ? null : playerSearchResult.GetInt16(0);
                                }

                                _databaseService.Disconnect();

                                if(existingPlayer == null)
                                {
                                    _databaseService.Initialize();
                                    MySqlCommand insertPlayerCmd = new MySqlCommand("INSERT INTO Players VALUES (null, @TeamID, @Owner, @FirstName, @LastName, @Image, @PlayerNumber, @DOB, @Position, null)", _databaseService.Connection);
                                    insertPlayerCmd.Parameters.AddWithValue("@TeamID", teamId);
                                    insertPlayerCmd.Parameters.AddWithValue("@Owner", user);
                                    insertPlayerCmd.Parameters.AddWithValue("@FirstName", columns[1].Trim().Replace("'", @"\'").Trim());
                                    insertPlayerCmd.Parameters.AddWithValue("@LastName", columns[2].Trim().Replace("'", @"\'").Trim());
                                    insertPlayerCmd.Parameters.AddWithValue("@Image", columns[4].Trim().Replace("'", @"\'").Trim());
                                    insertPlayerCmd.Parameters.AddWithValue("@PlayerNumber", columns[3].Trim().Replace("'", @"\'").Trim());
                                    insertPlayerCmd.Parameters.AddWithValue("@DOB", $"{ageDivision}-01-01");
                                    insertPlayerCmd.Parameters.AddWithValue("@Position", columns[5].Trim().Replace("'", @"\'").Trim());

                                    insertPlayerCmd.ExecuteNonQuery();
                                }

                                _databaseService.Disconnect();

                                break;
                        }                
                    }
                
                    if(teamId != null)
                        teams.Add((TeamResponse)_teamService.GetTeamByID(teamId, true).Data);
                }

                return new Response
                {
                    StatusCode = 201,
                    Data = new
                    {
                        Total = teams.Count,
                        Teams = teams
                    }
                };
            }

            return new Response
            {
                StatusCode = 401,
                Data = null
            };
        }
        public bool VerifyUserIsAdmin(string userId)
        {
            int resultCount = 0;

            _databaseService.Initialize();
            MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM Users WHERE AuthID=@UserID AND Admin=1", _databaseService.Connection);
            cmd.Parameters.AddWithValue("@UserID", userId);

            MySqlDataReader result = cmd.ExecuteReader();

            while (result.Read())
            {
                resultCount = result.IsDBNull(0) ? 0 : result.GetInt32(0);
            }

            _databaseService.Disconnect();

            if (resultCount > 0)
                return true;

            return false;
        }
    }
}
