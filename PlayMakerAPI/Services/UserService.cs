using Microsoft.OpenApi.Writers;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using PlayMakerAPI.Models.Response;
using System.Net.Http;

namespace PlayMakerAPI.Services
{
    public class UserService
    {
        private DatabaseService _databaseService = new DatabaseService();
        public async Task VerifyOrInsertUser(string userId, string accessToken)
        {
            int resultCount = 0;

            _databaseService.Initialize();
            MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM Users WHERE AuthID=@UserID", _databaseService.Connection);
            cmd.Parameters.AddWithValue("@UserID", userId);

            MySqlDataReader result = cmd.ExecuteReader();

            while(result.Read())
            {
                resultCount = result.IsDBNull(0) ? 0 : result.GetInt32(0);
            }

            _databaseService.Disconnect();

            if(resultCount == 0)
            {
                HttpClient _client = new HttpClient();
                _client.DefaultRequestHeaders.Add("Authorization", accessToken);
                HttpResponseMessage response = await _client.GetAsync("https://dev-bfq0h45o5v2zcw6h.us.auth0.com/userinfo");
                ProfileInfo profileInfo = JsonConvert.DeserializeObject<ProfileInfo>(await response.Content.ReadAsStringAsync());

                _databaseService.Initialize();
                MySqlCommand insertCmd = new MySqlCommand("INSERT INTO Users VALUES (null, @UserID, @FirstName, @LastName, @Image, null)", _databaseService.Connection);
                insertCmd.Parameters.AddWithValue("@UserID", userId);
                insertCmd.Parameters.AddWithValue("@FirstName", profileInfo.Given_Name);
                insertCmd.Parameters.AddWithValue("@LastName", profileInfo.Family_Name);
                insertCmd.Parameters.AddWithValue("@Image", profileInfo.Picture);

                insertCmd.ExecuteNonQuery();
            }

        }
        public User? GetUserByID(int? userId)
        {
            User? user = null;

            _databaseService.Initialize();
            MySqlCommand cmd = new MySqlCommand("SELECT UserID, FirstName, LastName, Image FROM Users WHERE UserID=@UserID", _databaseService.Connection);
            cmd.Parameters.AddWithValue("@UserID", userId);

            MySqlDataReader result = cmd.ExecuteReader();

            while (result.Read())
            {
                user = new User
                {
                    UserID = (result.IsDBNull(0)) ? null : result.GetInt16(0),
                    FirstName = (result.IsDBNull(1)) ? null : result.GetString(1),
                    LastName = (result.IsDBNull(2)) ? null : result.GetString(2),
                    UserImage = (result.IsDBNull(3)) ? null : result.GetString(3)
                };
            }

            _databaseService.Disconnect();

            return user;
        }

    }
}
