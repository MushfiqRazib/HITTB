using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using Npgsql;
using System.IO;

/// <summary>
/// Summary description for PostgresDBHandler
/// </summary>
namespace HIT.PEditor.Wrapper
{
    public class PgsqlDBHandler : PEditorFunctionsManager
    {
        public PgsqlDBHandler()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public void SaveData()
        {
            string fileLocation = HttpContext.Current.Server.MapPath("Output") + "\\";
            string filePath = fileLocation + WrapperAppParams.FileName;
            Dictionary<string, string> fieldValDictionary = CommonFunctions.GetFieldValueList(filePath);

            StringBuilder sqlBuilder = new StringBuilder();
            string mapsetCode = string.Empty;
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection_Wrapper"].ToString();
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    //sql = @"select mapset_type from maps,mapset where maps.mapset_code = mapset.mapset_code and map_code like @mapCode";
                    sqlBuilder.AppendFormat(@"update {0} set ", WrapperAppParams.TableName);

                    foreach (KeyValuePair<string, string> kvp in fieldValDictionary)
                    {
                        if (kvp.Value.Equals("NULL"))
                        {
                            sqlBuilder.AppendFormat(@" {0} = {1}, ", kvp.Key, HttpUtility.HtmlDecode(kvp.Value.Replace("'", "''")));
                        }
                        else
                        {
                            sqlBuilder.AppendFormat(@" {0} = '{1}', ", kvp.Key, HttpUtility.HtmlDecode(kvp.Value.Replace("'", "''")));
                        }
                    }
                    sqlBuilder = sqlBuilder.Remove(sqlBuilder.ToString().LastIndexOf(','), 1);
                    sqlBuilder.Append(" Where ");

                    string[] pkFieldList = WrapperAppParams.FieldNames.Split(';');
                    string[] pkFieldValues = WrapperAppParams.FieldValues.Split(';');
                    for (int k = 0; k < pkFieldList.Length; k++)
                    {
                        sqlBuilder.AppendFormat(" \"{0}\"= '{1}' ", pkFieldList[k], HttpUtility.HtmlDecode(pkFieldValues[k]));
                        if (k < pkFieldList.Length - 1)
                        {
                            sqlBuilder.Append(" AND ");
                        }
                    }

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sqlBuilder.ToString(), connection))
                    {
                        cmd.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public void addData()
        {
            string fileLocation = HttpContext.Current.Server.MapPath("Output") + "\\";
            string filePath = fileLocation + WrapperAppParams.FileName;
            Dictionary<string, string> fieldValDictionary = CommonFunctions.GetFieldValueList(filePath);

            StringBuilder sqlBuilder = new StringBuilder();
            string mapsetCode = string.Empty;
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection_Wrapper"].ToString();
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    //sql = @"select mapset_type from maps,mapset where maps.mapset_code = mapset.mapset_code and map_code like @mapCode";
                    sqlBuilder.AppendFormat(@"insert into {0} (", WrapperAppParams.TableName);

                    foreach (KeyValuePair<string, string> kvp in fieldValDictionary)
                    {
                        sqlBuilder.AppendFormat(@"{0}", kvp.Key);
                        sqlBuilder.Append(", ");
                    }

                    sqlBuilder.Remove(sqlBuilder.ToString().LastIndexOf(','), 1);
                    sqlBuilder.Append(")values(");

                    foreach (KeyValuePair<string, string> kvp in fieldValDictionary)
                    {
                        if (kvp.Value.Equals("NULL"))
                        {
                            sqlBuilder.AppendFormat(@"{0}, ", HttpUtility.HtmlDecode(kvp.Value.Replace("'", "''")));
                        }
                        else
                        {
                            if (kvp.Value.Equals(string.Empty))
                            {
                                sqlBuilder.Append("NULL, ");
                            }
                            else
                            {
                                sqlBuilder.AppendFormat(@" '{0}', ", HttpUtility.HtmlDecode(kvp.Value.Replace("'", "''")));
                            }
                        }
                    }
                    sqlBuilder = sqlBuilder.Remove(sqlBuilder.ToString().LastIndexOf(','), 1);
                    sqlBuilder.Append(")");

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sqlBuilder.ToString(), connection))
                    {
                        cmd.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public static ArrayList GetPrimaryKeyFields(String tableName, ref ArrayList skipList)
        {

            using (NpgsqlConnection connection = new NpgsqlConnection(CommonFunctions.GetWrapperConenctionString()))
            {
                DataTable table = new DataTable();
                using (NpgsqlDataAdapter adapt = new NpgsqlDataAdapter("select column_name, constraint_name from information_schema.constraint_column_usage where table_name='" + tableName + "'", connection))
                {
                    adapt.Fill(table);
                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        String v = (String)table.Rows[i]["column_name"];
                        if (table.Rows[i]["constraint_name"].ToString().EndsWith("_pkey"))
                        {
                            skipList.Add(v);
                        }
                    }
                }
            }
            return skipList;
        }

