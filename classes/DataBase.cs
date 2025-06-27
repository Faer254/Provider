using MySql.Data.MySqlClient;
using Provider.help_windows;
using System.Data;
using System.Globalization;

namespace Provider.classes
{
    public class DataBase
    {
        private readonly string connectionString = "Server=localhost;Database=provider;Uid=root;Pwd=root;";

//-------------------------------------SYSTEM COMMANDS----------------------------------------
        public bool completeCommand(string query)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.ExecuteNonQuery();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                new ErrorWindow($"Ошибка: {ex.Message}", Manager.theme).ShowDialog();

                return false;
            }
        }

        public string isRecordExists(string tableName, string whatToCheck, string value)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = $"SELECT COUNT(*) FROM `{MySqlHelper.EscapeString(tableName)}` WHERE `{MySqlHelper.EscapeString(whatToCheck)}` = @value";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@value", value);

                        int count = Convert.ToInt32(command.ExecuteScalar());

                        return count > 0 ? "true" : "false";
                    }
                }
            }
            catch (Exception ex)
            {
                new ErrorWindow($"Ошибка: {ex.Message}", Manager.theme).ShowDialog();

                return "error";
            }
        }

        public string checkPassword(string tableName, uint id, string password)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = $"SELECT COUNT(*) FROM `{MySqlHelper.EscapeString(tableName)}` WHERE `id` = @id AND `password` = @password";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        command.Parameters.AddWithValue("@password", Encrypter.EncryptStringToBytes_AES(password));

                        int count = Convert.ToInt32(command.ExecuteScalar());

                        return count > 0 ? "true" : "false";
                    }
                }
            }
            catch (Exception ex)
            {
                new ErrorWindow($"Ошибка: {ex.Message}", Manager.theme).ShowDialog();

                return "error";
            }
        }
//-------------------------------------SYSTEM COMMANDS END----------------------------------------

//-------------------------------------ACTION TABLES----------------------------------------
        public void addClientAction(uint userID, string action, DateTime time)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = $"INSERT INTO `client_action` (`client_id`, `action`, `time`) VALUES (@id, @action, @time)";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", userID);
                        command.Parameters.AddWithValue("@action", action);
                        command.Parameters.AddWithValue("@time", time);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                new ErrorWindow($"Ошибка: {ex.Message}", Manager.theme).ShowDialog();
            }
        }

        public void addEmployeeAction(uint userID, string action, DateTime time)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = $"INSERT INTO `employee_action` (`employee_id`, `action`, `time`) VALUES (@id, @action, @time)";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", userID);
                        command.Parameters.AddWithValue("@action", action);
                        command.Parameters.AddWithValue("@time", time);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                new ErrorWindow($"Ошибка: {ex.Message}", Manager.theme).ShowDialog();
            }
        }

        public DataTable getActionLog(string tableName)
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = $"SELECT * FROM `{tableName}`";

                    using (var adapter = new MySqlDataAdapter(query, connection))
                    {
                        adapter.Fill(dataTable);
                    }
                    
                }
            }
            catch (Exception ex)
            {
                new ErrorWindow($"Ошибка: {ex.Message}", Manager.theme).ShowDialog();
            }

            return dataTable;
        }
//-------------------------------------ACTION TABLES END----------------------------------------

