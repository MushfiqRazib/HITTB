using System;
using System.Data;
using System.Configuration;
using Npgsql;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Summary description for PostgresDBHandler
/// </summary>

namespace HIT.OB.STD.Core.DAL
{
    public class PostgresDBManager : IOBFunctions
    {

        private string connectionString;
        private int DBConnectionTimeout
        {
            get
            {
                return Int32.Parse(ConfigurationManager.AppSettings["dbConnectionTimeout"]);
            }
        }
        public PostgresDBManager(string conString)
        {
            this.connectionString = conString;
        }

        private string ConnectionString
        {
            get { return connectionString; }
            set { connectionString = value; }
        }
		
		
        public DataTable GetItemData(string sqlFrom, string keyList, string valueList)
        {
            string whereClause = MakeWhereClauseFromKeyValues(keyList, valueList);
            string query = string.Format("SELECT * FROM {0} WHERE {1}", sqlFrom, whereClause);
            return GetDataTable(query);
        }
		
		private string GetCaseInsensitiveWhere(string sqlWhere, string tableName)
        {
            string[] fieldTypes = GetFieldNameType(tableName).Split('|');
            string outerPattern = @"( AND )|( OR )";
            string newTgtString;
            StringBuilder newWhere = new StringBuilder();
            string[] conditions = Regex.Split(sqlWhere, outerPattern, RegexOptions.IgnoreCase);
            foreach (string condString in conditions)
            {
                newTgtString = condString.Trim();
                string innerPattern = @"(<>)|(=)|(<=)|(>=)|(<)|(>)|(\sLIKE\s)";
                string[] condItems = Regex.Split(newTgtString, innerPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (condItems.Length == 3)
                {
                    string type = GetFieldType(fieldTypes, condItems[0]);
                    if (type.Equals("STRING", StringComparison.OrdinalIgnoreCase))
                    {
                        newWhere.Append("UPPER(").Append(condItems[0]).Append(") ");
                        newWhere.Append(condItems[1]);
                        newWhere.Append(" UPPER(").Append(condItems[2]).Append(")");
                    }
                    else
                    {
                        newWhere.Append(condItems[0]).Append(" ");
                        newWhere.Append(condItems[1]);
                        newWhere.Append(" ").Append(condItems[2]).Append(" ");
                    }
                }
                else
                {
                    newWhere.Append(" ").Append(newTgtString).Append(" ");
                }
            }
            return newWhere.ToString().Trim();
        }

        private string GetFieldType(string[] fieldTypes, string fieldName)
        {
            foreach (string str in fieldTypes)
            {
                string[] typeName = str.Split(';');
                if (typeName[1].Equals(fieldName, StringComparison.OrdinalIgnoreCase))
                {
                    return typeName[0];
                }
            }
            return "NUEMRIC";
        }


        public ArrayList GetFieldList(string tableName)
        {
            string query = "SELECT * FROM " + tableName + " WHERE 1 = 0";
            DataTable dt = GetDataTable(query);
            ArrayList FieldList = new ArrayList();
            for (int k = 0; k < dt.Columns.Count; k++)
            {
                FieldList.Add(dt.Columns[k]);
            }
            return FieldList;
        }

        public int GetGroupByGridRowCount(string columnName, string sqlWhere, string tableName)
        {
			sqlWhere = GetCaseInsensitiveWhere(sqlWhere, tableName);
            if (string.IsNullOrEmpty(sqlWhere))
            {
                sqlWhere = "";
            }
            else
            {
                sqlWhere = " WHERE " + sqlWhere;
            }

            string query = string.Format("SELECT count(distinct(COALESCE(cast({0} as varchar),'a'))) FROM {1} {2}", columnName, tableName, sqlWhere);
            LogWriter.WriteLog("GetGroupByRowCount: " + query);
            int totalRows = 0;
            using (NpgsqlConnection con = new NpgsqlConnection(ConnectionString))
            {
                NpgsqlCommand com = new NpgsqlCommand(query, con);
                com.CommandTimeout = DBConnectionTimeout;
                con.Open();
                totalRows = Convert.ToInt32(com.ExecuteScalar());
                con.Close();
                com.Dispose();
            }
            return totalRows;

        }

        public DataTable GetGroupByGridData(string sqlSelectFields, string columnName, string tableName, string sqlWhere, string startRow, string pageSize, string SQL_ORDER_BY, string SQL_ORDER_DIR, string QB_GB_SELECT_CLAUSE, string GIS_THEME_LAYER)
        {
            sqlWhere = GetCaseInsensitiveWhere(sqlWhere, tableName);
            string order_by = string.Empty;
            string countBlock = string.Format("COUNT(*) AS Nr");
            if (!string.IsNullOrEmpty(sqlWhere))
            {
                sqlWhere = " WHERE " + sqlWhere;
            }

            if (!string.IsNullOrEmpty(SQL_ORDER_BY) && SQL_ORDER_BY.IndexOf(" AS ") == -1)
            {
                order_by = String.Format(" ORDER BY {0} {1} ", columnName, SQL_ORDER_DIR);
            }
            else if (SQL_ORDER_BY.IndexOf(" AS ") > -1)
            {
                order_by = SQL_ORDER_BY.Split(new string[] { " AS " }, StringSplitOptions.RemoveEmptyEntries)[0];
                order_by = String.Format(" ORDER BY {0} {1} ", order_by, SQL_ORDER_DIR);
            }

            //string query = string.Format("SELECT {0}, {5} FROM {1} {6} GROUP BY {0} {4} LIMIT {2} OFFSET {3} ", columnName, tableName, pageSize, startRow, order_by, countBlock, sqlWhere);
            string query = string.Format("SELECT {0},{1} FROM {2} {3} GROUP BY {4} {5} LIMIT {6} OFFSET {7} ", sqlSelectFields, countBlock, tableName, sqlWhere, columnName, order_by, pageSize, startRow);

            LogWriter.WriteLog("GetGroupByResultsetForGrid: " + query);
            return GetDataTable(query);
        }


        public DataTable GetColorCodeTable(string columnName, string REPORT_CODE, DataTable groupedTable)
        {
            string query = "select COLORCODE,GROUPCODE from group_color where REPORTCODE='" + REPORT_CODE + "' and "
            + " GROUPBY='" + columnName + "' AND GROUPCODE in (";

            string groupCodes = string.Empty;
            for (int n = 0; n < groupedTable.Rows.Count; n++)
            {
                string groupCode = groupedTable.Rows[n][columnName].ToString();
                if (groupCode == "")
                {
                    groupCodes += "'NULL',";
                }
                else
                {
                    groupCodes += "'" + groupCode.Trim().Replace("'", "''") + "',";
                }
            }

            groupCodes = groupCodes.Trim(',');
            query += groupCodes + ")";
            return GetDataTable(query);
        }

        public int GetNormalGridRowCount(string tableName, string sqlWhere)
        {
            sqlWhere = GetCaseInsensitiveWhere(sqlWhere, tableName);
            string query = "SELECT count(*) FROM " + tableName;
            if (!sqlWhere.Equals(string.Empty))
            {
                query += " WHERE " + sqlWhere;
            }
            LogWriter.WriteLog("GetNormalRowCount: " + query);
            int totalRows = 0;
            using (NpgsqlConnection con = new NpgsqlConnection(ConnectionString))
            {
                NpgsqlCommand com = new NpgsqlCommand(query, con);
                com.CommandTimeout = DBConnectionTimeout;
                con.Open();
                totalRows = Convert.ToInt32(com.ExecuteScalar());
                con.Close();
                com.Dispose();
            }
            return totalRows;
        }

        public DataTable GetNormalGridData(string tableName, string sqlSelect, string sqlWhere, string START_ROW, string PAGE_SIZE, string SQL_ORDER_BY, string SQL_ORDER_DIR)
        {
            sqlWhere = GetCaseInsensitiveWhere(sqlWhere, tableName);
            string query = "SELECT " + sqlSelect + " FROM " + tableName;
            if (!sqlWhere.Equals(string.Empty))
            {
                query += " WHERE " + sqlWhere;
            }

            if (SQL_ORDER_BY != null && SQL_ORDER_BY != "" && SQL_ORDER_BY != "undefined" && SQL_ORDER_BY != "ADD" && SQL_ORDER_BY != "EDIT" && SQL_ORDER_BY != "DELETE")
            {
                query += String.Format(" ORDER BY {0} {1}", SQL_ORDER_BY, SQL_ORDER_DIR);
            }

            query += " limit " + PAGE_SIZE + " offset " + START_ROW;

            LogWriter.WriteLog("GetNormalResultsetForGrid: " + query);

            DataTable dt = GetDataTable(query);
            //dt.Columns.Add(new DataColumn("id-no"));

            return dt;
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
                throw new Exception(ex.Message);
            }
            return dataTable;
        }
        
