using System;
using System.Collections;
using System.Data;
using System.Configuration;
using System.Web;
using Npgsql;
using System.Xml.Linq;
using System.Linq;


namespace HIT.PEditor.Core
{
    public class PgsqlDBHandler : IDatabaseFunctionsManager
    {
        private string connectionString = null;

        public PgsqlDBHandler() : this(ConfigurationManager.ConnectionStrings["DatabaseConnection_Wrapper"].ToString()) { }

        public PgsqlDBHandler(string connectionstr)
        {            
            connectionString = connectionstr;
        }


        public string GetQuery(string tableName,string groupName)
        {
            //string tableName = ConfigurationManager.AppSettings["metatable"].ToString();
            ArrayList fieldList = GetFieldList(tableName);
            string fiels = string.Empty;
            foreach (Object field in fieldList)
            {
                fiels += "\"" + field + "\",";
            }

            fiels = fiels.Substring(0, fiels.LastIndexOf(','));
            return "select " + fiels + " from " + tableName + " where groupname='" + groupName + "'";
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
                using (NpgsqlConnection objCon = new NpgsqlConnection(connectionString))
                {
                    NpgsqlDataAdapter adapter = new NpgsqlDataAdapter();
                    adapter.SelectCommand = new NpgsqlCommand(sqlQuery, objCon);
                    adapter.Fill(datatable);
                    adapter.Dispose();
                    adapter = null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("CONN_STR_ERROR");
            }

            return datatable;
        }

       

       
       
    }

}