//----------------------------------------GET SOMEONE-----------------------------------------
        public List<object>? getClient(string parameter, string value)
        {
            List<object> userList = new List<object>();

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = $"SELECT `id`, `phone_number`, `email`, `status_id`, `full_name`, `wallet_account`, `internet`, `phone`, `sms` FROM `client` " +
                                    $"WHERE `{MySqlHelper.EscapeString(parameter)}` = @value";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@value", value);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var record = new List<object>();

                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    object val = reader.GetValue(i);

                                    if (val is DBNull)
                                        val = null;

                                    record.Add(val);
                                }

                                userList = record;
                            }
                        }
                    }
                }
            }
            catch
            {
                return null;
            }

            return userList;
        }

        public List<List<object>>? getClientServices(uint clientID)
        {
            List<List<object>> services = new List<List<object>>();

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT cs.id, cs.service_id, cs.next_payment, cs.address, " +
                                          "s.type_id, s.service_name, s.cost " +
                                    "FROM `concluded_services` cs " +
                                    "LEFT JOIN `services` s " +
                                    "ON cs.service_id = s.id " +
                                    "WHERE cs.client_id = @value";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@value", clientID);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var record = new List<object>
                                {
                                    reader["id"],
                                    reader["service_id"],
                                    reader["next_payment"] is DBNull ? null : reader["next_payment"],
                                    reader["address"] is DBNull ? null : reader["address"],
                                    reader["type_id"],
                                    reader["service_name"],
                                    reader["cost"],
                                };

                                services.Add(record);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                new ErrorWindow($"Ошибка: {ex.Message}", Manager.theme).ShowDialog();
                return null;
            }

            return services;
        }

        public List<object>? getEmployee(string parameter, string value)
        {
            List<object> userList = new List<object>();

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = $"SELECT `id`, `login`, `email`, `status_id`, `full_name`, `role_id`, `phone_number` FROM `employee` " +
                                    $"WHERE `{MySqlHelper.EscapeString(parameter)}` = @value";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@value", value);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var record = new List<object>();

                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    object val = reader.GetValue(i);

                                    if (val is DBNull)
                                        val = null;

                                    record.Add(val);
                                }

                                userList = record;
                            }
                        }
                    }
                }
            }
            catch
            {
                return null;
            }

            return userList;
        }

        public List<object> getUserByLoginAndPassword(string table, string loginOrEmail, string password)
        {
            List<object> userList = new List<object>();

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query;

                    if (table == "client")
                    {
                        query = $"SELECT `id`, `phone_number`, `email`, `status_id`, `full_name`, `wallet_account`, `internet`, `phone`, `sms` FROM `client` " +
                                $"WHERE `email` = @login AND `password` = @password";
                    }
                    else
                    {
                        query = $"SELECT `id`, `login`, `email`, `status_id`, `full_name`, `role_id`, `phone_number` FROM `employee` " +
                                $"WHERE (`login` = @login OR `email` = @login) AND `password` = @password";
                    }

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@login", loginOrEmail);
                        command.Parameters.AddWithValue("@password", Encrypter.EncryptStringToBytes_AES(password));

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var record = new List<object>();

                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    object value = reader.GetValue(i);

                                    if (value is DBNull)
                                        value = null;

                                    record.Add(value);
                                }

                                userList = record;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                new ErrorWindow($"Ошибка: {ex.Message}", Manager.theme).ShowDialog();
            }

            return userList;
        }
//----------------------------------------GET SOMEONE END-----------------------------------------

//----------------------------------------SAFETY---------------------------------------------------

        public bool updateEmployee(uint userID, string whatToUpdate, string newValue)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = $"UPDATE `employee` SET `{MySqlHelper.EscapeString(whatToUpdate)}` = @newvalue WHERE `id` = @id";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", userID);
                        command.Parameters.AddWithValue("@newvalue", newValue);

                        command.ExecuteNonQuery();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                new ErrorWindow($"Ошибка: {ex.Message}", Manager.theme).ShowDialog();

                return false;
            }
        }

        public bool updateEmail(string tableName, uint userID, string newEmail)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = $"UPDATE `{MySqlHelper.EscapeString(tableName)}` SET `email` = @newemail WHERE `id` = @id";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", userID);
                        command.Parameters.AddWithValue("@newemail", newEmail);

                        command.ExecuteNonQuery();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                new ErrorWindow($"Ошибка: {ex.Message}", Manager.theme).ShowDialog();

                return false;
            }
        }

        public bool updatePassword(string tableName, uint userID, string newPassword)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = $"UPDATE `{MySqlHelper.EscapeString(tableName)}` SET `password` = @newpassword WHERE `id` = @id";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", userID);
                        command.Parameters.AddWithValue("@newpassword", Encrypter.EncryptStringToBytes_AES(newPassword));

                        command.ExecuteNonQuery();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                new ErrorWindow($"Ошибка: {ex.Message}", Manager.theme).ShowDialog();
                
                return false;
            }
        }
