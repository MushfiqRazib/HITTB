using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;

public partial class OfficeDownloader : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string docPath = "";
        string fileType = "";
        if (Request.QueryString["FILE"] == null)
        {
            string reportCode = Request.QueryString["REPORT_CODE"];
            string sqlFrom = Request.QueryString["SQL_FROM"];
            string keyList = Request.QueryString["KEYLIST"].ToString();
            string valueList = Request.QueryString["VALUELIST"].ToString();

            //*** Check document validation.
            fileType = Request.QueryString["TYPE"];
            //if (fileType.ToLower().Equals("odt")) {
            //    fileType = "zip";
            //}
            HIT.OB.STD.Core.BLL.DBManagerFactory dbManagerFactoryCore = new HIT.OB.STD.Core.BLL.DBManagerFactory();
            HIT.OB.STD.Core.DAL.IOBFunctions iCoreFunc = dbManagerFactoryCore.GetDBManager(reportCode);
            //string relFilename = iCoreFunc.GetRelativeFileName(sqlFrom, keyList, valueList);
            //string relFilename = iCoreFunc.GetFullFileName(sqlFrom, keyList, valueList);
            //string basePath = HIT.OB.STD.Wrapper.CommonFunctions.GetDocBasePath("DocBasePath");
            //string docPath = Path.Combine(basePath, relFilename.Replace(@"/", @"\"));
            docPath = iCoreFunc.GetFullFileName(sqlFrom, keyList, valueList);
            docPath = docPath.Replace(@"/", @"\");
            docPath = docPath.Replace(".dwf", "." + fileType);
        }
        else
        {
            docPath = Request.QueryString["FILE"].ToString();
            fileType = docPath.Substring(docPath.LastIndexOf('.') + 1);
        }
        if (System.IO.File.Exists(docPath))
        {
            switch (fileType.ToLower())
            {
                case "dwf":
                    Response.ContentType = "application/x-Autodesk-DWF";
                    break;
                case "dwg":
                    Response.ContentType = "application/x-Autodesk-DWF";
                    break;
                case "tiff":
                    Response.ContentType = "image/tiff";
                    break;
                case "tif":
                    Response.ContentType = "image/tiff";
                    break;
                case "pdf":
                    Response.ContentType = "application/pdf";
                    break;
                case "txt":
                    Response.ContentType = "text/xml";
                    break;
                case "doc":
                    Response.ContentType = "application/msword";
                    break;
                case "odt":
                case "ott":
                case "ots":
                    Response.ContentType = "application/vnd.oasis.opendocument.text";
                //    Response.Charset = "UTF-8";
                   break;
                default:
                    Response.ContentType = "application/octet-stream";
                    break;
            }
            
            int startIndex = docPath.Replace('\\','/').LastIndexOf('/')+1;
            string fileName = docPath.Substring(startIndex, docPath.Length - startIndex);
            Response.AddHeader("Content-Disposition", "attachment;filename=" + fileName);
            //Response.AddHeader("Content-Disposition", "attachment;filename=" + fileName.Replace(".odt",".zip"));
            FileStream file = new FileStream(docPath, FileMode.OpenOrCreate, FileAccess.Read);
            StreamReader sr = new StreamReader(file);
            Stream stream = sr.BaseStream;

            const int buffersize = 1024 * 16;
            byte[] buffer = new byte[buffersize];
            int count = stream.Read(buffer, 0, buffersize);
            while (count > 0)
            {
                Response.OutputStream.Write(buffer, 0, count);
                //Response.BinaryWrite(buffer);
                count = stream.Read(buffer, 0, buffersize);
            }
            //file.Dispose();
            //stream.Dispose();
            stream.Close();
            Response.End();
        }
        else
        {
            //Response.ContentType = "text/javascript";
            Response.Write("<script type=\"text/javascript\">alert(\"File not found "+ docPath+"\");</script>");
            //Response.End();
        }

    }
}
