using System.Data;
using System.Text;
using HIT.OB.STD.Wrapper.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
/// <summary>
/// Summary description for DBManaFactory
/// </summary>
/// 
namespace HIT.OB.STD.Wrapper.BLL
{
    public class WrappingManager
    {

        public WrappingManager(string activeDb)
        {
        }
        public static string GetReportArguments(string reportCode)
        {
            DBManagerFactory dbManagerFactory = new DBManagerFactory();
            IWrapFunctions iWrapFunctions = dbManagerFactory.GetDBManager();

            DataTable dtReportInfo = iWrapFunctions.GetReportArguments(reportCode);
            StringBuilder settingsBuilder = new StringBuilder();
            string sqlSelect = string.Empty;
            string FieldList = "";
            StringBuilder sqlkeyfields = new StringBuilder();
            StringBuilder detailSqlFields = new StringBuilder();
            string fieldTypesJson = "";

            string keyfields = string.Empty;
            string detailSqlFieldSets = string.Empty;

            if (dtReportInfo.Rows.Count > 0)
            {
                DataRow reportInfoRow = dtReportInfo.Rows[0];                
                string sqlFrom = reportInfoRow["sql_from"].ToString();
                string reportSettings = reportInfoRow["report_settings"].ToString();
                string orderBy = reportInfoRow["sql_orderby"].ToString();
                string groupBy = reportInfoRow["sql_groupby"].ToString();
                if (string.IsNullOrEmpty(groupBy))
                {
                    groupBy = "NONE";
                }
                string fieldNameAndType = GetFieldAndTypeList(reportInfoRow["sql_from"].ToString(), reportCode);
                string sqlWhere = reportInfoRow["sql_where"].ToString();
                if (!string.IsNullOrEmpty(sqlWhere))
                {
                    string isWhereValid = HIT.OB.STD.Core.BLL.OBController.ValidateWhereClause(reportCode, sqlFrom, sqlWhere);
                    if (isWhereValid != "true")
                    {
                        sqlWhere = "INVALID_WHERE";
                    }
                }
                string fieldCaps = reportInfoRow["field_caps"].ToString().Replace(',', ';').Trim(new char[] { ';' });
                FieldList = fieldNameAndType.Split(new string[]{"$$$$"},StringSplitOptions.None)[0];
                fieldTypesJson = fieldNameAndType.Split(new string[] { "$$$$" }, StringSplitOptions.None)[1];

                settingsBuilder.AppendFormat("report_code: \"{0}\"", GetJSONFormat(reportCode));
                settingsBuilder.AppendFormat(",report_name: \"{0}\"", GetJSONFormat(reportInfoRow["report_name"].ToString()));
                settingsBuilder.AppendFormat(",field_caps: \"{0}\"", GetJSONFormat(fieldCaps));
                
                string selectedFields = FieldList.Replace("'", "").Replace(',', ';').Trim(';');
                settingsBuilder.AppendFormat(",sql_select: \"{0}\"", GetJSONFormat(selectedFields));

                settingsBuilder.AppendFormat(",sql_from: \"{0}\"", GetJSONFormat(sqlFrom));
                settingsBuilder.AppendFormat(",sql_where: \"{0}\"", GetJSONFormat(sqlWhere));
                settingsBuilder.AppendFormat(",sql_groupby: \"{0}\"", GetJSONFormat(groupBy));
                settingsBuilder.AppendFormat(",gis_theme_layer: \"{0}\"", reportInfoRow["gis_theme_layer"].ToString().ToLower());

                settingsBuilder.AppendFormat(",sql_orderby: \"{0}\"", GetJSONFormat(orderBy));
                settingsBuilder.AppendFormat(",sql_orderdir: \"{0}\"", GetJSONFormat(reportInfoRow["sql_orderdir"].ToString()));
                settingsBuilder.AppendFormat(",report_settings: \"{0}\"", GetJSONFormat(reportSettings));
                string multiSelect = reportInfoRow["multiselect"].ToString().ToLower();                
                settingsBuilder.AppendFormat(",multiselect: \"{0}\"", multiSelect);

                // sqlSelect = reportInfoRow["sql_select"].ToString();
                detailSqlFieldSets = reportInfoRow["detail_fieldsets"].ToString().Replace(",", ";").Trim(';');
                detailSqlFields.AppendFormat("detailsqlfields: \"{0}\"", GetJSONFormat(detailSqlFieldSets));
                
                keyfields = reportInfoRow["sql_keyfields"].ToString().Replace(',', ';').Trim(';');                
                sqlkeyfields.AppendFormat("sqlkeyfields: {0}", GetJSONFormat(SQLKeyFieldsJSON(keyfields)));
                

            }
            dtReportInfo.Dispose();

            //***Report function processing            
            DataTable dtFunctionReportInfo = iWrapFunctions.GetReportFunctionsList(reportCode);
            StringBuilder functionList = new StringBuilder();
            StringBuilder sqlMandatory = new StringBuilder();
            string parameters = string.Empty;
            string sqlFunList = string.Empty;
            string imageUrls = string.Empty;
            string orderPos = string.Empty;
            string temp = string.Empty;
            string commonParams = string.Empty;
            string iscustom = string.Empty;

            if (dtFunctionReportInfo.Rows.Count > 0)
            {
                foreach (DataRow dr in dtFunctionReportInfo.Rows)
                {
                    sqlFunList = dr["function_name"].ToString();
                    orderPos = dr["order_position"].ToString();
                    parameters = dr["parameters"].ToString().Replace(',',';').Trim(new char[] { ';' });
                    iscustom = dr["iscustom"].ToString().ToLower();
                   
                    // getting sqlparameters as semicolon separated.
                    parameters = parameters.Trim(';');                    
                    StringBuilder param = new StringBuilder();                    
                    param.Append("'").Append(parameters.Replace(";", "','")).Append("'");
                    
                    // checking common parameters.
                    commonParams = CommonParameters(parameters, commonParams);
                    functionList.AppendFormat("['{0}','{1}','{2}',[{3}]]", sqlFunList.ToUpper(), orderPos, iscustom, param.ToString());
                    functionList.Append(",");
                } // END of foreach

                commonParams = CompareSqlManSqlKey(commonParams, keyfields);
                functionList.Remove(functionList.ToString().LastIndexOf(','), 1);
                sqlMandatory.Append("'").Append(commonParams).Append("'");
            }
            else
            {
                sqlMandatory.Append("");
                functionList.Append("");
            }
            dtFunctionReportInfo.Dispose();

            return "{settings:{" + settingsBuilder.ToString() +
                    "},fieldTypes:{" + fieldTypesJson + "},fieldList:[" + FieldList + "]" +
                    ",sqlmandatory:[" + sqlMandatory.ToString() +
                    "],functionlist:[" + functionList.ToString() +
                    "]," + sqlkeyfields.ToString() + "," + detailSqlFields.ToString() + "}";
        }

