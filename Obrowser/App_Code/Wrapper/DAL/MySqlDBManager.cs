using System;
using System.Data;
using System.Configuration;
using HIT.OB.STD.Core;
using MySql.Data.MySqlClient;

/// <summary>
/// Summary description for MySqlDBManager
/// </summary>

namespace HIT.OB.STD.Wrapper.DAL
{
    public class MySqlDBManager : IWrapFunctions
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
                using (MySqlConnection dbConnection = new MySqlConnection(ConnectionString))
                {
                    MySqlDataAdapter adapter = new MySqlDataAdapter();
                    adapter.SelectCommand = new MySqlCommand(query, dbConnection);
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
            try
            {
                string updateQuery = @"update dfn_repdetail set 
                                   report_settings=?report_settings,
                                   sql_where=?sql_where,sql_groupby=?sql_groupby,
                                   sql_orderby=?sql_orderby,sql_orderdir=?sql_orderdir Where REPORT_CODE=?REPORT_CODE";
                using (MySqlConnection con = new MySqlConnection(ConnectionString))
                {
                    using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, con))
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
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

       

        

        public DataRow GetReportConfigInfo(string reportCode)
        {
            DataTable dataTable = new DataTable();
            try
            {
                string query = "select field_caps, sql_from,detail_fieldsets from " + ConfigManager.GetReportTableName() + " where upper(report_code) ='" + reportCode.ToUpper() + "'";
                using (MySqlConnection dbConnection = new MySqlConnection(ConnectionString))
                {
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter())
                    {
                        adapter.SelectCommand = new MySqlCommand(query, dbConnection);
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

        public static void ExecuteTransaction(MySqlCommand[] commands)
        {
            MySqlTransaction transaction = null;
            MySqlConnection conn;
            conn = new MySqlConnection(MySqlDBManager.ConnectionString);

            try
            {
                conn.Open();
                transaction = conn.BeginTransaction();
                foreach (MySqlCommand command in commands)
                {
                    command.Connection = conn;
                    command.Transaction = transaction;
                    command.ExecuteNonQuery();
                }
                transaction.Commit();
            }
            catch (MySqlException ex)
            {
                throw new Exception("ERROR: " + ex.Number + "<br>" + "ERROR Message: " + ex.Message);
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
            GROUP_CODE = GROUP_CODE.Remove(GROUP_CODE.LastIndexOf(','), 1);
            COLOR_CODE = COLOR_CODE.Remove(COLOR_CODE.LastIndexOf(','), 1);
            string[] groupList = GROUP_CODE.Split(',');
            string[] colorList = COLOR_CODE.Split(',');
            MySqlCommand commands = new MySqlCommand();
            string deleteQueryValues = "(";
            for (int i = 0; i < groupList.Length; i++)
            {
                deleteQueryValues += "'" + REPORT_CODE + "'||'" + GROUP_BY +
                                    "'||'" + groupList[i] + "',";
            }
            deleteQueryValues = deleteQueryValues.Remove(deleteQueryValues.LastIndexOf(','), 1);
            deleteQueryValues += ")";
            MySqlConnection conn = new MySqlConnection(ConnectionString);

            try
            {
                conn.Open();
                string deleteSQL = "delete from " + TableName + " where reportcode || groupby || groupcode in " +
                                        deleteQueryValues;

                commands = new MySqlCommand(deleteSQL, conn);
                commands.ExecuteNonQuery();

                for (int i = 0; i < groupList.Length; i++)
                {

                    string insertSQL = " insert into " + TableName +
                                        " (reportcode,groupby,groupcode,colorcode) " +
                                        " values(:reportcode,:groupby,:groupcode,:colorcode) ";

                    commands = new MySqlCommand(insertSQL, conn);
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
            throw new Exception("Not implemented yet");
        }

        public string AddUser(string userCode, string userName, string fullName, string address, string city, string phoneNr,
                           string mobile, string skype, string birthDay, string _function, string joinedHawarit, string status,
                           string internNr, string comment, string department, string email, string nickName)
        {
            throw new Exception("Not implemented yet");
        }
        public DataTable GetProjectCodes()
        {
            throw new Exception("Not implemented yet");
        }

        public string GetRelativePath(string reportName, string fileName)
        {
            throw new Exception("Not implemented for this database !");
        }

        public string GetInfoForThisFile(string reportName, string fileName)
        {
            throw new Exception("Not implemented for this database !");
        }

        public string UpdateFileInfo(string information, string viewName, string relfilename)
        {

            throw new Exception("Not implemented for this database !");
        }

        public string SaveFileInfo(string information, string viewName, string reference)
        {
            throw new Exception("Not implemented for this database !");
        }

        public string DeleteFileInfo(string viewName, string relfilename)
        {
            throw new Exception("Not implemented for this database !");
        }
      

    }
}
