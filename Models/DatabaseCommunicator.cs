using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace PLC_GUI
{
    public class DatabaseCommunicator
    {
        private string _connectionString;

        public DatabaseCommunicator(string server, string database, string userId, string password)
        {
            _connectionString = $"Server={server};Database={database};User ID={userId};Password={password};";
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<object> GetValueFromDatabaseAsync(string tableName, string columnName, int rowId)
        {
           
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = $"SELECT {columnName} FROM {tableName} WHERE id = @id";
                
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", rowId);
                    var result = await command.ExecuteScalarAsync();
                    return result;
                }
            }
        }

        public async Task UpdateDatabaseAsync(string tableName, string columnName, int rowId, object value)
        {
            string query = $"UPDATE {tableName} SET {columnName} = @Value WHERE id = @Id";

            using (var connection = new MySqlConnection(_connectionString))
            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Value", value);
                command.Parameters.AddWithValue("@Id", rowId);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