        public static string SQLKeyFieldsJSON(string keyFields)
        {
            string sqlKeyJson = string.Empty;
            if (!keyFields.Equals(string.Empty))
            {
                string[] sqlParams = keyFields.Split(';');
                sqlKeyJson = "{";
                for (int i = 0; i < sqlParams.Length; i++)
                {
                    sqlKeyJson += sqlParams[i].ToString() + ": '', ";
                }
                sqlKeyJson = sqlKeyJson.Remove(sqlKeyJson.LastIndexOf(','), 1);
                sqlKeyJson += "}";
            }
            else
            {
                sqlKeyJson = "''";
            }
            return sqlKeyJson;

        }

        public static string CompareSqlManSqlKey(string sqlMand, string sqlKey)
        {
            if (!sqlKey.Equals(string.Empty))
            {
                string[] sqlParameters = sqlKey.Split(';');
                List<string> mandFieldList = sqlMand.Split(';').ToList<string>();
                for (var x = 0; x < sqlParameters.Length; x++)
                {
                    if (!mandFieldList.Contains(sqlParameters[x]))
                    {
                        sqlMand += ";" + sqlParameters[x].ToLower();
                    }
                }
            }
            return sqlMand;
        }


        // checking common parameters.
        public static string CommonParameters(string parameters, string commonParams)
        {
            string[] sqlParameters = parameters.Split(';');
            if (commonParams.Equals(string.Empty))
            {
                commonParams += parameters;
            }
            else
            {
                List<string> commonParamList = commonParams.ToLower().Split(';').ToList<string>();
                for (int x = 0; x < sqlParameters.Length; x++)
                {
                    if (!commonParamList.Contains(sqlParameters[x].ToLower())) 
                    {
                        commonParams += ";" + sqlParameters[x].ToString();
                    }
                }
            }
            return commonParams;
        }

