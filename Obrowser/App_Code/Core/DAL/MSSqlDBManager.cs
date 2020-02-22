using System;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;
using System.Collections;
using System.Text.RegularExpressions;

/// <summary>
/// Summary description for PostgresDBHandler
/// </summary>

namespace HIT.OB.STD.Core.DAL
{
    public class MSSqlDBManager : IOBFunctions
    {

        private string connectionString;
        private int DBConnectionTimeout
        {
            get
            {
                return Int32.Parse(ConfigurationManager.AppSettings["dbConnectionTimeout"]);
            }
        }
        public MSSqlDBManager(string conString)
        {
            this.connectionString = conString;
        }

        private string ConnectionString
        {
            get { return connectionString; }
            set { connectionString = value; }
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
            if (string.IsNullOrEmpty(sqlWhere))
            {
                sqlWhere = "";
            }
            else
            {
                sqlWhere = " WHERE " + sqlWhere;
            }

            string query = string.Format(@"SELECT Count(distinct(case when cast({0} as varchar(100)) = '' or {0} is null 
                                          then 'a' else cast({0} as varchar(100)) end)) FROM {1} {2}", columnName, tableName, sqlWhere);

            LogWriter.WriteLog("GetGroupByRowCount: " + query);
            int totalRows = 0;
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                SqlCommand com = new SqlCommand(query, con);
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
            columnName = "[" + columnName + "]";
            string order_by = "ORDER BY 1";
            string countBlock = string.Format("COUNT(*) AS Nr");

            string fromRowNumber = (Int32.Parse(startRow) + 1).ToString();
            string toRowNumber = (Int32.Parse(startRow) + Int32.Parse(pageSize)).ToString();


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

            string query = string.Format("SELECT {0}, {2} FROM {1} {3} GROUP BY {0}", columnName, tableName, countBlock, sqlWhere);


            query = " SELECT row_number() OVER ( " + order_by + ") ROWID_, A.* FROM (" + query + ") A";

            query = "WITH [ROWID_] AS (" + query + ") SELECT * FROM [ROWID_] WHERE ROWID_ BETWEEN " + fromRowNumber + " AND " + toRowNumber;

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
            groupCodes = groupCodes.Remove(groupCodes.LastIndexOf(','), 1);
            query += groupCodes + ")";
            return GetDataTable(query);
        }

        public int GetNormalGridRowCount(string tableName, string sqlWhere)
        {
            string query = "SELECT count(*) FROM " + tableName;
            if (!sqlWhere.Equals(string.Empty))
            {
                query += " WHERE " + sqlWhere;
            }
            LogWriter.WriteLog("GetNormalRowCount: " + query);
            int totalRows = 0;
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                SqlCommand com = new SqlCommand(query, con);
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

            sqlSelect = GetSurroundedSquareBracketFieldNameString(sqlSelect);
            string query = "SELECT " + sqlSelect + " FROM " + tableName;

            string whereClause = string.Empty;
            string orderByClause = "ORDER BY " + sqlSelect.Split(new char[] { ',' })[0];

            string fromRowNumber = (Int32.Parse(START_ROW) + 1).ToString();
            string toRowNumber = (Int32.Parse(START_ROW) + Int32.Parse(PAGE_SIZE)).ToString();

            if (!sqlWhere.Equals(string.Empty))
            {
                query += " WHERE " + sqlWhere;
            }

            if (SQL_ORDER_BY != null && SQL_ORDER_BY != "" && SQL_ORDER_BY != "undefined" && SQL_ORDER_BY != "ADD" && SQL_ORDER_BY != "EDIT" && SQL_ORDER_BY != "DELETE")
            {
                orderByClause = String.Format(" ORDER BY {0} {1}", SQL_ORDER_BY, SQL_ORDER_DIR);
            }


            query = " SELECT row_number() OVER ( " + orderByClause + ") ROWID_, A.* FROM (" + query + ") A";
            query = "WITH [ROWID_] AS (" + query + ") SELECT * FROM [ROWID_] WHERE ROWID_ BETWEEN " + fromRowNumber + " AND " + toRowNumber;

            LogWriter.WriteLog("GetNormalResultsetForGrid: " + query);

            DataTable dt = GetDataTable(query);
            dt.Columns.Remove("ROWID_");
            //dt.Columns.Add(new DataColumn("id-no"));

            return dt;
        }

