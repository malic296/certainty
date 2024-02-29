using certainty.Pages;
using Newtonsoft.Json.Linq;
using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using certainty.Pages.models;
using System.Reflection.PortableExecutable;
using System.ComponentModel.Design;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace certainty.Injections
{
    public interface ISqlClient
    {
        string registerUser(string username, string email, string password);
        string HashPassword(string password, string salt);

        bool validateLogin(string username, string password);

        string getColumn(string username);

        bool setCurrency(string currency, string username);


        string ogCurrency(string username);

        List<double> getValues(string username);

        void finalCurrencyUpdate(double oldCur, double newCur, string username);

        List<Record> getRecords(string username);
        List<Record> getRecordsSort(string username, string sort, string sortBy);

        void deleteRecord(int id);

        Record getRecord(int id);
        bool createRecord(string category, double value, string userID, DateTime date);

        List<string> getCategories(string username);

        bool updateRecord(int recordID, string category, double value, DateTime date);

        List<YearRecord> getYearRecords(DateTime dateYearAgo, string username);

        List<Record> getRecordsThisMonth(string username, DateTime today);

        List<Record> getCategoryValue(string username);

        void deleteAllRecords(string username);

    }

    public class SqlClient : ISqlClient
    {
        private readonly string _connectionString;
        private readonly string _userTable;
        private readonly string _recordTable;

        public SqlClient(IConfiguration configuration)
        {
            _connectionString = configuration.GetSection("DatabaseConfig:ConnectionString").Value;
            _userTable = configuration.GetSection("DatabaseConfig:userTable").Value;
            _recordTable = configuration.GetSection("DatabaseConfig:recordTable").Value;
        }

        //deletes all records
        public void deleteAllRecords(string username)
        {
            string deleteQuery = $"DELETE FROM {_recordTable} WHERE userID = @username;";

            using(SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using(SqlCommand command = new SqlCommand(deleteQuery, connection))
                {
                    command.Parameters.AddWithValue("@username", username);

                    command.ExecuteNonQuery();
                }
            }


        }
            //method for categories with their values
            public List<Record> getCategoryValue(string username)
            {

            List<Record> records = new List<Record>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = $"select category, SUM(value) as value from {_recordTable} where userID = @username group by category;";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);

                    try
                    {
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {

                            Record record = new Record()
                            {
                                category = reader["category"].ToString(),
                                value = Convert.ToDouble(reader["value"]),

                            };
                            records.Add(record);

                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    return records;
                }
            }

        }


        //Graph methods
        public List<Record> getRecordsThisMonth(string username, DateTime today)
        {
            List<Record> records = new List<Record>();

            DateTime firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {

                string query = $"SELECT SUM(value) AS value, recordDate FROM {_recordTable} " +
                    "WHERE userID = @username AND recordDate >= @date GROUP BY recordDate ORDER BY recordDate;";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@date", firstDayOfMonth.Date);

                    try
                    {
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {

                            Record record = new Record()
                            {
                               
                                value = Convert.ToDouble(reader["value"]),
                                recordDate = Convert.ToDateTime(reader["recordDate"]).Date
                            };
                            records.Add(record);

                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    return records;
                }
            }

        }

        public List<YearRecord> getYearRecords(DateTime dateYearAgo, string username)
        {
            List<YearRecord> records = new List<YearRecord>();

            string selectQuery = "" +
                "" +
                "SELECT " +
                "DATEPART(YEAR, recordDate) AS Year, " +
                "DATEPART(MONTH, recordDate) AS Month, " +
                "COUNT(*) AS RecordsCount, " +
                "SUM(value) as value " +
                "FROM " +
                $"{_recordTable} " +
                "WHERE " +
                "recordDate >= @dateYearAgo AND userID = @username " +
                "GROUP BY " +
                "DATEPART(YEAR, recordDate)," +
                "DATEPART(MONTH, recordDate) " +
                "ORDER BY " +
                "Year, Month;";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using(SqlCommand command = new SqlCommand(selectQuery, connection))
                {
                    command.Parameters.AddWithValue("@dateYearAgo", dateYearAgo);
                    command.Parameters.AddWithValue("@username", username);

                    try
                    {
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {

                            YearRecord record = new YearRecord()
                            {
                                year = (int)reader["Year"],
                                month = (int)reader["Month"],
                                recordsCount = (int)reader["RecordsCount"],
                                value = (double)reader["value"]
                            };

                            records.Add(record);
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }

                }
            }

            return records;
        }

        //-------------------------------------

        //method for record selection (sorted)
        public List<Record> getRecordsSort(string username, string sort, string sortBy)
        {
            List<Record> records = new List<Record>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                // Validate sortBy against a list of valid columns
                List<string> validColumns = new List<string> { "category", "value", "recordDate" };

                if (!validColumns.Contains(sortBy))
                {
                    // Handle invalid sortBy values (throw an exception, log an error, etc.)
                    throw new ArgumentException("Invalid sortBy parameter.");
                }

                string sortOrder = (sort == "DESC") ? "DESC" : "ASC";
                string query = $"SELECT * FROM {_recordTable} WHERE userID = @username ORDER BY {sortBy} {sortOrder};";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);

                    try
                    {
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            Record record = new Record()
                            {
                                category = reader["category"].ToString(),
                                value = Convert.ToDouble(reader["value"]),
                                userID = reader["userID"].ToString(),
                                recordID = Convert.ToInt32(reader["recordID"]),
                                recordDate = Convert.ToDateTime(reader["recordDate"]).Date,
                            };
                            records.Add(record);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            }

            return records;
        }

        public bool updateRecord(int recordID, string category, double value, DateTime date)
        {
            string updateQuery = $"update {_recordTable} set category=@category, value=@value, recordDate=@date WHERE recordID = @recordID;";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {

                try
                {
                    connection.Open();
                    using(SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@recordID", recordID);
                        command.Parameters.AddWithValue("@category", category);
                        command.Parameters.AddWithValue("@value", value);
                        command.Parameters.AddWithValue("@date", date.ToString("yyyy-MM-dd"));

                        int rowsAffected = command.ExecuteNonQuery();

                        if(rowsAffected > 0)
                        {
                            return true;
                        }

                        else
                        {
                            return false;
                        }


                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

        }

        //gets users categories
        public List<string> getCategories(string username)
        {
            List<String> categories = new List<String>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = $"select category from {_recordTable} where userID = @username GROUP BY category;";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);

                    try
                    {
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {

                            string category = reader["category"].ToString();
                                
                            
                            categories.Add(category);

                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            }

            return categories;
        }

        //CreateRecord
        public bool createRecord(string category, double value, string userID, DateTime date)
        {
            string insertQuery = "" +
                $"insert into {_recordTable}(category, userID, value, recordDate) VALUES(@category, @userID, @value, @date)" +
                "";
           
            
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@category", category);
                        command.Parameters.AddWithValue("@value", value);
                        command.Parameters.AddWithValue("@userID", userID);
                        command.Parameters.AddWithValue("@date", date.Date);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }

                    }
                }
                catch (Exception ex)
                {
                    string err = ex.Message;
                    return false;
                }
                

                    
            }
        }


        //returns record
        public Record getRecord(int id)
        {
            Record record = null;

            string updateQuery = $"select * from {_recordTable} where recordID = @id";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("@id", id);

                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        record = new Record()
                        {
                            recordID = reader.GetInt32(0),
                            category = reader.GetString(1),
                            userID = reader.GetString(2),
                            value = reader.GetDouble(3),
                            recordDate = reader.GetDateTime(4),

                        };
                       
                    }
                    

                }
            }
            return record;
        }


        //Delete record based on ID
        public void deleteRecord(int id)
        {
            string updateQuery = $"delete from {_recordTable} where recordID = @id";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("@id", id);


                    command.ExecuteNonQuery();

                }
            }
        }


        //metoda pro zjiskání záznamů
        public List<Record> getRecords(string username)
        {            
            List<Record> records = new List<Record>();

            using(SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = $"select * from {_recordTable} where userID = @username;";
                using(SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);

                    try
                    {
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {

                            Record record = new Record()
                            {
                                category = reader["category"].ToString(),
                                value = Convert.ToDouble(reader["value"]),
                                userID = reader["userID"].ToString(),
                                recordID = Convert.ToInt32(reader["recordID"]),
                                recordDate = Convert.ToDateTime(reader["recordDate"]).Date,
                            };
                            records.Add(record);

                        }
                        
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    return records;
                }
            }
            
        }



        //metoda pro update měny uživatele 
        public void finalCurrencyUpdate(double oldVal, double newVal, string username){
            string updateQuery = $"UPDATE {_recordTable} set value = @newVal where value = @oldVal and userID = @username";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("username", username);
                    command.Parameters.AddWithValue("newVal", newVal);
                    command.Parameters.AddWithValue("oldVal", oldVal);


                    command.ExecuteNonQuery();

                }
            }
        }

       public List<double> getValues(string username)
        {
            List<double> values = new List<double>();


            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = $"select value from {_recordTable} where userID = @username group by value;";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);

                    try
                    {
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            values.Add(Convert.ToDouble(reader["value"]));

                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Chyba: " + ex.Message);
                    }

                    return values;
                }
            }
        }


        //původní měna
        public string ogCurrency(string username)
        {
            string final = "";


            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = $"select currency from {_userTable} where username = @username;";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);

                    try
                    {
                        connection.Open();
                        object result = command.ExecuteScalar();

                        
                        final = result.ToString();
                        
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Chyba: " + ex.Message);

                    }

                    return final;
                }
            }
        }

        //Nastavení měny uživatele

        public bool setCurrency(string currency, string username)
        {
            using(SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                
                string query =
                    $"UPDATE {_userTable} SET currency = @newCur where username = @username;";
                using(SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@newCur", currency);

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        return true;
                    }
                    else { return false; }
                    connection.Close();
                }
            }

        }


        //získání emailu uživatele
        public string getColumn(string Username)
        {
            string email = "";


            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = $"select email from {_userTable} Where username = @username";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", Username);
                    

                    try
                    {
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                email = reader.GetString(0);
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }



                }
            }

            return email;
        }

        //Získání soli uživatele
        private string getSalt(string Username)
        {
            string salt = "";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = $"select salt from {_userTable} Where username = @username";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", Username);

                    try
                    {
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                salt = reader.GetString(0);
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }



                }
            }

            return salt;
        }

        //ověření loginu uživatele
        public bool validateLogin(string username, string password)
        {
            bool validation = false;
            string salt = getSalt(username);
            password = HashPassword(password, salt);
            string databasePassword = "";
            string databaseUsername = "";

            try {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = $"select username, password from {_userTable}";
                    using(SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);

                        

                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                        
                            databaseUsername = reader.GetString(0);
                            databasePassword = reader.GetString(1);

                            if(string.Equals(username, databaseUsername) && string.Equals(password, databasePassword))
                            {
                                validation = true; 
                                break;
                            }
                            else
                            {
                                validation = false;
                                continue;
                            }

                        }
                    }
                }
            }
            catch(Exception ex)
            {
                validation = false;
            }


            return validation;

        }



        //Vytvoření soli
        private String generateSalt()
        {
            byte[] saltBytes = new byte[32];
            using (var rngCsp = new RNGCryptoServiceProvider()) {
                rngCsp.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }


        //metoda pro hashování
        public string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] saltBytes = Encoding.UTF8.GetBytes(salt);
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password + Convert.ToBase64String(saltBytes));

                byte[] hashedBytes = sha256.ComputeHash(passwordBytes);

                
                return Convert.ToBase64String(hashedBytes);
            }
        }


        //Check if Username or Email exists in my DB
        private bool checkCredentials(string username, string email)
        {
            bool CredValidation = false;
            string SqlUsername = "";
            string SqlEmail = "";


            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try{
                    conn.Open();
                    string selectQuery = $"SELECT * FROM {_userTable}";

                    using (SqlCommand cmd = new SqlCommand(selectQuery, conn))
                    {
                        SqlDataReader reader = cmd.ExecuteReader();
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                SqlUsername = reader.GetString(0);
                                SqlEmail = reader.GetString(2);
                                if(SqlUsername == username || SqlEmail == email)
                                {
                                    CredValidation = false;
                                    break;
                                    
                                }
                                
                                else
                                {
                                    CredValidation = true;
                                    continue;
                                }
                            }
                            if(CredValidation == true)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return true;
                        }

                    }
                    
                }
                catch (Exception ex)
                {
                    return false;
                }
                
            }
            
        }

        //Registering user
        public string registerUser(string username, string email, string password)
        {
            bool validation = false;
            validation = checkCredentials(username, email);
            string salt = generateSalt();
            password = HashPassword(password, salt);
            if (validation == true)
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {



                    string insertQuery = $"INSERT INTO {_userTable}(username, email, password, salt, currency) VALUES (@Username, @Email, @Password, @Salt, 'CZK')";

                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {

                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@Password", password);
                        command.Parameters.AddWithValue("@Salt", salt);

                        try
                        {
                            connection.Open();
                            int rowsAffected = command.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                return "created";
                            }
                            else
                            {
                                return "Something went wrong";
                            }
                        }
                        catch (Exception ex)
                        {
                            return "Something went wrong";
                        }
                    }
                }

            }
            else
            {
                return "Username or email already exists";
            }
            
        }
    }
}
