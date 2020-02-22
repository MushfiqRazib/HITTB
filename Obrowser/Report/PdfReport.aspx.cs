using System;
using System.Configuration;
using System.Data;

using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;


using System.Collections.Generic;
using PdfSharp.Drawing;

public partial class _Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        PrintReport();
    }

    /// <summary>
    /// 
    /// </summary>
    private void PrintReport()
    {
        //if space is required, then it can be passed by - char 
        //the - will remove by space

        string username = "null";
        string titel = "null";
        string showfilter = "null";
        string curdate = "null";
        string paperformat = string.Empty;
        string papersize = string.Empty;
        string table = string.Empty;
        string fields = string.Empty;
        string whereclause = string.Empty;
        string groupby = "NONE";
        string orderby = string.Empty;
        string reporttype = string.Empty;
        string fieldswidth = string.Empty;
        
        string securityKey = string.Empty;

        string selectSql = string.Empty;
        string countSql = string.Empty;
        string gbselectexpression = string.Empty;
        string reportCode = string.Empty;

        EPageSize EpageSize;
        EPaperFormat EpaperFormat;

        DBUtil.CONN_STRING = ConfigurationManager.ConnectionStrings["obcore_connectionstring"].ConnectionString;
        try
        {
            string QueryStringUrl = Server.UrlDecode(Request.Url.Query.Substring(Request.Url.Query.IndexOf('?') + 1));
            string[] KeyValues = QueryStringUrl.Split('&');

            for (int i = 0; i < KeyValues.Length; i++)
            {
                string[] KeyValuePair = KeyValues[i].Split('=');
                string key = KeyValuePair.Length > 0 ? KeyValuePair[0].Trim() : string.Empty;
                string value = KeyValues[i].Substring(KeyValues[i].IndexOf('=') + 1).Trim();

                switch (key)
                {
                    case "username":
                        username = value;
                        break;
                    case "securityKey":
                        securityKey = value;
                        break;
                    case "titel":
                        titel = value;
                        break;
                    case "showfilter":
                        showfilter = value;
                        break;
                    case "curdate":
                        curdate = value;
                        break;
                    case "paperformat":
                        paperformat = value;
                        break;
                    case "papersize":
                        papersize = value;
                        break;
                    case "reporttype":
                        reporttype = value;
                        break;
                    case "reportcode":
                        reportCode = value;
                        break;
                    case "table":
                        table = value.Replace("@@@", "\""); 
                        break;
                    case "fields":
                        fields = value;
                        break;
                    case "whereclause":
                        whereclause = value;
                        break;
                    case "groupby":
                        groupby = value;
                        break;
                    case "orderby":
                        orderby = value;
                        break;
                    case "gbselectexpression":
                        gbselectexpression = value;
                        break;
                    case "fieldswidth":
                        fieldswidth = value;
                        break;
                   
                }
            }


            if (!string.IsNullOrEmpty(fields) && !string.IsNullOrEmpty(table))
            {
                selectSql = " select " + fields + " from " + table;

                if (!string.IsNullOrEmpty(whereclause))
                {
                    selectSql += " where " + whereclause + " ";
                }

                if (!groupby.Equals("NONE") && !string.IsNullOrEmpty(groupby))
                {
                    if (reporttype == "Regular")
                    {
                        countSql = " select count(*)," + groupby + " from " + table;
                    }
                    else
                    {
                        countSql = " select " + gbselectexpression + "," + groupby + " from " + table;
                    }
                                      
                    if (!string.IsNullOrEmpty(whereclause))
                    {
                        countSql += " where " + whereclause + " ";
                    }

                    countSql += " group by " + groupby;

                    selectSql += " order by " + groupby;
                }

                if (!string.IsNullOrEmpty(orderby))
                {
                    selectSql += selectSql.Contains("order by") == true ? " , " + orderby : " order by " + orderby;
                    if (countSql != string.Empty)
                    {
                        countSql += " order by " + orderby;
                    }
                }
            }

            Dictionary<string, string> dicParams = new Dictionary<string, string>();

            username = username != "null" && securityKey != string.Empty ? "Static Name" : "null";

            dicParams.Add("username", username);
            dicParams.Add("titel", titel);

            showfilter = showfilter != "null" ? whereclause == string.Empty ? "De data wordt niet gefiltered"
                : whereclause
                : showfilter;
            dicParams.Add("showfilter", showfilter);
            dicParams.Add("curdate", curdate.Equals("null") == true ? "null" : DateTime.Now.ToString("dd/MM/yyyy"));

            switch (papersize)
            {
                case "a3":
                    EpageSize = EPageSize.A3;
                    break;
                case "a4":
                    EpageSize = EPageSize.A4;
                    break;
                default:
                    EpageSize = EPageSize.A4;
                    break;
            }

            switch (paperformat)
            {
                case "landscape":
                    EpaperFormat = EPaperFormat.LandScape;
                    break;
                case "portrait":
                    EpaperFormat = EPaperFormat.Portrait;
                    break;
                default:
                    EpaperFormat = EPaperFormat.Portrait;
                    break;
            }

            ReportUtil oReportUtil = new ReportUtil();

            ReportProperties data = oReportUtil.GetReportProperties(selectSql, fieldswidth, reportCode, dicParams, EpageSize, EpaperFormat, Server.MapPath(FileNameManager.PdfFileName));

            DataTable recordCountTableForEachGroup = oReportUtil.GetRecordCountForEachGroup(countSql,reportCode);

            switch (reporttype)
            {
                case "Regular":
                    oReportUtil.GenerateRegularReport(data, recordCountTableForEachGroup, Response);
                    break;
                case "Piechart":
                case "Histogram":
                    Dictionary<string, string> groupColors =
                        new ColorUtil().GetGroupColors(reportCode,recordCountTableForEachGroup, table, groupby);
                    List<Group> oListGroup = new List<Group>();
                    for (int i = 0; i < recordCountTableForEachGroup.Rows.Count; i++)
                    {
                        Group oGroup = new Group();
                        oGroup.GroupValue = double.Parse(recordCountTableForEachGroup.Rows[i][0].ToString());
                        oGroup.GroupName = recordCountTableForEachGroup.Rows[i][groupby].ToString();
                        oGroup.GroupColour = XColor.FromArgb(System.Drawing.ColorTranslator.FromHtml("#" + groupColors[oGroup.GroupName]));
                        oListGroup.Add(oGroup);
                    }

                    if (reporttype == "Piechart")
                    {
                        oReportUtil.GeneratePieChart(data, Response, oListGroup);
                    }
                    else if (reporttype == "Histogram")
                    {
                        oReportUtil.GenerateColumnChart(data, Response, oListGroup, groupby);
                    }
                    break;
                default:
                    oReportUtil.GenerateRegularReport(data, recordCountTableForEachGroup, Response);
                    break;
            }
        }
        catch (Exception oEx)
        {
            Response.Write("Following server error occurs...<br/>");
            Response.Write("Select sql:" + selectSql + "<br/>");
            Response.Write("Count sql:" + countSql + "<br/>");
            Response.Write(oEx.Message + "<br/>");
            Response.Write(oEx.StackTrace);
        }
    }

    private string getFields(string FiledWithWidth)
    {
        string FieldsWithoutWidth = string.Empty;
        string[] Fields= FiledWithWidth.Split(',');
        foreach (string item in Fields)
        {
            string[] items = item.Split('*');
            FieldsWithoutWidth  =FieldsWithoutWidth + items[0] + ",";
        }

       FieldsWithoutWidth = FieldsWithoutWidth.Substring(0, FieldsWithoutWidth.LastIndexOf(','));
        
       return FieldsWithoutWidth;
    }
}
