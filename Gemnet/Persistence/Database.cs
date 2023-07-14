using System.Data;
using Dapper;
using MySql.Data.MySqlClient;

namespace Gemnet.Persistence
{
    public class Database
    {
        public static string ConnectionString { get; set; }

        private IDbConnection m_connection;
        public Database(string connectionString = "")
        {
            if (ConnectionString != null && connectionString == string.Empty)
                connectionString = ConnectionString;

            this.m_connection = new MySqlConnection(connectionString);
        }

        public void Connect()
        {
            this.m_connection.Open();
        }

        public void Close()
        {
            this.m_connection.Close();
        }

        public bool IsConnected()
            => this.m_connection.State == ConnectionState.Open;

        public IEnumerable<T> Select<T>(string filter, object? param)
        {
            if (!this.IsConnected())
                this.Connect();

            using (var conn = this.m_connection)
            {
                return conn.Query<T>(filter, param).ToList();
            }
        }

        public int Update<T>(string filter, object? param)
        {
            if (!this.IsConnected())
                this.Connect();

            using (var conn = this.m_connection)
                return conn.Execute(filter, param);
        }

        public T Scalar<T>(string filter, object? param)
        {
            if (!this.IsConnected())
                this.Connect();

            using (var conn = this.m_connection)
                return (T)conn.ExecuteScalar(filter, param);
        }

        public T SelectFirst<T>(string filter, object? param)
        {
            if (!this.IsConnected())
                this.Connect();

            try
            {
                using (var conn = this.m_connection)
                    return (T)conn.QuerySingle<T>(filter, param);
            } catch {
                return default(T);
            }
        }

        public int Execute(string filter, object? param)
        {
            if (!this.IsConnected())
                this.Connect();

            using (var conn = this.m_connection)
                return conn.Execute(filter, param);
        }
    }
}
