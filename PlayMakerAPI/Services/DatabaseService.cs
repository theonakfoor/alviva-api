using MySql.Data.MySqlClient;

namespace PlayMakerAPI.Services
{
    public class DatabaseService
    {
        private string Server = "REDACTED";
        private string DBName = "core";
        private string Username = "system";
        private string Password = "REDACTED";

        private bool Connected = false;
        public MySqlConnection Connection { get; set; }

        public DatabaseService()
        {
            Initialize();
        }

        public void Initialize()
        {
            this.Connection = new MySqlConnection(string.Format("Server={0}; database={1}; UID={2}; password={3}", Server, DBName, Username, Password));
            this.Connection.Open();
            this.Connected = true;
        }

        public bool IsConnected()
        {
            return this.Connected;
        }

        public MySqlDataReader ExecuteQuery(string query)
        {

            if (!this.Connected)
                Initialize();

            var cmd = new MySqlCommand(query, this.Connection);
            return cmd.ExecuteReader();
        }

        public int ExecuteNonQuery(string query)
        {
            if (!this.Connected)
                Initialize();

            var cmd = new MySqlCommand(query, this.Connection);
            return cmd.ExecuteNonQuery();
        }

        public void Disconnect()
        {
            this.Connection.Close();
            this.Connection.Dispose();
            this.Connected = false;
        }

    }
}