        static string GetFieldAndTypeList(string tableName, string reportCode)
        {
            StringBuilder delimittedFields = new StringBuilder();
            try
            {
                HIT.OB.STD.Core.BLL.DBManagerFactory dbManagerFactory = new HIT.OB.STD.Core.BLL.DBManagerFactory();
                HIT.OB.STD.Core.DAL.IOBFunctions iCoreFunctions = dbManagerFactory.GetDBManager(reportCode);
                //ArrayList fieldList = iCoreFunctions.GetFieldList(tableName);
                string[] fieldTypes = iCoreFunctions.GetFieldNameType(tableName).Split(new char[]{'|'});
                StringBuilder types = new StringBuilder();
                string[] nameTypeArr = new string[2];
                for (int i = 0; i < fieldTypes.Length; i++)
                {
                    nameTypeArr = fieldTypes[i].Split(';');
                    delimittedFields.Append("'").Append(nameTypeArr[1].ToString()).Append("'").Append(",");
                    types.Append("'").Append(nameTypeArr[1]).Append("':'").Append(nameTypeArr[0]).Append("',");
                }

                string fields = delimittedFields.ToString().Trim(',');
                return fields + "$$$$" + types.ToString().Trim(',');
            }
            catch (Exception ex)
            {
                return "";
            }           
        }

        public static string GetReportList()
        {
            DBManagerFactory dbManagerFactory = new DBManagerFactory();
            IWrapFunctions iWrapFunctions = dbManagerFactory.GetDBManager();

            DataTable dtReportInfo = iWrapFunctions.GetReportList();
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("reportList:[");
            for (int r = 0; r < dtReportInfo.Rows.Count; r++)
            {
                DataRow reportInfoRow = dtReportInfo.Rows[r];
                stringBuilder.Append("{");
                stringBuilder.AppendFormat("report_code: '{0}'", reportInfoRow["report_code"].ToString());
                stringBuilder.AppendFormat(",report_name: '{0}'", reportInfoRow["report_name"].ToString());
                stringBuilder.Append("}");
                if (r < dtReportInfo.Rows.Count - 1)
                {
                    stringBuilder.Append(",");
                }
            }
            dtReportInfo.Dispose();
            stringBuilder.Append("]");
            return "{" + stringBuilder + "}";
        }