//----------------------------------------SAFETY END---------------------------------------------------

//------------------------------------TARIFS--------------------------------------------
        public List<object> getClientTarif(uint clientID)
        {
            List<object> services = new List<object>();

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT cs.id, cs.next_payment, " +
                                          "s.service_name, s.cost, s.internet, s.phone, s.sms, cs.service_id " +
                                    "FROM `concluded_services` cs " +
                                    "LEFT JOIN `services` s " +
                                    "ON cs.service_id = s.id " +
                                    "WHERE cs.client_id = @clientID " +
                                    "AND s.type_id = 1";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@clientID", clientID);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var record = new List<object>();

                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    object value = reader.GetValue(i);

                                    if (value is DBNull)
                                        value = null;

                                    record.Add(value);
                                }

                                services = record;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                new ErrorWindow($"Ошибка: {ex.Message}", Manager.theme).ShowDialog();
                return null;
            }

            return services;
        }

        public DataTable getTarifs(string idForIgnore = null)
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query;
                    if (idForIgnore != null)
                    {
                        query = $"SELECT * FROM `services` WHERE `id` <> {idForIgnore} AND `availability` = 1 AND `type_id` = 1";

                        using (var adapter = new MySqlDataAdapter(query, connection))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                    else
                    {
                        query = $"SELECT * FROM `services` WHERE `availability` = 1 AND `type_id` = 1";

                        using (var adapter = new MySqlDataAdapter(query, connection))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                new ErrorWindow($"Ошибка: {ex.Message}", Manager.theme).ShowDialog();
            }

            return dataTable;
        }
//------------------------------------TARIFS END--------------------------------------------

//------------------------------------HOME SERVICES--------------------------------------------
        public List<object> getClientHomeService(uint clientID, uint type)
        {
            List<object> services = new List<object>();

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT cs.id, cs.service_id, cs.next_payment, cs.address, " +
                                          "s.service_name, s.description, s.cost " +
                                    "FROM `concluded_services` cs " +
                                    "LEFT JOIN `services` s " +
                                    "ON cs.service_id = s.id " +
                                    "WHERE cs.client_id = @clientID " +
                                    $"AND s.type_id = {type}";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@clientID", clientID);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var record = new List<object>();

                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    object value = reader.GetValue(i);

                                    if (value is DBNull)
                                        value = null;

                                    record.Add(value);
                                }

                                services = record;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                new ErrorWindow($"Ошибка: {ex.Message}", Manager.theme).ShowDialog();
                return null;
            }

            return services;
        }

        public DataTable getHomeServices(uint type, string idForIgnore = null)
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query;
                    if (idForIgnore != null)
                    {
                        query = $"SELECT * FROM `services` WHERE `id` <> {idForIgnore} AND `availability` = 1 AND `type_id` = {type}";

                        using (var adapter = new MySqlDataAdapter(query, connection))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                    else
                    {
                        query = $"SELECT * FROM `services` WHERE `availability` = 1 AND `type_id` = {type}";

                        using (var adapter = new MySqlDataAdapter(query, connection))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                new ErrorWindow($"Ошибка: {ex.Message}", Manager.theme).ShowDialog();
            }

            return dataTable;
        }
//------------------------------------HOME SERVICES END--------------------------------------------