        public static ArrayList GetColumnType(string tableName, ref ArrayList skipList)
        {

            string sql = @"select column_name, data_type as type 
                                        from information_schema.columns where table_name='" + tableName + "'";

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(CommonFunctions.GetWrapperConenctionString()))
                {
                    connection.Open();
                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, connection))
                    {
                        DataTable dtColType = new DataTable();
                        NpgsqlDataAdapter adapt = new NpgsqlDataAdapter(cmd);
                        adapt.Fill(dtColType);
                        foreach (DataRow dr in dtColType.Rows)
                        {
                            if (dr["type"].ToString().ToUpper().Equals("USER-DEFINED"))
                            {
                                skipList.Add(dr["column_name"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return skipList;
        }

        public static ArrayList GetPrimaryKeyFields(String tableName)
        {
            ArrayList primaryKeyList = new ArrayList();
            using (NpgsqlConnection connection = new NpgsqlConnection(CommonFunctions.GetWrapperConenctionString()))
            {
                DataTable table = new DataTable();
                using (NpgsqlDataAdapter adapt = new NpgsqlDataAdapter("select column_name, constraint_name from information_schema.constraint_column_usage where table_name='" + tableName + "'", connection))
                {
                    adapt.Fill(table);
                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        String v = (String)table.Rows[i]["column_name"];
                        if (table.Rows[i]["constraint_name"].ToString().EndsWith("_pkey"))
                        {
                            primaryKeyList.Add(v);
                        }
                    }
                }
            }
            return primaryKeyList;
        }

        public void WritePropertyXML()
        {
            string fileLocation = HttpContext.Current.Server.MapPath("Output") + "\\";
            try
            {
                ArrayList skipList = new ArrayList();
                //GetPrimaryKeyFields(WrapperAppParams.TableName, ref skipList);
                GetColumnType(WrapperAppParams.TableName, ref skipList);
                string query = "select * from " + WrapperAppParams.TableName + " where ";
                string[] pkFieldList = WrapperAppParams.FieldNames.Split(';');
                string[] pkFieldValues = WrapperAppParams.FieldValues.Split(';');
                for (int k = 0; k < pkFieldList.Length; k++)
                {
                    if (pkFieldList[k].Equals("-1"))
                    {
                        ArrayList pkList = GetPrimaryKeyFields(WrapperAppParams.TableName);
                        if (pkList.Count > 0)
                        {
                            pkFieldList[k] = pkList[0].ToString();
                        }
                        else
                        {
                            query += "1=0";
                            break;
                        }
                    }

                    query += " \"" + pkFieldList[k] + "\"='" + pkFieldValues[k] + "'";

                    if (k < pkFieldList.Length - 1)
                    {
                        query += " AND ";
                    }
                }

                NpgsqlConnection dbConnection = new NpgsqlConnection();
                dbConnection.ConnectionString = CommonFunctions.GetWrapperConenctionString();
                dbConnection.Open();
                DataTable dtValues = GetDataTable(query, dbConnection);
                XmlDocument xml = new XmlDocument();
                XmlElement root = xml.CreateElement("CAD");
                xml.AppendChild(root);
                XmlComment comment = xml.CreateComment("config values");
                root.AppendChild(comment);

                XmlElement connectionString = xml.CreateElement("connectionString");
                connectionString.InnerText = CommonFunctions.GetConnectionString();
                root.AppendChild(connectionString);

                XmlElement wrapperConnectionString = xml.CreateElement("wrapperConnectionString");
                wrapperConnectionString.InnerText = CommonFunctions.GetWrapperConenctionString();
                root.AppendChild(wrapperConnectionString);

                XmlElement metaTable = xml.CreateElement("metaTable");
                metaTable.InnerText = CommonFunctions.GetMetaTableName();
                root.AppendChild(metaTable);

                //XmlElement metaTableStorage = xml.CreateElement("metadataStorage");
                //metaTableStorage.InnerText = CommonFunctions.GetMetaDataStorageName();
                //root.AppendChild(metaTableStorage);

                XmlElement activeDatabase = xml.CreateElement("activeDatabase");
                activeDatabase.InnerText = CommonFunctions.GetActiveDatabaseName();
                root.AppendChild(activeDatabase);

                comment = xml.CreateComment("field-value pair below");
                root.AppendChild(comment);
                XmlElement group = xml.CreateElement("group");
                group.InnerText = WrapperAppParams.GroupName;
                root.AppendChild(group);

                if (dtValues.Rows.Count > 0)
                {
                    DataRow dtRow = dtValues.Rows[0];
                    foreach (DataColumn col in dtValues.Columns)
                    {
                        XmlElement field = xml.CreateElement("field");
                        XmlElement name = xml.CreateElement("name");
                        if (!skipList.Contains(col.ColumnName))
                        {
                            name.InnerText = col.ColumnName;
                            XmlElement value = xml.CreateElement("value");
                            if (col.DataType.Name == "DateTime")
                            {
                                string date = "";
                                try
                                {
                                    date = Convert.ToDateTime(dtRow[col.ColumnName]).ToString("dd-MM-yyyy");
                                }
                                catch
                                {

                                }
                                finally
                                {
                                    value.InnerText = date;
                                }
                            }
                            else
                            {
                                value.InnerText = dtRow[col.ColumnName].ToString();
                            }

                            field.AppendChild(name);
                            field.AppendChild(value);
                            root.AppendChild(field);
                        }
                    }
                }
                else
                {
                    string addMode = HttpContext.Current.Request.Params["mode"];

                    if (!String.IsNullOrEmpty(addMode) && addMode.Equals("add"))
                    {
                        foreach (DataColumn col in dtValues.Columns)
                        {
                            XmlElement field = xml.CreateElement("field");
                            XmlElement name = xml.CreateElement("name");
                            XmlElement value = xml.CreateElement("value");

                            if (!skipList.Contains(col.ColumnName))
                            {
                                name.InnerText = col.ColumnName;

                                foreach (string pkfield in pkFieldList)
                                {
                                    if (col.ColumnName.Equals(pkfield))
                                    {
                                        if (col.DataType.Name.Equals("Int32"))
                                        {
                                            string sql = @"SELECT MAX(" + col.ColumnName + ")+1 from " + HttpContext.Current.Request.Params["tableName"];
                                            value.InnerText = GetDataTable(sql, dbConnection).Rows[0][0].ToString();
                                            break;
                                        }
                                    }
                                }
                                field.AppendChild(name);
                                field.AppendChild(value);
                                root.AppendChild(field);
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("No record found");
                    }
                }


                System.IO.StreamWriter sw = new System.IO.StreamWriter(fileLocation + WrapperAppParams.FileName, false);

                sw.WriteLine(xml.InnerXml);
                sw.Close();

                dbConnection.Close();
                dtValues.Dispose();
            }
            catch (Exception exp)
            {
                File.Delete(fileLocation + WrapperAppParams.FileName);
                throw exp;
            }

        }

        public static DataTable GetDataTable(string query, NpgsqlConnection dbConnection)
        {
            DataTable dataTable = new DataTable();
            try
            {
                NpgsqlConnection myConnection = dbConnection;
                NpgsqlDataAdapter adapter = new NpgsqlDataAdapter();
                adapter.SelectCommand = new NpgsqlCommand(query, myConnection);
                adapter.Fill(dataTable);
            }
            catch (Exception ex)
            {
                throw new Exception("From GetDataTable method:" + ex.Message);
            }
            return dataTable;
        }

        public static ArrayList GetFieldList(string tableName, NpgsqlConnection dbConnection)
        {
            string query = "SELECT * FROM " + tableName + " WHERE 1 = 0";
            DataTable dt = GetDataTable(query, dbConnection);
            ArrayList fieldList = new ArrayList();
            for (int k = 0; k < dt.Columns.Count; k++)
            {
                fieldList.Add(dt.Columns[k]);
            }
            return fieldList;
        }


    }
}
