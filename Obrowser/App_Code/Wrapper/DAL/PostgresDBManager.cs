using System;
using System.Data;
using Npgsql;
using System.Configuration;
using HIT.OB.STD.Core;
using System.Text.RegularExpressions;

/// <summary>
/// Summary description for PostgresDBHandler
/// </summary>

namespace HIT.OB.STD.Wrapper.DAL
{
    public class PostgresDBManager : IWrapFunctions
    {
        private static string ConnectionString
        {
            get { return ConfigManager.GetConnectionString(); }
        }
       
        private int DBConnectionTimeout
        {
            get
            {
                return Int32.Parse(ConfigurationManager.AppSettings["dbConnectionTimeout"]);
            }
        }

        public DataTable GetReportList()
        {
            string query = "select report_code, report_name from " + ConfigManager.GetReportTableName() + " order by report_order;";
            DataTable dtReportList = GetDataTable(query);
            return dtReportList;
        }

        public DataTable GetReportArguments(string reportCode)
        {
            try
            {
                string query = "select * from " + ConfigManager.GetReportTableName() + " where upper(report_code) ='" + reportCode.ToUpper() + "'";
                LogWriter.WriteLog(query);
                DataTable dtReportArgs = GetDataTable(query);
                return dtReportArgs;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetReportFieldList(string tableName)
        {
            try
            {
                string query = "SELECT * FROM " + tableName + " WHERE 1 = 0";
                DataTable dtFieldList = GetDataTable(query);
                return dtFieldList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetReportFunctionsList(string reportCode)
        {
            try
            {
                string query = "select * from " + ConfigurationManager.AppSettings["reportfunctionstable"] + " where report_code ='" + reportCode.ToUpper() + "' order by order_position";
                LogWriter.WriteLog(query);
                DataTable dtFunReportArgs = GetDataTable(query);
                return dtFunReportArgs;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public DataTable GetDataTable(string query)
        {
            DataTable dataTable = new DataTable();
            try
            {
                using (NpgsqlConnection dbConnection = new NpgsqlConnection(ConnectionString))
                {
                    NpgsqlDataAdapter adapter = new NpgsqlDataAdapter();
                    adapter.SelectCommand = new NpgsqlCommand(query, dbConnection);
                    adapter.SelectCommand.CommandTimeout = DBConnectionTimeout;
                    adapter.Fill(dataTable);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("From GetDataTable method:" + ex.Message);
            }
            return dataTable;
        }
        
        public bool UpdateUserDefinedReportSettings(string REPORT_CODE, string SQL_WHERE, string GROUP_BY, string ORDER_BY, string ORDER_BY_DIR, string report_settings)
        {
            string updateQuery = @"update dfn_repdetail set 
                                   report_settings=:report_settings,
                                   sql_where=:sql_where,sql_groupby=:sql_groupby,
                                   sql_orderby=:sql_orderby,sql_orderdir=:sql_orderdir Where REPORT_CODE=:REPORT_CODE";
            using (NpgsqlConnection con = new NpgsqlConnection(ConnectionString))
            {
                using (NpgsqlCommand updateCmd = new NpgsqlCommand(updateQuery, con))
                {
                    updateCmd.Parameters.Add("report_settings", report_settings);
                    updateCmd.Parameters.Add("sql_where", SQL_WHERE);
                    updateCmd.Parameters.Add("sql_groupby", GROUP_BY);
                    updateCmd.Parameters.Add("sql_orderby", ORDER_BY);
                    updateCmd.Parameters.Add("sql_orderdir", ORDER_BY_DIR);
                    updateCmd.Parameters.Add("REPORT_CODE", REPORT_CODE);
                    updateCmd.Connection.Open();
                    updateCmd.ExecuteNonQuery();
                    updateCmd.Connection.Close();
                }
            }

            return true;
        }
          
        public DataRow GetReportConfigInfo(string reportCode)
        {
            DataTable dataTable = new DataTable();
            try
            {
                string query = "select field_caps, sql_from,detail_fieldsets from " + ConfigManager.GetReportTableName() + " where upper(report_code) ='" + reportCode.ToUpper() + "'";
                using (NpgsqlConnection dbConnection = new NpgsqlConnection(ConnectionString))
                {
                    using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter())
                    {
                        adapter.SelectCommand = new NpgsqlCommand(query, dbConnection);
                        adapter.Fill(dataTable);
                        return dataTable.Rows[0];
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("From GetDataTable method:" + ex.Message);
            }
            return null;
        }

        public static void ExecuteTransaction(NpgsqlCommand[] commands)
        {
            NpgsqlTransaction transaction = null;
            NpgsqlConnection conn;
            conn = new NpgsqlConnection(PostgresDBManager.ConnectionString);

            try
            {
                conn.Open();
                transaction = conn.BeginTransaction();
                foreach (NpgsqlCommand command in commands)
                {
                    command.Connection = conn;
                    command.Transaction = transaction;
                    command.ExecuteNonQuery();
                }
                transaction.Commit();
            }
            catch (NpgsqlException ex)
            {
                throw new Exception("ERROR: " + ex.Code + "<br>" + "ERROR Message: " + ex.Message);
                transaction.Rollback();
            }
            finally
            {
                conn.Close();
            }
        }

        public string GetConnectionStringForReport(string REPORT_CODE)
        {
            string query = "select connection_string from " + ConfigManager.GetReportTableName() + " Where report_code='" + REPORT_CODE + "'";
            DataTable dtConSring = GetDataTable(query);
            if (dtConSring.Rows.Count > 0 && !dtConSring.Rows[0][0].ToString().Trim().Equals(string.Empty))
            {
                return dtConSring.Rows[0][0].ToString();
            }
            else
            {
                return string.Empty;
            }
        }
               
        public bool InsertGroupColor(string REPORT_CODE, string GROUP_BY, string GROUP_CODE, string COLOR_CODE)
        {
            bool IsInserted = false;
            string TableName = ConfigurationManager.AppSettings["groupcolortable"];
            GROUP_CODE = GROUP_CODE.Trim(',');
            COLOR_CODE = COLOR_CODE.Trim(',');
            string[] groupList = GROUP_CODE.Split(',');
            string[] colorList = COLOR_CODE.Split(',');
            NpgsqlCommand commands = new NpgsqlCommand();
            string deleteQueryValues = "(";
            for (int i = 0; i < groupList.Length; i++)
            {
                deleteQueryValues += "'" + REPORT_CODE + "'||'" + GROUP_BY +
                                    "'||'" + groupList[i] + "',";
            }
            deleteQueryValues = deleteQueryValues.Remove(deleteQueryValues.LastIndexOf(','), 1);
            deleteQueryValues += ")";
            NpgsqlConnection conn = new NpgsqlConnection(ConnectionString);

            try
            {
                conn.Open();
                string deleteSQL = "delete from " + TableName + " where reportcode || groupby || groupcode in " +
                                        deleteQueryValues;

                commands = new NpgsqlCommand(deleteSQL, conn);
                commands.ExecuteNonQuery();

                for (int i = 0; i < groupList.Length; i++)
                {

                    string insertSQL = " insert into " + TableName +
                                        " (reportcode,groupby,groupcode,colorcode) " +
                                        " values(:reportcode,:groupby,:groupcode,:colorcode) ";

                    commands = new NpgsqlCommand(insertSQL, conn);
                    commands.Parameters.Add("reportcode", REPORT_CODE);
                    commands.Parameters.Add("groupby", GROUP_BY);
                    commands.Parameters.Add("groupcode", groupList[i]);
                    commands.Parameters.Add("colorcode", colorList[i]);
                    commands.ExecuteNonQuery();
                }
                IsInserted = true;
            }
            catch (Exception ex)
            {
                IsInserted = false;
                throw new Exception("From GetDataTable method:" + ex.Message);
            }
            finally
            {
                conn.Close();
            }
            return IsInserted;
        }

        public string DeleteRecord(string keyInfo, string tableName)
        {
            string query = "delete from " + tableName + " Where ";
            string[] keyInfoKeyValues = keyInfo.Split('$');
            string[] keyNames = keyInfoKeyValues[0].Split(';');
            string[] keyValues = keyInfoKeyValues[1].Split(';');
            string clause = string.Empty;
            for (int i = 0; i < keyNames.Length; i++)
            {
                clause = keyNames[i] + " = '" + keyValues[i] + "'";
                query += (i < keyNames.Length - 1) ? clause + " AND " : clause;
            }
            using (NpgsqlConnection con = new NpgsqlConnection(ConnectionString))
            {
                con.Open();
                using (NpgsqlCommand deleteCmd = new NpgsqlCommand(query, con))
                {
                    deleteCmd.ExecuteNonQuery();                    
                }
            }
            return "true";

        }

        public string AddUser(string userCode, string userName, string fullName, string address, string city, string phoneNr,
                           string mobile, string skype, string birthDay, string _function, string joinedHawarit, string status,
                           string internNr, string comment, string department, string email, string nickName)
        {
            string insertQuery = @"insert into employee (usercode,username,full_name,address,city,phonenr,mobile,skype,birthday,
                        function,joined_hawarit,status,intern_nr,comment,department,email,nickname)
                        values(@usercode,@username,@full_name,@address,@city,@phonenr,@mobile,@skype,@birthday,
                        @function,@joined_hawarit,@status,@intern_nr,@comment,@department,@email,@nickname)";    
            

            using (NpgsqlConnection con = new NpgsqlConnection(ConnectionString))
            {
                using (NpgsqlCommand insertCmd = new NpgsqlCommand(insertQuery, con))
                {
                    insertCmd.Parameters.Add("usercode", userCode);
                    insertCmd.Parameters.Add("username", userName);
                    insertCmd.Parameters.Add("full_name", fullName);
                    insertCmd.Parameters.Add("address", address);
                    insertCmd.Parameters.Add("city", city);
                    insertCmd.Parameters.Add("phonenr", phoneNr);
                    insertCmd.Parameters.Add("mobile", mobile);
                    insertCmd.Parameters.Add("skype", skype);
                    insertCmd.Parameters.Add("birthday", birthDay);
                    insertCmd.Parameters.Add("function", _function);
                    insertCmd.Parameters.Add("joined_hawarit", joinedHawarit);
                    insertCmd.Parameters.Add("status", status);
                    insertCmd.Parameters.Add("intern_nr", internNr);
                    insertCmd.Parameters.Add("comment", comment);
                    insertCmd.Parameters.Add("department", department);
                    insertCmd.Parameters.Add("email", email);
                    insertCmd.Parameters.Add("nickname", nickName);
                    insertCmd.Parameters.Add("full_name", address);
                    insertCmd.Connection.Open();
                    insertCmd.ExecuteNonQuery();
                    insertCmd.Connection.Close();
                }
            }

            return "true";
        }
        public DataTable GetProjectCodes()
        {
            string projectCode = "select projcode from projects order by projcode";
            return GetDataTable(projectCode);
        }
        
        public string GetRelativePath(string reportName, string fileName)
        {
            DataTable dataTable = new DataTable();
            string _sql = "SELECT sql_from FROM dfn_repdetail WHERE report_code = '" + reportName + "'";

            try
            {
                DataTable dt = GetDataTable(_sql);

                if (dt.Rows.Count > 0)
                {
                    _sql = "SELECT relfilename FROM " + dt.Rows[0]["sql_from"].ToString() + "  WHERE LOWER(relfilename) LIKE '%" + fileName.ToLower() + "'";
                    //LogWriter.WriteLog(_sql);
                    dataTable = GetDataTable(_sql);
                    if (dataTable.Rows.Count > 0)
                    {
                        return dataTable.Rows[0]["relfilename"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return "";
        }

        public string GetInfoForThisFile(string viewName, string fileName)
        {
            DataTable dataTable = new DataTable();
            string _sql = string.Empty;

            try
            {
                _sql = "SELECT rev, description, relfilename FROM " + viewName + "  WHERE LOWER(relfilename) LIKE '%" + fileName.ToLower() + "'";
                LogWriter.WriteLog("File Info: " + _sql);
                dataTable = GetDataTable(_sql);
                LogWriter.WriteLog("File Info: " + _sql + "@@" + dataTable.Rows.Count.ToString());
                if (dataTable.Rows.Count > 0)
                {
                    return "revision::" + dataTable.Rows[0]["rev"].ToString() + "##description::" + dataTable.Rows[0]["description"].ToString() + "##relfilename::" + dataTable.Rows[0]["relfilename"].ToString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return "";
        }

        public string UpdateFileInfo(string information, string viewName, string relfilename)
        {
            DataTable dataTable = new DataTable();
            DataTable dt = new DataTable();
            string _sql = "SELECT sql_keyfields FROM dfn_repdetail WHERE sql_from = :viewName";
            string primarykey = string.Empty;
            string[] primaryKeys = null;

            try
            {
                using (NpgsqlConnection dbConnection = new NpgsqlConnection(ConnectionString))
                {
                    dbConnection.Open();
                    using (NpgsqlCommand command = new NpgsqlCommand(_sql))
                    {
                        command.Parameters.Add("viewName", viewName);
                        command.Connection = dbConnection;

                        primarykey = command.ExecuteScalar().ToString();

                    }

                    primaryKeys = primarykey.Split(new char[] { ',', ';' });

                    _sql = "SELECT ";

                    foreach (string pKey in primaryKeys)
                    {
                        _sql = _sql + pKey + ", ";
                    }

                    _sql = _sql.Trim().TrimEnd(new char[] { ',' });
                    _sql = _sql + " FROM " + viewName;
                    _sql = _sql + " WHERE LOWER(relfilename) LIKE '%" + relfilename.ToLower() + "'";

                    NpgsqlDataAdapter adapter = new NpgsqlDataAdapter();
                    adapter.SelectCommand = new NpgsqlCommand(_sql, dbConnection);
                    adapter.Fill(dataTable);



                    string[] info = Regex.Split(information, "##");
                    if (dataTable.Rows.Count > 0)
                    {
                        using (NpgsqlCommand command1 = new NpgsqlCommand())
                        {
                            _sql = "UPDATE " + viewName.Replace("_v", "") + " SET rev = '" + info[0] +
                                "', description='" + info[1] + "', title='" + info[2] +
                                "', author='" + info[3] + "', filename='" + info[4] + "', file_fullname='" + info[5] + "' WHERE ";
                          
                            foreach (string pKey in primaryKeys)
                            {
                                _sql = _sql + pKey + " = :" + pKey + ", ";
                                command1.Parameters.Add(pKey, dataTable.Rows[0][pKey]);
                            }

                            _sql = _sql.Trim().TrimEnd(new char[] { ',' });


                            command1.CommandText = _sql;
                            command1.Connection = dbConnection;
                            int a = command1.ExecuteNonQuery();
                        }
                        using (NpgsqlCommand command2 = new NpgsqlCommand())
                        {
                            _sql = "SELECT relfilename FROM " + viewName + " WHERE ";

                            foreach (string pKey in primaryKeys)
                            {
                                _sql = _sql + pKey + " = :" + pKey + ", ";
                                command2.Parameters.Add(pKey, dataTable.Rows[0][pKey]);
                            }

                            _sql = _sql.Trim().TrimEnd(new char[] { ',' });


                            command2.CommandText = _sql;
                            command2.Connection = dbConnection;
                            //NpgsqlDataAdapter adapter1 = new NpgsqlDataAdapter();
                            adapter.SelectCommand = command2;
                            adapter.Fill(dt);
                        }


                        if (dt.Rows.Count > 0)
                        {
                            return dt.Rows[0]["relfilename"].ToString();
                        }
                    }

                    dbConnection.Close();
                }

            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "success";
        }

        public string SaveFileInfo(string information, string viewName, string reference)
        {
            DataTable dataTable = new DataTable();
            string primarykey = string.Empty;
            //string[] primaryKeys = null;

            string _sql = "SELECT sql_keyfields FROM dfn_repdetail WHERE sql_from = :viewName";

            try
            {
                using (NpgsqlConnection dbConnection = new NpgsqlConnection(ConnectionString))
                {
                    dbConnection.Open();
                    using (NpgsqlCommand command = new NpgsqlCommand(_sql))
                    {
                        command.Parameters.Add("viewName", viewName);
                        command.Connection = dbConnection;

                        primarykey = command.ExecuteScalar().ToString();

                    }

                    string[] primaryKeys = primarykey.Split(new char[] { ',', ';' });
                    string[] info = Regex.Split(information, "##");
                    _sql = "INSERT INTO " + viewName.Replace("_v", "") + "(" + primaryKeys[0] + 
                        ", rev, description, title, author, filename, file_fullname) VALUES (:reference ,'" +
                        info[0] + "','" + info[1] + "','" + info[2] + "','" + info[3] + "','" + info[4] + "','" + info[5] + "')";
                   
                    //_sql = "INSERT INTO :tableName (:pKey, rev, description) VALUES (:reference, :rev, :description)";

                    using (NpgsqlCommand command = new NpgsqlCommand(_sql))
                    {
                        command.Parameters.Add("reference", reference);
                        command.Connection = dbConnection;
                        int a = command.ExecuteNonQuery();
                    }

                    _sql = "SELECT relfilename FROM " + viewName + " WHERE " + primaryKeys[0] + " = :reference";

                    using (NpgsqlCommand command = new NpgsqlCommand(_sql))
                    {
                        command.Parameters.Add("reference", reference);
                        command.Connection = dbConnection;
                        NpgsqlDataAdapter adapter = new NpgsqlDataAdapter();
                        adapter.SelectCommand = command;
                        adapter.Fill(dataTable);
                    }
                    dbConnection.Close();

                    if (dataTable.Rows.Count > 0)
                    {
                        return dataTable.Rows[0]["relfilename"].ToString();
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                return ex.Message;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }


            return "";
        }

        public string DeleteFileInfo(string viewName, string relfilename)
        {
            DataTable dataTable = new DataTable();
            DataTable dt = new DataTable();
            string _sql = "SELECT sql_keyfields FROM dfn_repdetail WHERE sql_from = :viewName";
            string primarykey = string.Empty;
            string[] primaryKeys = null;

            try
            {
                using (NpgsqlConnection dbConnection = new NpgsqlConnection(ConnectionString))
                {
                    dbConnection.Open();
                    using (NpgsqlCommand command = new NpgsqlCommand(_sql))
                    {
                        command.Parameters.Add("viewName", viewName);
                        command.Connection = dbConnection;

                        primarykey = command.ExecuteScalar().ToString();

                    }

                    primaryKeys = primarykey.Split(new char[] { ',', ';' });

                    _sql = "SELECT ";

                    foreach (string pKey in primaryKeys)
                    {
                        _sql = _sql + pKey + ", ";
                    }

                    _sql = _sql.Trim().TrimEnd(new char[] { ',' });
                    _sql = _sql + " FROM " + viewName;
                    _sql = _sql + " WHERE LOWER(relfilename) LIKE '%" + relfilename.ToLower().Split(new char[] { '\\' })[1] + "%'";

                    NpgsqlDataAdapter adapter = new NpgsqlDataAdapter();
                    adapter.SelectCommand = new NpgsqlCommand(_sql, dbConnection);
                    adapter.Fill(dataTable);

                    if (dataTable.Rows.Count > 0)
                    {
                        using (NpgsqlCommand command = new NpgsqlCommand())
                        {
                            _sql = "DELETE FROM " + viewName.Replace("_v", "") + " WHERE ";

                            foreach (string pKey in primaryKeys)
                            {
                                _sql = _sql + pKey + " = :" + pKey + ", ";
                                command.Parameters.Add(pKey, dataTable.Rows[0][pKey]);
                            }

                            _sql = _sql.Trim().TrimEnd(new char[] { ',' });


                            command.CommandText = _sql;
                            command.Connection = dbConnection;
                            int a = command.ExecuteNonQuery();
                        }
                    }

                    dbConnection.Close();
                }

            }
            catch (NpgsqlException ex)
            {
                return ex.Message;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "success";
        }

    }
}
