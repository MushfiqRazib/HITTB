using System;
using System.Data;
using HIT.OB.STD.Core.DAL;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
/// Summary description for OBController
/// </summary>
namespace HIT.OB.STD.Core.BLL
{
    public class OBController
    {

        public static string GetNormalGridData(string REPORT_CODE, string FIELD_CAPS, string SQL_SELECT,
                                        string SQL_FROM, string SQL_WHERE, string SQL_ORDER_BY, string SQL_ORDER_DIR, string START_ROW,
                                        string PAGE_SIZE, string SQL_MANDATORY, string MULTI_SELECT,
                                        string FUNCTION_LIST)
        {
            string reportData = string.Empty;
            try
            {
                DBManagerFactory dbManagerFactory = new DBManagerFactory();
                IOBFunctions iOBFunctions = dbManagerFactory.GetDBManager(REPORT_CODE);
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("{");

                int rowCount = iOBFunctions.GetNormalGridRowCount(SQL_FROM, SQL_WHERE);
                stringBuilder.Append("\"rowCount\":\"" + rowCount + "\",");
                stringBuilder = PrepareJsonForNormalGrid(stringBuilder, iOBFunctions, SQL_FROM, SQL_WHERE, SQL_SELECT, START_ROW, PAGE_SIZE, SQL_MANDATORY, MULTI_SELECT, FUNCTION_LIST, SQL_ORDER_BY, SQL_ORDER_DIR);

                stringBuilder.Append("}");
                reportData = stringBuilder.ToString();
            }
            catch (Exception ex)
            {
                LogWriter.WriteLog(ex.Message);
                reportData = ex.StackTrace;
            }
            return reportData;
        }


        public static string GetGroupByGridData(string REPORT_CODE, string FIELD_CAPS, string SQL_SELECT,
                                        string SQL_FROM, string SQL_WHERE, string SQL_ORDER_BY, string SQL_ORDER_DIR, string START_ROW,
                                        string PAGE_SIZE, string SQL_GROUP_BY, string MULTI_SELECT, string QB_GB_SELECT_CLAUSE, string GIS_THEME_LAYER)
        {
            string reportData = string.Empty;
            try
            {
                DBManagerFactory dbManagerFactory = new DBManagerFactory();
                IOBFunctions iOBFunctions = dbManagerFactory.GetDBManager(REPORT_CODE);
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("{");

                int rowCount = iOBFunctions.GetGroupByGridRowCount(SQL_GROUP_BY, SQL_WHERE, SQL_FROM);
                stringBuilder.Append("\"rowCount\":\"" + rowCount + "\",");
                stringBuilder.Append(PrepareJsonForGroupGrid(SQL_SELECT, SQL_GROUP_BY, SQL_FROM, SQL_WHERE, START_ROW, iOBFunctions, PAGE_SIZE, SQL_ORDER_BY, SQL_ORDER_DIR, QB_GB_SELECT_CLAUSE, GIS_THEME_LAYER, REPORT_CODE, MULTI_SELECT));

                stringBuilder.Append("}");
                reportData = stringBuilder.ToString();
            }
            catch (Exception ex)
            {
                LogWriter.WriteLog(ex.Message);
                reportData = ex.StackTrace;
            }
            return reportData;
        }