        public DataTable GetDataTable(string query)
        {
            DataTable dataTable = new DataTable();
            try
            {
                using (SqlConnection dbConnection = new SqlConnection(ConnectionString))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand(query, dbConnection);
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



        public void ExecuteTransaction(SqlCommand[] commands)
        {
            SqlTransaction transaction = null;
            SqlConnection conn;
            conn = new SqlConnection(ConnectionString);
            
            try
            {
                conn.Open();
                transaction = conn.BeginTransaction();
                foreach (SqlCommand command in commands)
                {
                    command.CommandTimeout = DBConnectionTimeout;
                    command.Connection = conn;
                    command.Transaction = transaction;
                    command.ExecuteNonQuery();
                }
                transaction.Commit();
            }
            catch (SqlException ex)
            {
                throw new Exception("ERROR: " + ex.ErrorCode + "<br>" + "ERROR Message: " + ex.Message);
                transaction.Rollback();
            }
            finally
            {
                conn.Close();
            }
        }


        public DataTable GetSpecificRowDetailData(string tableName, string selectedField, string keyList, string valueList)
        {
            string[] keyItems = keyList.Split(';');
            string[] valueItems = valueList.Split(';');
            string whereClause = string.Empty;
            for (int k = 0; k < keyItems.Length; k++)
            {
                whereClause += "lower(CAST (" + keyItems[k] + " AS VARCHAR)) = lower('" + valueItems[k] + "') & ";
            }
            whereClause = whereClause.TrimEnd(new char[] { '&', ' ' }).Replace("&", "AND");
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
                whereClause += keyItems[k] + " = '" + valueItems[k] + "' & ";
                //whereClause += "lower(trim(cast(" + keyItems[k] + " AS VARCHAR(255)))) = trim(lower('" + valueItems[k] + "')) & ";
            }
            whereClause = whereClause.TrimEnd(new char[] { '&', ' ' }).Replace("&", "AND");
            return whereClause;
        }

        public ArrayList GetFieldValues(string SQL_FROM, string FIELD_NAME)
        {
            string query = string.Empty;
            FIELD_NAME = GetSurroundedSquareBracketFieldNameString(FIELD_NAME);
            query = "SELECT  DISTINCT " + FIELD_NAME + " FROM " + SQL_FROM;

            DataTable dt = GetDataTable(query);
            ArrayList FieldValues = new ArrayList();

            for (int k = 0; k < dt.Rows.Count; k++)
            {
                FieldValues.Add(dt.Rows[k][0]);
            }
            return FieldValues;
        }

        public string CheckCustomFieldValidation(string REPORT_CODE, string SQL_FROM, string customFields)
        {
            string query = string.Empty;
            customFields = GetSurroundedSquareBracketFieldNameString(customFields);
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
                return ex.Message.Split(':')[2] + "\n" + query;
            }
        }

        public string GetFieldNameType(string SQL_FROM)
        {

            SQL_FROM = SQL_FROM.Trim('"');
            string query = "exec sp_columns " + SQL_FROM + "";

            DataTable dt = GetDataTable(query);
            string typeAndNames = string.Empty;
            string dataType = string.Empty;
            foreach (DataRow dr in dt.Rows)
            {
                if (dr["TYPE_NAME"].ToString().ToLower().StartsWith("int") ||
                    dr["TYPE_NAME"].ToString().ToLower().StartsWith("tinyint") ||
                    dr["TYPE_NAME"].ToString().ToLower().StartsWith("smallint") ||
                    dr["TYPE_NAME"].ToString().ToLower().StartsWith("bigint") ||
                    dr["TYPE_NAME"].ToString().ToLower().StartsWith("decimal") ||
                    dr["TYPE_NAME"].ToString().ToLower().StartsWith("float") ||
                    dr["TYPE_NAME"].ToString().ToLower().StartsWith("numeric") ||
                    dr["TYPE_NAME"].ToString().ToLower().StartsWith("real") ||
                    dr["TYPE_NAME"].ToString().ToLower().StartsWith("bit"))
                {
                    typeAndNames += "NUMERIC";
                }
                else if (dr["TYPE_NAME"].ToString().ToLower().StartsWith("smalldatetime") || dr["TYPE_NAME"].ToString().ToLower().StartsWith("datetime"))
                {
                    typeAndNames += "DATE";
                }
                else if (dr["TYPE_NAME"].ToString().ToLower().StartsWith("timestamp"))
                {
                    typeAndNames += "TIMESTAMP";
                }
                else
                {
                    typeAndNames += "STRING";
                }
                typeAndNames += ";" + dr["COLUMN_NAME"].ToString() + "|";


            }

            return typeAndNames.TrimEnd('|');
        }