//------------------------------------FREE SERVICES-------------------------------------------------
        public DataTable getFreeConnectedServices(uint userId)
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT cs.id, cs.service_id, cs.address, " +
                                          "s.service_name, s.description, s.need_an_address " +
                                    "FROM `concluded_services` cs " +
                                    "LEFT JOIN `services` s " +
                                    "ON cs.service_id = s.id " +
                                   $"WHERE cs.client_id = {userId} " +        
                                    "AND s.type_id = 5";

                    using (var adapter = new MySqlDataAdapter(query, connection))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            catch (Exception ex)
            {
                new ErrorWindow($"Ошибка: {ex.Message}", Manager.theme).ShowDialog();
            }

            return dataTable;
        }

        public DataTable getFreeUnConnectedServices(uint userId)
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT s.id, s.service_name, s.description, s.need_an_address " +
                                   "FROM `services` s " +
                                   "WHERE s.type_id = 5 " +
                                   "AND s.availability = 1 " +
                                   "AND s.id NOT IN " +
                                   "(" +
                                       "SELECT cs.service_id " +
                                       "FROM `concluded_services` cs " +
                                      $"WHERE cs.client_id = {userId}" +
                                   ")";

                    using (var adapter = new MySqlDataAdapter(query, connection))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            catch (Exception ex)
            {
                new ErrorWindow($"Ошибка: {ex.Message}", Manager.theme).ShowDialog();
            }

            return dataTable;
        }
//------------------------------------FREE SERVICES END-------------------------------------------------

//------------------------------------PAID SERVICES-------------------------------------------------
        public DataTable getPaidConnectedServices(uint userId)
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT cs.id, cs.service_id, cs.next_payment, " +
                                          "s.service_name, s.cost, s.description " +
                                    "FROM `concluded_services` cs " +
                                    "LEFT JOIN `services` s " +
                                    "ON cs.service_id = s.id " +
                                   $"WHERE cs.client_id = {userId} " +        
                                    "AND s.type_id = 4";

                    using (var adapter = new MySqlDataAdapter(query, connection))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            catch (Exception ex)
            {
                new ErrorWindow($"Ошибка: {ex.Message}", Manager.theme).ShowDialog();
            }

            return dataTable;
        }

        public DataTable getPaidUnConnectedServices(uint userId)
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT s.id, s.service_name, s.cost, s.description, s.need_an_address " +
                                   "FROM `services` s " +
                                   "WHERE s.type_id = 4 " +
                                   "AND s.availability = 1 " +
                                   "AND s.id NOT IN " +
                                   "(" +
                                       "SELECT cs.service_id " +
                                       "FROM `concluded_services` cs " +
                                      $"WHERE cs.client_id = {userId}" +
                                   ")";

                    using (var adapter = new MySqlDataAdapter(query, connection))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            catch (Exception ex)
            {
                new ErrorWindow($"Ошибка: {ex.Message}", Manager.theme).ShowDialog();
            }

            return dataTable;
        }
//------------------------------------PAID SERVICES END-------------------------------------------------

//------------------------------------EMPLOYEE TABLE INTERACTIONS--------------------------------------- 
        public DataTable getEmployeeTable(uint idForIgnor)
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = $"SELECT e.`id`, e.`login`, e.`email`, e.`status_id`, e.`full_name`, e.`role_id`, e.`phone_number`, " +
                                          $"r.`role_name` " +
                                   $"FROM `employee` e " +
                                   $"LEFT JOIN `role` r " +
                                   $"ON e.`role_id` = r.`id` " +
                                   $"WHERE e.`id` <> {idForIgnor}";

                    using (var adapter = new MySqlDataAdapter(query, connection))
                    {
                        adapter.Fill(dataTable);
                    }
                    
                }
            }
            catch (Exception ex)
            {
                new ErrorWindow($"Ошибка: {ex.Message}", Manager.theme).ShowDialog();
            }

            return dataTable;
        }

        public bool addEmployee(string login, string email, string password, int statusId,
                       string fullName, int roleId, string phoneNumber)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"INSERT INTO `employee` (`login`, `email`, `password`, `status_id`, `full_name`, `role_id`, `phone_number`) 
                                            VALUES (@login, @email, @password, @statusId, @fullName, @roleId, @phoneNumber)";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@login", login);
                        command.Parameters.AddWithValue("@email", email);
                        command.Parameters.AddWithValue("@password", Encrypter.EncryptStringToBytes_AES(password));
                        command.Parameters.AddWithValue("@statusId", statusId);
                        command.Parameters.AddWithValue("@fullName", fullName);
                        command.Parameters.AddWithValue("@roleId", roleId);
                        command.Parameters.AddWithValue("@phoneNumber", phoneNumber);

                        command.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                new ErrorWindow($"Ошибка: {ex.Message}", Manager.theme).ShowDialog();
                return false;
            }
        }
