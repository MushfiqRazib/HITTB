using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using Npgsql;




namespace HIT.PEditor.Core
{

    public class XMLDBHandler : IDatabaseFunctionsManager   
    {
        private string connectionString = null;

        public XMLDBHandler() : this(ConfigurationManager.ConnectionStrings["DatabaseConnection_Wrapper"].ToString()) { }

        public XMLDBHandler(string connectionstr)
        {

            connectionString = connectionstr;
        }


        public string GetQuery(string xmlFileNameORtabelName, string groupName)
        {
            return "#"+xmlFileNameORtabelName + "|" + groupName;
        }
        
        public DataTable GetDataTable(string sqlQuery)
        {
            DataTable datatable;
            bool xml = sqlQuery.StartsWith("#");

            if (xml)
            {
                DataTable dt = new DataTable();
                string tmpFileName = sqlQuery.Split('|')[0];
                string fileName = tmpFileName.Substring(1);
                string groupName = sqlQuery.Split('|')[1];
                try
                {
                    XElement elements = XElement.Load(HttpContext.Current.Server.MapPath("database") + "\\" + fileName + ".xml");
                    var records = from recordData in elements.Descendants("record")
                                  select recordData;

                    var record = records.First<XElement>();

                    var fields = from data in record.Elements()
                                 select data;
                    foreach (var field in fields)
                    {
                        dt.Columns.Add(field.Name.LocalName);
                    }

                    DataRow dr;
                    foreach (var rcord in records)
                    {
                        dr = dt.NewRow();
                        var columns = from data in rcord.Elements()
                                      select data;
                        foreach (var field in columns)
                        {
                            dr[field.Name.LocalName] = field.Value;
                        }
                        dt.Rows.Add(dr);
                    }
                }
                catch (Exception ex)
                {
                    throw new HttpException("Cannot load XML metadata file :" + ex.ToString());
                }

                DataRow[] rows = dt.Select("GROUPNAME='" + groupName + "'");
                datatable = dt.Clone();
                foreach (DataRow row in rows)
                {
                    datatable.ImportRow(row);
                }
            }
            else
            {
                datatable = new DataTable();
                
                IDatabaseFunctionsManager dbHlr;
                if (connectionString.IndexOf("Provider=OraOLEDB.Oracle") >= 0)
                {
                    dbHlr = DBManagerFactory.GetDBManager("oracle", connectionString);
                }
                else
                {
                    dbHlr = DBManagerFactory.GetDBManager("postgresql", connectionString);
                }

               datatable = dbHlr.GetDataTable(sqlQuery);

            }

            return datatable;
        }
    }


   
}