        public void ExecuteTransaction(NpgsqlCommand[] commands)
        {
            NpgsqlTransaction transaction = null;
            NpgsqlConnection conn;
            conn = new NpgsqlConnection(ConnectionString);
            
            try
            {
                conn.Open();
                transaction = conn.BeginTransaction();
                foreach (NpgsqlCommand command in commands)
                {
                    command.CommandTimeout = DBConnectionTimeout;
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
        
        public DataTable GetSpecificRowDetailData(string tableName, string selectedField, string keyList, string valueList)
        {

            string whereClause = MakeWhereClauseFromKeyValues(keyList, valueList);
            string query = string.Empty;
            if (selectedField != "*" && !selectedField.Equals(string.Empty))
            {
                query = "SELECT " + selectedField + " FROM " + tableName;
            }
            else
            {
                query = "SELECT * FROM " + tableName;
            }
            if (!whereClause.Equals(string.Empty))
            {
                query += " WHERE " + whereClause;
            }
            LogWriter.WriteLog("GetSpecificRowDetailData: " + query);

            DataTable dt = GetDataTable(query);
            return dt;
        }

        public DataTable GetRelativeFileName(string tableName, string keyList, string valueList)
        {
            string whereClause = MakeWhereClauseFromKeyValues(keyList, valueList);
            string query = query = "SELECT relfilename FROM " + tableName;

            if (!whereClause.Equals(string.Empty))
            {
                query += " WHERE " + whereClause;
            }
            LogWriter.WriteLog("GetRelativeFileName: " + query);

            DataTable dt = GetDataTable(query);
            return dt;
        }
        public string GetFullFileName(string tableName, string keyList, string valueList)
        {
            string whereClause = MakeWhereClauseFromKeyValues(keyList, valueList);
            string query = query = "SELECT file_fullname FROM " + tableName;

            if (!whereClause.Equals(string.Empty))
            {
                query += " WHERE " + whereClause;
            }
            LogWriter.WriteLog("GetFullFileName: " + query);

            string filename = "";
            try
            {
                filename = GetDataTable(query).Rows[0][0].ToString();
            }
            catch
            {
                filename = "";
            }
            return filename;
        }

        private string MakeWhereClauseFromKeyValues(string keyList, string valueList)
        {
            string[] keyItems = keyList.Split(';');
            string[] valueItems = valueList.Split(';');
            string whereClause = string.Empty;
            for (int k = 0; k < keyItems.Length; k++)
            {
                whereClause += keyItems[k]  + " = '" + valueItems[k] + "' & ";
                //whereClause += "lower(trim(cast(" + keyItems[k] + " AS VARCHAR))) = trim(lower('" + valueItems[k] + "')) & ";
            }
            whereClause = whereClause.TrimEnd(new char[] { '&', ' ' }).Replace("&", "AND").Replace("@$@","&");
            return whereClause;
        }

        public ArrayList GetFieldValues(string SQL_FROM, string FIELD_NAME)
        {
            string query = string.Empty;
            query = "SELECT DISTINCT " + FIELD_NAME + " FROM " + SQL_FROM ;

            DataTable dt = GetDataTable(query);
            ArrayList FieldValues = new ArrayList();

            for (int k = 0; k < dt.Rows.Count; k++)
            {
                FieldValues.Add(dt.Rows[k][FIELD_NAME]);
            }
            return FieldValues;
        }

        public string CheckCustomFieldValidation(string REPORT_CODE, string SQL_FROM, string customFields)
        {
            string query = string.Empty;

            try
            {
                query = "SELECT " + customFields + " FROM " + SQL_FROM + " where 1=0";
                DataTable dt = GetDataTable(query);
                return "true";
            }
            catch (Exception ex)
            {
                return ex.Message + "\n" + query;
            }
        }

        public string ValidateWhereClause(string SQL_FROM, string WHERE_CLAUSE)
        {
            string query = string.Empty;
            try
            {
                query = "SELECT COUNT(*) FROM " + SQL_FROM + " WHERE " + WHERE_CLAUSE;

                DataTable dt = GetDataTable(query);

                return "true";
            }
            catch (Exception ex)
            {
                return ex.Message + "\n" + query;
            }
        }

        public string GetFieldNameType(string SQL_FROM)
        {
            SQL_FROM = SQL_FROM.Trim('"');
            string query = "select column_name,data_type as type from information_schema.columns where table_name='" + SQL_FROM + "'";

            DataTable dt = GetDataTable(query);
            string typeAndNames = string.Empty;
            string dataType = string.Empty;
            foreach (DataRow dr in dt.Rows)
            {
                if (dr["type"].ToString().StartsWith("boolean") || dr["type"].ToString().StartsWith("bigint") || dr["type"].ToString().StartsWith("double") || dr["type"].ToString().StartsWith("numeric") || dr["type"].ToString().StartsWith("integer"))
                {
                    typeAndNames += "NUMERIC";
                }
                else if (dr["type"].ToString().StartsWith("date"))
                {
                    typeAndNames += "DATE";
                }
                else if (dr["type"].ToString().StartsWith("timestamp"))
                {
                    typeAndNames += "TIMESTAMP";
                }
                else
                {
                    typeAndNames += "STRING";
                }
                typeAndNames += ";" + dr["column_name"].ToString() + "|";


            }

            return typeAndNames.TrimEnd('|');
        }

        public string CheckGroupBySelectValidation(string REPORT_CODE, string SQL_FROM, string QB_GB_SELECT_CLAUSE)
        {
            string query = string.Empty;

            try
            {    
                query = "SELECT " + QB_GB_SELECT_CLAUSE + " FROM " + SQL_FROM + " where 1=0";
                DataTable dt = GetDataTable(query);
                return "true";
            }
            catch (Exception ex)
            {
                return ex.Message + "\n" + query;
            }
        }


    }

}