//------------------------------------EMPLOYEE TABLE INTERACTIONS END--------------------------------------- 

//------------------------------------CLIENT TABLE INTERACTIONS---------------------------------------------
        public DataTable getClientTable()
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = $"SELECT `id`, `phone_number`, `email`, `status_id`, `full_name`, `wallet_account`, `internet`, `phone`, `sms` FROM `client`";

                    using (var adapter = new MySqlDataAdapter(query, connection))
                    {
                        adapter.Fill(dataTable);
                    }
                    
                }
            }
            catch (Exception ex)
            {
                new ErrorWindow($"Ошибка: {ex.Message}", Manager.theme).ShowDialog();
            }

            return dataTable;
        }

        public bool addClient(string phoneNumber, string fullName)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"INSERT INTO `client` 
                                    (`phone_number`, `full_name`, `wallet_account`, 
                                     `status_id`, `internet`, `phone`, `sms`) 
                                    VALUES 
                                    (@phoneNumber, @fullName, 0, 
                                     1, 0, 0, 0)";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@phoneNumber", phoneNumber);
                        command.Parameters.AddWithValue("@fullName", fullName);

                        command.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                new ErrorWindow($"Ошибка: {ex.Message}", Manager.theme).ShowDialog();
                return false;
            }
        }
//------------------------------------CLIENT TABLE INTERACTIONS END---------------------------------------------

        public DataTable getConcludedServices()
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"SELECT cs.*, 
                                            st.service_type_name, 
                                            s.service_name, 
                                            cl.id, cl.full_name 
                                    FROM concluded_services cs 
                                    JOIN services s ON cs.service_id = s.id 
                                    JOIN service_type st ON s.type_id = st.id 
                                    JOIN client cl ON cs.client_id = cl.id";

                    using (var adapter = new MySqlDataAdapter(query, connection))
                    {
                        adapter.Fill(dataTable);
                    }
                    
                }
            }
            catch (Exception ex)
            {
                new ErrorWindow($"Ошибка: {ex.Message}", Manager.theme).ShowDialog();
            }

            return dataTable;
        }

        public DataTable getServices()
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"SELECT s.*, 
                                            st.service_type_name 
                                    FROM services s 
                                    JOIN service_type st ON s.type_id = st.id";

                    using (var adapter = new MySqlDataAdapter(query, connection))
                    {
                        adapter.Fill(dataTable);
                    }

                }
            }
            catch (Exception ex)
            {
                new ErrorWindow($"Ошибка: {ex.Message}", Manager.theme).ShowDialog();
            }

            return dataTable;
        }