        public string CheckGroupBySelectValidation(string REPORT_CODE, string SQL_FROM, string QB_GB_SELECT_CLAUSE)
        {
            string query = string.Empty;
            QB_GB_SELECT_CLAUSE = GetSurroundedSquareBracketFieldNameString(QB_GB_SELECT_CLAUSE);
            try
            {
                query = "SELECT " + QB_GB_SELECT_CLAUSE + " FROM " + SQL_FROM + " where 1=0";
                DataTable dt = GetDataTable(query);
                return "true";
            }
            catch (Exception ex)
            {
                return ex.Message.Split(':')[2] + "\n" + query;
            }
        }

        string GetSurroundedSquareBracketFieldNameString(string sqlSelect)
        {
            string[] sqlFieldName = sqlSelect.Split(new char[] { ',' });
            sqlSelect = "";
            string pattern = @"[a-z,A-Z,0-9,_]+";
            foreach (string tmpName in sqlFieldName)
            {
                string name = tmpName;
                MatchCollection collection = Regex.Matches(name, pattern);
                if (collection.Count > 0)
                {
                    double numericValue = 0;
                    int expectedMatchCount = 0;
                    foreach (Match item in collection)
                    {
                        if (!double.TryParse(item.ToString(), out numericValue) && IsDBFieldName(item, tmpName))
                        {
                            string stringToReplace = item.Value;
                            int index = item.Index + expectedMatchCount * 2;
                            int length = stringToReplace.Length;
                            name = name.Remove(index, length);
                            name = name.Insert(index, string.Format("[{0}]", stringToReplace));
                            expectedMatchCount++;
                        }
                    }
                }

                sqlSelect += name + ",";
            }
            
            sqlSelect = sqlSelect.TrimEnd(',');
            return sqlSelect;

        }

        bool IsDBFieldName(Match item, string fieldExpression)
        {
            bool returnValue = true;
            fieldExpression = fieldExpression.Trim();
            switch (item.Value.ToLower())
            {
                case "as":
                    int index = item.Index;
                    if (index > 0)
                    {
                        if (fieldExpression.Substring(index - 1, 1).Equals(" ") && fieldExpression.Substring(index + 2, 1).Equals(" "))
                        {
                            Match nextMatch = item.NextMatch();
                            if (nextMatch.Success)
                            {
                                int lenghtBetweenTwoMatches = nextMatch.Index - (index + 2);
                                if (fieldExpression.Substring(index + 2, lenghtBetweenTwoMatches).IndexOfAny(new char[] { '+', '-', '*', '/' }) == -1)
                                {
                                    return false;
                                }
                            }

                        }
                    }

                    break;
                case "count":
                case "avg":
                case "sum":
                    index = item.Index;
                    int length = item.Length;
                    if (fieldExpression.Remove(index, length).TrimStart().Substring(0, 1).Equals("("))
                    {
                        return false;

                    }
                    else
                    {
                        returnValue = true;
                    }
                    break;
                default:
                    break;
            }

            return true;
        }

        public DataTable GetItemData(string sqlFrom, string keyList, string valueList)
        {
            throw new Exception("Not implemented yet");
        }
    }

}