        private static string GetJSONFormat(string value)
        {
            //value = value.Trim();
            value = value.Replace("\"", "@@@");
            value = value.Replace("\r\n", "<br/>");
            //value = value.Replace("[", "");
            //value = value.Replace("]", "");
            //value = value.Replace("{", "");
            //value = value.Replace("}", "");
            value = value.Replace(@"\", "\\\\");
            value = value.Replace("\n", "<br/>");
            //value = System.Web.HttpUtility.HtmlEncode(value);                              

            return value;
        }


        internal static bool UpdateUserDefinedReportSettings(string REPORT_CODE,string SQL_WHERE,string GROUP_BY,string ORDER_BY,string ORDER_BY_DIR,string report_settings)
        {
            DBManagerFactory dbManagerFactory = new DBManagerFactory();
            IWrapFunctions iOBFunctions = dbManagerFactory.GetDBManager();
            iOBFunctions.UpdateUserDefinedReportSettings(REPORT_CODE, SQL_WHERE, GROUP_BY, ORDER_BY, ORDER_BY_DIR, report_settings);
            return true;
        }

        public static bool InsertGroupColor(string REPORT_CODE, string GROUP_BY, string GROUP_CODE, string COLOR_CODE)
        {
            DBManagerFactory dbManagerFactory = new DBManagerFactory();
            IWrapFunctions iOBFunctions = dbManagerFactory.GetDBManager();
            bool result = iOBFunctions.InsertGroupColor(REPORT_CODE, GROUP_BY, GROUP_CODE, COLOR_CODE);
            return result;
        }

        public static string DeleteRecord(string keyInfo, string tableName)
        {
            DBManagerFactory dbManagerFactory = new DBManagerFactory();
            IWrapFunctions iOBFunctions = dbManagerFactory.GetDBManager();
            return iOBFunctions.DeleteRecord(keyInfo, tableName);            
        }

        public static string AddUser(string userCode, string userName,string fullName, string address, string city, string phoneNr,
                            string mobile, string skype, string birthDay, string _function, string joinedHawarit, string status,
                            string internNr, string comment, string department, string email, string nickName)
        {
            DBManagerFactory dbManagerFactory = new DBManagerFactory();
            IWrapFunctions iOBFunctions = dbManagerFactory.GetDBManager();
            return iOBFunctions.AddUser(userCode, userName, fullName, address, city, phoneNr,
                                                mobile, skype, birthDay, _function, joinedHawarit, status,
                                                internNr, comment, department, email, nickName); 
        }


        public static string[] GetProjectCodes()
        {
            DBManagerFactory dbManagerFactory = new DBManagerFactory();
            IWrapFunctions iOBFunctions = dbManagerFactory.GetDBManager();
            DataTable tblProjCode = iOBFunctions.GetProjectCodes();
            string[] projCodes = new string[tblProjCode.Rows.Count];
            if (tblProjCode.Rows.Count > 0)
            {
                for (int index = 0; index < tblProjCode.Rows.Count; index++)
                {
                    projCodes[index] = tblProjCode.Rows[index]["projcode"].ToString();
                }
            }
            return projCodes;
        }

        public static string GetNewFileName(string fileName, string projCode)
        {
            DateTime dt = DateTime.Now;
            string revision = dt.Year.ToString().Substring(2) + dt.Month.ToString().PadLeft(2, '0') 
                + dt.Day.ToString().PadLeft(2, '0');
            string extension = fileName.Substring(fileName.LastIndexOf('.'));
            string filePath = System.Configuration.ConfigurationManager.AppSettings["Document-Network-Path"];
            string newFileName = filePath + @"\" + projCode + @"\" + projCode.Split(' ')[0] + revision + extension;
            bool isExists = File.Exists(newFileName);
            
            for (int incr = 2; !isExists.Equals(false); incr++ )
            {
                newFileName = filePath + @"\" + projCode + @"\" + projCode.Split(' ')[0] + revision 
                    + "_" + incr.ToString() + extension;
                isExists = File.Exists(newFileName);
            }
            return newFileName;

        }

        public static DataTable GetReportCodeList()
        {

            DBManagerFactory dbManagerFactory = new DBManagerFactory();
            IWrapFunctions iWrapFunctions = dbManagerFactory.GetDBManager();

            return iWrapFunctions.GetReportList();
        }

        public static string GetRelativePath(string reportName, string fileName)
        {
            DBManagerFactory dbManagerFactory = new DBManagerFactory();
            IWrapFunctions iWrapFunctions = dbManagerFactory.GetDBManager();

            return iWrapFunctions.GetRelativePath(reportName, fileName);
        }

        public static string GetInfoForThisFile(string viewName, string fileName)
        {
            DBManagerFactory dbManagerFactory = new DBManagerFactory();
            IWrapFunctions iWrapFunctions = dbManagerFactory.GetDBManager();

            return iWrapFunctions.GetInfoForThisFile(viewName, fileName);
        }

        public static string UpdateFileInfo(string information, string viewName, string relfilename)
        {
            DBManagerFactory dbManagerFactory = new DBManagerFactory();
            IWrapFunctions iWrapFunctions = dbManagerFactory.GetDBManager();

            return iWrapFunctions.UpdateFileInfo(information, viewName, relfilename);
        }

        public static string SaveFileInfo(string information, string viewName, string reference)
        {
            DBManagerFactory dbManagerFactory = new DBManagerFactory();
            IWrapFunctions iWrapFunctions = dbManagerFactory.GetDBManager();

            return iWrapFunctions.SaveFileInfo(information, viewName, reference);

        }

        public static string DeleteFileInfo(string viewName, string relfilename)
        {
            DBManagerFactory dbManagerFactory = new DBManagerFactory();
            IWrapFunctions iWrapFunctions = dbManagerFactory.GetDBManager();

            return iWrapFunctions.DeleteFileInfo(viewName, relfilename);
        }
    }
}