//-----------------------------------WALLET REPORT----------------------------------------------
        public List<ReplenishmentData> GetClientReplenishments(DateTime startDate, DateTime endDate)
        {
            var result = new List<ReplenishmentData>();

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
                                SELECT 
                                    ca.client_id, 
                                    ca.action, 
                                    ca.time, 
                                    c.full_name,
                                    c.phone_number
                                FROM client_action ca
                                LEFT JOIN client c ON ca.client_id = c.id
                                WHERE ca.time BETWEEN @startDate AND @endDate
                                AND ca.action LIKE 'Пополнил счёт на %'";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@startDate", startDate);
                    command.Parameters.AddWithValue("@endDate", endDate);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string action = reader.GetString("action");
                            DateTime time = reader.GetDateTime("time");
                            uint clientId = reader.GetUInt32("client_id");

                            string clientName = reader.IsDBNull(reader.GetOrdinal("full_name"))
                                ? $"Удалённый клиент (id {clientId})"
                                : reader.GetString("full_name");

                            string clientPhone = reader.IsDBNull(reader.GetOrdinal("phone_number"))
                                ? "-"
                                : reader.GetString("phone_number");

                            decimal amount = ExtractAmountFromAction(action);

                            result.Add(new ReplenishmentData
                            {
                                ReplenishmentType = "Самостоятельное пополнение",
                                ClientName = clientName,
                                ClientPhone = clientPhone ?? "-",
                                EmployeeName = "-",
                                Amount = amount,
                                TransactionDate = time
                            });
                        }
                    }
                }
            }

            return result;
        }

        public List<ReplenishmentData> GetEmployeeReplenishments(DateTime startDate, DateTime endDate)
        {
            var result = new List<ReplenishmentData>();

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
                                SELECT 
                                    ea.employee_id,
                                    ea.action,
                                    ea.time,
                                    e.full_name AS employee_full_name,
                                    c.full_name AS client_full_name,
                                    c.phone_number AS client_phone_number,
                                    SUBSTRING_INDEX(SUBSTRING_INDEX(ea.action, 'id ', -1), ' ', 1) AS extracted_client_id
                                FROM 
                                    employee_action ea
                                LEFT JOIN 
                                    employee e ON ea.employee_id = e.id
                                LEFT JOIN 
                                    client c ON SUBSTRING_INDEX(SUBSTRING_INDEX(ea.action, 'id ', -1), ' ', 1) = c.id
                                WHERE 
                                    ea.action LIKE 'Пополнил счёт клиента id%'
                                    AND ea.time BETWEEN @startDate AND @endDate";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@startDate", startDate);
                    command.Parameters.AddWithValue("@endDate", endDate);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string action = reader.GetString("action");
                            DateTime time = reader.GetDateTime("time");
                            uint employeeId = reader.GetUInt32("employee_id");
                            uint clientId = reader.GetUInt32("extracted_client_id");

                            string employeeName = reader.IsDBNull(reader.GetOrdinal("employee_full_name"))
                                ? $"Удалённый сотрудник (id {employeeId})"
                                : reader.GetString("employee_full_name");

                            string clientName = reader.IsDBNull(reader.GetOrdinal("client_full_name"))
                                ? $"Удалённый клиент (id {clientId})"
                                : reader.GetString("client_full_name");

                            string clientPhone = reader.IsDBNull(reader.GetOrdinal("client_phone_number"))
                                ? "-"
                                : reader.GetString("client_phone_number");

                            decimal amount = ExtractAmountFromAction(action);

                            result.Add(new ReplenishmentData
                            {
                                ReplenishmentType = "Пополнение сотрудником",
                                ClientName = clientName,
                                ClientPhone = clientPhone,
                                EmployeeName = employeeName,
                                Amount = amount,
                                TransactionDate = time
                            });
                        }
                    }
                }
            }

            return result;
        }

        private decimal ExtractAmountFromAction(string action)
        {
            try
            {
                int startIndex = action.IndexOf(" на ") + 3;
                int endIndex = action.IndexOf(" руб.", startIndex);

                string amountStr = action.Substring(startIndex, endIndex - startIndex).Trim();
                return decimal.Parse(amountStr, CultureInfo.InvariantCulture);
            }
            catch (Exception ex) 
            {
                new ErrorWindow($"Ошибка: {ex.Message}", Manager.theme).ShowDialog();
            }
            
            return 0;
        }
//-----------------------------------WALLET REPORT END----------------------------------------------
    }
}
