using System;
using System.Data;
using Npgsql;
using HIT.OB.STD.Wrapper.BLL;

/// <summary>
/// Summary description for DBHandler
/// </summary>
/// 
namespace HIT.OB.STD.Wrapper.DAL
{
    public class Security_Ext_DBHandler
    {
        public Security_Ext_DBHandler()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public static string ConnectionString
        {
            get
            {
                return ConfigManager.GetConnectionString();
            }
        }

        public static void InsertNewSecurityInfo(string userId, string securityId, 
                                string authString, string print_server_location, string kit_server_url,string docSharePath)
        {
            string sql = string.Empty;
            string loginStatus = string.Empty;
            DataTable activeMapExtent = new DataTable("securityinfo");
            try
            {
                string timeout = ConfigManager.GetLoginTimeout();
                using (NpgsqlConnection connection = new NpgsqlConnection(Security_Ext_DBHandler.ConnectionString))
                {
                    connection.Open();
                    sql = @"insert into securityinfo(userid,timeout, securityid,printserver_location,last_access_time,authentication_values,kitserverurl,docsharepath) values(@userId, @timeout, @securityid, @printServerlocation,now(),@authValues,@kit_server_url,@docSharePath)";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, connection))
                    {
                        cmd.Parameters.Add("userId", userId);
                        cmd.Parameters.Add("timeout", timeout);
                        cmd.Parameters.Add("securityid", securityId);                        
                        cmd.Parameters.Add("printServerlocation", print_server_location);
                        cmd.Parameters.Add("kit_server_url", kit_server_url);
                        cmd.Parameters.Add("docSharePath", docSharePath);
                        cmd.Parameters.Add("authValues", authString);
                        cmd.ExecuteNonQuery();
                    }
                    connection.Close();
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public static bool GetRoleValidationStatus(string roleName, string rolePass)
        {
            string sql = string.Empty;
            bool retCode = false;
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(Security_Ext_DBHandler.ConnectionString))
                {
                    connection.Open();
                    sql = @"select case when count(*)>0 then true else false end from hitcon_role_def where upper(rolename) = upper(@rolename) AND rolepass= @rolepass";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, connection))
                    {
                        cmd.Parameters.Add("rolename", roleName);
                        cmd.Parameters.Add("rolepass", rolePass);
                        retCode = bool.Parse(cmd.ExecuteScalar().ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return retCode;
        }

        public static bool GetKitServerURLWithValidityCheck(string securityId, out string url)
        {
            DataTable dt = Security_Ext_DBHandler.GetUserSessionInfo(securityId);
            url = "";
            if (dt.Rows.Count > 0)
            {
                dt.Rows[0]["kitserverurl"].ToString();
                Double idleTimeInSecond = Convert.ToDouble(dt.Rows[0]["timediff"]);
                Double timeout = Convert.ToDouble(dt.Rows[0]["timeout"]);
                if (timeout >= idleTimeInSecond)
                {
                    SecurityManager.UpdateLastAccessTime(securityId);
                    return true;
                }
                else
                {
                    SecurityManager.DeleteSecurityKeyInfo(securityId);
                    return false;
                }
            }
            return false;
        }

      
        internal static string[] GetAuthenticationValues(string securityKey)
        {
            string sql = string.Empty;
            DataTable dt = new DataTable();
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(Security_Ext_DBHandler.ConnectionString))
                {
                    connection.Open();
                    sql = @"select authentication_values from securityinfo where securityid = @securityKey;";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, connection))
                    {
                        cmd.Parameters.Add("securityKey", securityKey);
                        NpgsqlDataAdapter adapt = new NpgsqlDataAdapter(cmd);
                        adapt.Fill(dt);

                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }


            if (dt.Rows.Count > 0)
            {
                return new string[] { dt.Rows[0]["authentication_values"].ToString()};
            }
            return null;
        }

        public static DataTable GetUserSessionInfo(string securityKey)
        {
            string sql = string.Empty;
            DataTable dt = new DataTable();
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(Security_Ext_DBHandler.ConnectionString))
                {
                    connection.Open();
                    sql = @"select timeout, getdifftsinsec(last_access_time) as timediff, kitserverurl from securityinfo where securityid = @securityKey";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, connection))
                    {
                        cmd.Parameters.Add("securityKey", securityKey);
                        NpgsqlDataAdapter adapt = new NpgsqlDataAdapter(cmd);
                        adapt.Fill(dt);
                    }
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public static void UpdateLastAccessTime(string securityKey)
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(Security_Ext_DBHandler.ConnectionString))
                {
                    connection.Open();
                    string sql = @"update securityinfo set last_access_time = now() where securityid = @securityKey";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, connection))
                    {
                        cmd.Parameters.Add("securityKey", securityKey);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        internal static void DeleteSecurityKeyInfo(string securityKey)
        {
            string sql = string.Empty;
            DataTable activeMapExtent = new DataTable("securityinfo");
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(Security_Ext_DBHandler.ConnectionString))
                {
                    connection.Open();
                    sql = @"delete from securityinfo where securityid = @securityKey;";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, connection))
                    {
                        cmd.Parameters.Add("securityKey", securityKey);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public static string GetPrintServerLocation(string securityKey)
        {
            string sql = string.Empty;
            DataTable dt = new DataTable();
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(Security_Ext_DBHandler.ConnectionString))
                {
                    connection.Open();
                    sql = @"select printserver_location from securityinfo where securityid = @securityKey";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, connection))
                    {
                        cmd.Parameters.Add("securityKey", securityKey);
                        NpgsqlDataAdapter adapt = new NpgsqlDataAdapter(cmd);
                        adapt.Fill(dt);
                    }

                    connection.Close();
                }


            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            if (dt.Rows.Count > 0)
            {
                return dt.Rows[0]["printserver_location"].ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        public static string[] GetKitServerInfo(string securityKey)
        {
            string sql = string.Empty;
            DataTable dt = new DataTable();
            string[] retData = new string[3];
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(Security_Ext_DBHandler.ConnectionString))
                {
                    connection.Open();
                    sql = @"select kitserverurl,printserver_location,docSharePath from securityinfo where securityid = @securityKey";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, connection))
                    {
                        cmd.Parameters.Add("securityKey", securityKey);
                        NpgsqlDataAdapter adapt = new NpgsqlDataAdapter(cmd);
                        adapt.Fill(dt);
                    }

                    connection.Close();
                }


            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            if (dt.Rows.Count > 0)
            {
                retData[0] = dt.Rows[0]["kitserverurl"].ToString();
                retData[1] = dt.Rows[0]["printserver_location"].ToString();
                retData[2] = dt.Rows[0]["docSharePath"].ToString();
                return retData;
            }
            else
            {
                return null;
            }
        }

        internal static DataTable GetAuthenticationInfo(string roleName)
        {
            string sql = string.Empty;
            DataTable dt = new DataTable();
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(Security_Ext_DBHandler.ConnectionString))
                {
                    connection.Open();
                    sql = @"select  distinct authsection,functionname
                            from hitcon_auth_def ad
                            where lower(rolename) = lower(@rolename)";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, connection))
                    {
                        cmd.Parameters.Add("rolename", roleName);
                        NpgsqlDataAdapter adapt = new NpgsqlDataAdapter(cmd);
                        adapt.Fill(dt);
                    }
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

       

    }
}