        private static StringBuilder PrepareJsonForNormalGrid(StringBuilder stringBuilder, IOBFunctions iOBFunctions,
             string SQL_FROM, string SQL_WHERE, string SQL_SELECT, string START_ROW, string PAGE_SIZE,
             string SQL_MANDATORY, string MULTI_SELECT, string FUNCTION_LIST, string SQL_ORDERBY, string SQL_ORDER_DIR)
        {
            try
            {
                ArrayList fieldList = iOBFunctions.GetFieldList(SQL_FROM);
                //string selectFields = GetSelectableFieldList(SQL_SELECT, SQL_GISSELECT, fieldList);
                string selectFields = GetSelectableFieldList(SQL_SELECT, SQL_MANDATORY);

                ArrayList FieldsList = new ArrayList();
                string[] array = selectFields.Split(',');
                for (int k = 0; k < array.Length; k++)
                {
                    FieldsList.Add(array[k]);
                }

                string columnNames = GetCurrentTablesColumnsFormat(SQL_SELECT, MULTI_SELECT, FieldsList, FUNCTION_LIST);
                stringBuilder.Append(columnNames);
                string[] selectFieldList = selectFields.Split(',');
                DataTable dtForNormalGrid = iOBFunctions.GetNormalGridData(SQL_FROM, selectFields, SQL_WHERE, START_ROW, PAGE_SIZE, SQL_ORDERBY, SQL_ORDER_DIR);
                stringBuilder.Append("\"gridInfo\":[");

                int rowNum = Int32.Parse(START_ROW) + 1;
                for (int i = 0; i < dtForNormalGrid.Rows.Count; i++, rowNum++)
                {
                    DataRow currentRow = dtForNormalGrid.Rows[i];
                    if (MULTI_SELECT.Equals("true"))
                    {
                        stringBuilder.Append("[\"MULTISELECT\",");
                    }
                    else
                    {
                        stringBuilder.Append("[");
                    }
                    stringBuilder.Append("\"" + rowNum + "\",");

                    if (!FUNCTION_LIST.Equals(string.Empty))
                    {
                        FUNCTION_LIST = FUNCTION_LIST.TrimEnd('#');
                        string[] functions = FUNCTION_LIST.Split('#');
                        string[] parameters = null;

                        for (int count = 0; count < functions.Length; count++)
                        {
                            string funcArgs = functions[count].Trim(',');
                            // Make json for only built in/non custom functions
                            string isCustom = funcArgs.Split(',')[2];
                            if (!isCustom.Equals("true"))
                            {
                                string paramValues = string.Empty;
                                string paramsDef = FunctionsParam(functions[count]);
                                if (!string.IsNullOrEmpty(paramsDef))
                                {
                                    parameters = paramsDef.Split(',');
                                    for (int p = 0; p < parameters.Length; p++)
                                    {
                                        paramValues += currentRow[parameters[p]].ToString() + ",";
                                    }
                                }

                                string functionJson = GetFunctionJson(functions[count]);
                                stringBuilder.Append(functionJson);
                            }
                        }
                    }

                    //foreach (var field in selectFieldList)
                    //{
                    //    stringBuilder.AppendFormat("\"{0}\",", GetJSONFormat(currentRow[field].ToString()));
                    //}

                    foreach (DataColumn dc in currentRow.Table.Columns)
                    {
                        stringBuilder.AppendFormat("\"{0}\",", GetJSONFormat(currentRow[dc].ToString()));
                    }

                    stringBuilder = stringBuilder.Remove(stringBuilder.ToString().LastIndexOf(','), 1);
                    stringBuilder.Append("]");
                    if (i < dtForNormalGrid.Rows.Count - 1)
                    {
                        stringBuilder.Append(",");
                    }
                }
                stringBuilder.Append("]");
            }
            catch (Exception ex)
            {
                LogWriter.WriteLog(ex.Message + "    " + ex.StackTrace);
                throw new Exception(ex.Message);
            }
            return stringBuilder;
        }

