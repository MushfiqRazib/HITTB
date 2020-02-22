using System;
using System.Collections;
using System.Data;
using System.Configuration;
using System.Web;
using System.Data.OleDb;
using Npgsql;


namespace HIT.PEditor.Core
{
    public class OracleDBHandler : IDatabaseFunctionsManager
    {
        private string connectionString = null;

        public OracleDBHandler() : this(ConfigurationManager.ConnectionStrings["DatabaseConnection_Wrapper"].ToString()) { }

        public OracleDBHandler(string connectionstr)
        {
            connectionString = connectionstr;
        }
        
        public string GetQuery(string tableName,string groupName)
        {
            //string tableName = ConfigurationManager.AppSettings["metatable"].ToString();
            ArrayList fieldList = GetFieldList(tableName);
            string fields = string.Empty;
            string fieldName;
            foreach (Object field in fieldList)
            {
                fieldName = field.ToString();
                fields += "\"" + field.ToString().ToUpper() + "\",";
            }

            fields = fields.Substring(0, fields.LastIndexOf(','));

            return "select " + fields + " from " + tableName + " where \"GROUPNAME\"='" + groupName + "'";
        }

        public ArrayList GetFieldList(string tableName)
        {
            string query = "SELECT * FROM " + tableName + " WHERE 1 = 0";
            DataTable dt = GetDataTable(query);
            ArrayList fieldList = new ArrayList();
            for (int k = 0; k < dt.Columns.Count; k++)
            {
                fieldList.Add(dt.Columns[k]);
            }
            return fieldList;
        }

        private DataTable GetDataTable(string query, OleDbConnection dbConnection)
        {
            DataTable dataTable = new DataTable();
            try
            {
                //LogWriter.WriteLog("query: " + query);
                OleDbDataAdapter adapter = new OleDbDataAdapter();
                adapter.SelectCommand = new OleDbCommand(query, dbConnection);
                adapter.Fill(dataTable);
            }
            catch (Exception ex)
            {
                throw new Exception("From GetDataTable method:" + ex.Message);
            }
            return dataTable;
        }

        /// <summary>
        /// Standard function to do a select-query
        /// </summary>
        /// <param name="sqlQuery">Query to execute</param>
        /// <returns>DataTable with the query's results</returns>
        public DataTable GetDataTable(string sqlQuery)
        {
            DataTable datatable = new DataTable();

            try
            {
                using (OleDbConnection objCon = new OleDbConnection(connectionString))
                {
                    OleDbDataAdapter adapter = new OleDbDataAdapter();
                    adapter.SelectCommand = new OleDbCommand(sqlQuery, objCon);
                    adapter.Fill(datatable);
                    adapter.Dispose();
                    adapter = null;
                }
            }
            catch (Exception ex)
            {
                //LogWriter.WriteLog(ex.StackTrace);
                throw new Exception("CONN_STR_ERROR");
            }

            return datatable;
        }

       

    }


}