        private static string GetJSONFormat(string value)
        {
            //value = value.Trim();
            value = value.Replace("\"", "''");
            value = value.Replace("'", "\\'");
            value = value.Replace("\r\n", "<br/>");
            value = value.Replace(@"\", "\\\\");
            value = value.Replace("\n", "<br/>");
            value = value.Replace("\r", "<br/>");
            return value;
        }

        public static StringBuilder GetDistinctFieldsFromSqlSelect(string sqlSelect, string mandatoryFields)
        {
            StringBuilder sqlMandatory = new StringBuilder();

            mandatoryFields = mandatoryFields.Trim(';');

            // if no select sql * found than sqlmandatory == null
            // Case 1: Parameters : id; SQL_SELECT= *; sqlmandatory= null;
            if (sqlSelect == "*" || sqlSelect.Equals(string.Empty))
            {
                sqlMandatory.Append("");
            }
            // if existing parameter not stayed in sqlselect than it assigned to sqlmandatory
            // Case 2: Parameters : id; SQL_SELECT= ref_name,create_date; sqlmandatory= id;
            else
            {
                string[] sqlParameters = mandatoryFields.Split(';');
                List<string> selectedList = sqlSelect.ToLower().Split(';').ToList<string>();
                //sqlmandatory.Append("[");
                for (int x = 0; x < sqlParameters.Length; x++)
                {
                    if (!selectedList.Contains(sqlParameters[x].ToLower()))
                    {
                        sqlMandatory.Append(sqlParameters[x].ToLower() + ";");
                    }
                }
                //sqlMandatory.Remove(sqlMandatory.ToString().LastIndexOf(';'), 1);
                //sqlmandatory.Append("]");
            }
            return sqlMandatory;
        }

        private static string GetGroupedTablesColumnsFormat(string sqlSelectFields, DataTable dt, string GIS_THEME_LAYER, string MULTI_SELECT, bool isQB_GroupBy_Function_Exists)
        {
            string columns = string.Empty;
            StringBuilder sb = new StringBuilder();

            //if (GIS_THEME_LAYER == "true")
            //{
            //    sb.Append("\"columnInfo\":['MULTISELECT',\"#\",\"EXPANDCOLLAPSE\",\"GROUP2FILTER\",");
            //}
            //else
            //{
            sb.Append("\"columnInfo\":[\"#\",\"EXPANDCOLLAPSE\",\"GROUP2FILTER\",");
            //}

            if (GIS_THEME_LAYER == "true")
            {
                sb.Append("\"THEME_COLOR\",");
            }


            string[] selectFields = sqlSelectFields.Split(',');
            foreach (string field in selectFields)
            {
                columns += "\"" + field + "\",";
            }

            sb.Append(columns);
            sb = sb.Remove(sb.ToString().LastIndexOf(','), 1);
            sb.Append("],");

            return sb.ToString();
        }


        private static string PrepareJsonForGroupGrid(string sqlSelectFields, string columnName, string tableName, string SQL_WHERE, string START_ROW, IOBFunctions iOBFunctions, string PAGE_SIZE, string SQL_ORDER_BY, string SQL_ORDER_DIR, string QB_GB_SELECT_CLAUSE, string GIS_THEME_LAYER, string REPORT_CODE, string MULTI_SELECT)
        {
            bool isQB_GroupBy_Function_Exists = false;
            DataTable groupedTable = iOBFunctions.GetGroupByGridData(sqlSelectFields, columnName, tableName, SQL_WHERE, START_ROW, PAGE_SIZE, SQL_ORDER_BY, SQL_ORDER_DIR, QB_GB_SELECT_CLAUSE, GIS_THEME_LAYER);
            int totalCol = groupedTable.Columns.Count;
            if (QB_GB_SELECT_CLAUSE != null && QB_GB_SELECT_CLAUSE != "" && QB_GB_SELECT_CLAUSE != "undefined")
            {
                isQB_GroupBy_Function_Exists = true;
            }
            string columnsNames = GetGroupedTablesColumnsFormat(sqlSelectFields, groupedTable, GIS_THEME_LAYER, MULTI_SELECT, isQB_GroupBy_Function_Exists);

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(columnsNames);
            stringBuilder.Append("\"gridInfo\":[");
            int rowNum = Int32.Parse(START_ROW) + 1;

            string select = string.Empty;
            DataTable groupColorTable = null;
            if (GIS_THEME_LAYER == "true" && groupedTable.Rows.Count > 0)
            {
                groupColorTable = iOBFunctions.GetColorCodeTable(columnName, REPORT_CODE, groupedTable);
            }

            for (int i = 0; i < groupedTable.Rows.Count; i++, rowNum++)
            {
                DataRow currentRow = groupedTable.Rows[i];
                // for Checkbox column 
                //if (GIS_THEME_LAYER == "true")
                //{
                //    stringBuilder.Append("[\"MULTISELECT\",\"" + rowNum + "\",");
                //}
                //else
                //{
                stringBuilder.Append("[\"" + rowNum + "\",");
                //}

                string antal = currentRow[totalCol - 1].ToString();     // antal == total                              
                // image '+'/'-' sign column                
                string temp = "'<div  id=\"img" + rowNum + "\" class=\"collapse\" onclick=\"OBSettings.ShowInnerGrid( \\'" +
                                rowNum + "\\', \\'" + antal + "\\',event)\" ></div>',";
                stringBuilder.Append(temp);

                temp = "'<div  id=\"grp2img" + rowNum + "\" class=\"GROUP2FILTER\" onclick=\"OBSettings.ShowGroup2Filter(\\'" + rowNum + "\\')\" ></div>',";
                stringBuilder.Append(temp);

                GetColorColumn(columnName, GIS_THEME_LAYER, groupedTable, stringBuilder, groupColorTable, i);
                foreach (DataColumn column in groupedTable.Columns)
                {
                    if (!(isQB_GroupBy_Function_Exists == true && column.Ordinal == totalCol - 1))
                    {
                        stringBuilder.AppendFormat("\"{0}\",", GetJSONFormat(currentRow[column].ToString()));
                    }
                }
                stringBuilder = stringBuilder.Remove(stringBuilder.ToString().LastIndexOf(','), 1);
                stringBuilder.Append("]");
                if (i < groupedTable.Rows.Count - 1)
                {
                    stringBuilder.Append(",");
                }
            }

            stringBuilder.Append("]");
            return stringBuilder.ToString();
        }

        private static void GetColorColumn(string columnName, string GIS_THEME_LAYER, DataTable groupedTable, StringBuilder stringBuilder, DataTable groupColorTable, int i)
        {
            // for color column
            string colorDiv = string.Empty;
            DataRow[] dRows = null;
            if (GIS_THEME_LAYER == "true")
            {
                string tmpGC = groupedTable.Rows[i][columnName].ToString().Trim();
                if (tmpGC == "")
                {
                    tmpGC = "NULL";
                }
                dRows = groupColorTable.Select("GROUPCODE='" + tmpGC.Replace("'", "''") + "'");

                if (dRows.Length > 0)
                {
                    colorDiv = "'<div id=\"color" + i + "\" style=\"background-color:#" +
                                    dRows[0]["COLORCODE"].ToString() + ";width:10px;height:10px;\" colorcode=\"" +
                                    dRows[0]["COLORCODE"].ToString() + "\" ></div>',";
                }
                else
                {
                    colorDiv = "'<div id=\"color" + i + "\" style=\"background-color:#" +
                               ColorCodes.colorList[i] + ";width:10px;height:10px;\" colorcode=\"" +
                               ColorCodes.colorList[i] + "\" ></div>',";
                }
                stringBuilder.Append(colorDiv);
            }
        }


        private static string GetCurrentTablesColumnsFormat(string sqlSelect, string MULTI_SELECT, ArrayList tableFields, string functionlist)
        {
            string selectFields = string.Empty;
            StringBuilder sb = new StringBuilder();
            sb.Append("\"columnInfo\":[");
            if (MULTI_SELECT.Equals("true"))
            {
                sb.Append("\"MULTISELECT\",");
            }
            sb.Append("\"#\",");
            string functionsName = GetReportFunctionNameList(functionlist);
            if (!functionsName.Equals(string.Empty))
            {
                sb.Append(functionsName);
            }
            //functionsName = functionsName.Remove(functionsName.LastIndexOf(','), 1);            
            foreach (var listItem in tableFields)
            {
                //if (listItem.ToString().ToUpper().Contains(" AS "))
                //{
                //    string fieldAlias = listItem.ToString().ToUpper();

                //    sb.Append("\"" + listItem.ToString().Substring(fieldAlias.IndexOf(" AS ") + 4).Trim() + "\",");
                //}
                //else
                //{
                sb.Append("\"" + listItem + "\",");
                //}
            }

            sb = sb.Remove(sb.ToString().LastIndexOf(','), 1);
            sb.Append("],");

            return sb.ToString();
        }


        public static string GetReportFunctionNameList(string functionList)
        {
            string functionsName = string.Empty;
            if (!functionList.Equals(string.Empty))
            {
                functionList = functionList.Trim('#');
                string[] functions = functionList.Split('#');
                for (int i = 0; i < functions.Length; i++)
                {
                    functions[i] = functions[i].Trim(',');
                    string[] fArgs = functions[i].Split(',');
                    //Only non-custom/built in functions
                    string isCustom = fArgs[2];
                    if (!isCustom.Equals("true"))
                    {
                        functionsName += "\"" + fArgs[0] + "\",";
                    }
                }
            }
            return functionsName;
        }

        public static string GetFunctionJson(string functionRow)
        {
            string functionsJson = string.Empty;
            if (!functionRow.Equals(string.Empty))
            {
                if (functionRow.StartsWith(","))
                {
                    functionRow = functionRow.Remove(functionRow.IndexOf(',', 0), 1);
                }
                string[] fname = functionRow.Split(',');
                functionsJson += "\'<div class=\"" + fname[0] + "\"  onclick=\"" + fname[0] +
                        "(event)\" ></div>\', ";

            }
            return functionsJson;
        }

        public static string FunctionsParam(string functionRow)
        {
            string functionParam = string.Empty;
            if (!functionRow.Equals(string.Empty))
            {
                if (functionRow.StartsWith(","))
                {
                    functionRow = functionRow.Remove(functionRow.IndexOf(',', 0), 1);
                }
                string[] fparam = functionRow.Split('|');
                fparam[1] = fparam[1].Remove(fparam[1].IndexOf(',', 0), 1);
                fparam[1] = fparam[1].Remove(fparam[1].LastIndexOf(','), 1);
                functionParam = fparam[1];
            }
            return functionParam;
        }

        private static string GetSelectableFieldList(string sqlSelect, string sqlMandatory)
        {
            string selectFields = string.Empty;
            selectFields = GetDistinctFieldsFromSqlSelect(sqlSelect, sqlMandatory).ToString();
            selectFields += sqlSelect;
            string replacedSelect = selectFields.Replace(";", ",");
            selectFields = replacedSelect.Trim(new char[] { ',' });
            return selectFields;
        }

        public static string GetFieldValues(string REPORT_CODE, string SQL_FROM, string FIELD_NAME)
        {
            StringBuilder sb = new StringBuilder();

            DBManagerFactory dbManagerFactory = new DBManagerFactory();
            IOBFunctions iOBFunctions = dbManagerFactory.GetDBManager(REPORT_CODE);


            ArrayList filedvalues = iOBFunctions.GetFieldValues(SQL_FROM, FIELD_NAME);
            foreach (object value in filedvalues)
            {
                sb.Append(value.ToString() + "###");
            }

            return sb.ToString().Trim('#');
        }

        public static string CheckCustomFieldValidation(string REPORT_CODE, string SQL_FROM, string customFields)
        {
            StringBuilder sb = new StringBuilder();

            DBManagerFactory dbManagerFactory = new DBManagerFactory();
            IOBFunctions iOBFunctions = dbManagerFactory.GetDBManager(REPORT_CODE);

            return iOBFunctions.CheckCustomFieldValidation(REPORT_CODE, SQL_FROM, customFields);

        }

        public static string ValidateWhereClause(string REPORT_CODE, string SQL_FROM, string WHERE_CLAUSE)
        {
            StringBuilder sb = new StringBuilder();
            //WHERE_CLAUSE = GetRectifiedWhereClause(WHERE_CLAUSE);
            DBManagerFactory dbManagerFactory = new DBManagerFactory();
            IOBFunctions iOBFunctions = dbManagerFactory.GetDBManager(REPORT_CODE);
            return iOBFunctions.ValidateWhereClause(SQL_FROM, WHERE_CLAUSE);
        }

        public static string GetFieldNameType(string REPORT_CODE, string SQL_FROM)
        {
            DBManagerFactory dbManagerFactory = new DBManagerFactory();
            IOBFunctions iOBFunctions = dbManagerFactory.GetDBManager(REPORT_CODE);

            return iOBFunctions.GetFieldNameType(SQL_FROM);
        }


        internal static string CheckGroupBySelectValidation(string REPORT_CODE, string SQL_FROM, string QB_GB_SELECT_CLAUSE)
        {
            DBManagerFactory dbManagerFactory = new DBManagerFactory();
            IOBFunctions iOBFunctions = dbManagerFactory.GetDBManager(REPORT_CODE);

            return iOBFunctions.CheckGroupBySelectValidation(REPORT_CODE, SQL_FROM, QB_GB_SELECT_CLAUSE);
        }
    }


}

