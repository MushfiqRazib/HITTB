<%@ WebHandler Language="C#" Class="DocLoadHandler" %>

using System;
using System.IO;
using System.Web;
using System.Data;
using HIT.OB.STD.Core.DAL;
using System.Collections;
using System.Text;

public class DocLoadHandler : IHttpHandler
{

    public void ProcessRequest(HttpContext context)
    {
        string docStatusRequest = context.Request.QueryString["GETSTATUS"];
        if (!string.IsNullOrEmpty(docStatusRequest))
        {
            try
            {
                //string whereClause = string.Empty;
                string sqlFrom = string.Empty;
                string reportCode = context.Request.QueryString["REPORT_CODE"];

                HIT.OB.STD.Wrapper.BLL.DBManagerFactory dbManagerFactory = new HIT.OB.STD.Wrapper.BLL.DBManagerFactory();
                HIT.OB.STD.Wrapper.DAL.IWrapFunctions iWrapFunctions = dbManagerFactory.GetDBManager();
                                
                sqlFrom = context.Request.QueryString["SQL_FROM"];
                //whereClause = context.Request.QueryString["whereClause"].ToString().Replace("$", " AND ");
                string keyList = context.Request.QueryString["KEYLIST"].ToString();
                string valueList = context.Request.QueryString["VALUELIST"].ToString();     
                
                
                //*** Check document validation.
                string fileType = context.Request.QueryString["TYPE"];
                string relFilename = string.Empty;


                HIT.OB.STD.Core.BLL.DBManagerFactory dbManagerFactoryCore = new HIT.OB.STD.Core.BLL.DBManagerFactory();
                HIT.OB.STD.Core.DAL.IOBFunctions iCoreFunc = dbManagerFactoryCore.GetDBManager(reportCode);
                DataTable dt = iCoreFunc.GetRelativeFileName(sqlFrom, keyList, valueList);
                
                if (dt.Rows.Count > 0)
                {
                    relFilename = dt.Rows[0][0].ToString();
                }
                //string basePath = @"D:\Redline\";
                string basePath = HIT.OB.STD.Wrapper.CommonFunctions.GetDocBasePath();
                DataTable itemData = iCoreFunc.GetItemData(sqlFrom, keyList, valueList);
                string relativeFileName = itemData.Rows[0]["relfilename"].ToString();
                string filePath = Path.Combine(basePath, relativeFileName);
                if (fileType.ToLower().Equals("pdf"))
                {
                    filePath = filePath.ToLower().Replace(".dwf", ".pdf");
                    relativeFileName = relativeFileName.ToLower().Replace(".dwf", ".pdf");
                }

                if (File.Exists(filePath))
                {
                    context.Response.ContentType = "application/xml";
                    context.Response.Write(relativeFileName);
                    //context.Response.End();
                    HttpContext.Current.ApplicationInstance.CompleteRequest();
                }
                else
                {
                    //*** When all validation failed
                    context.Response.Write("$$$$$:Document not found");
                }
            }
            catch (Exception exp)
            {
                if (exp.Message.ToLower().IndexOf("relfilename") > -1)
                {
                    context.Response.Write("$$$$$: Documents relative filename not defined.");
                }
                else
                {
                    context.Response.Write("$$$$$: Document viewer not allowed for the report.");
                }
            }
        }
        else
        {
            string basePath = HIT.OB.STD.Wrapper.CommonFunctions.GetDocBasePath();
            string filePath = basePath + context.Request.QueryString["RELFILEPATH"];
            string fileType = context.Request.QueryString["TYPE"];

            switch (fileType.ToLower())
            {
                case "dwf":
                    context.Response.ContentType = "application/x-Autodesk-DWF";

                    break;
                case "pdf":
                    context.Response.ContentType = "application/pdf";
                    break;
                default:
                    context.Response.ContentType = "application/octet-stream";
                    break;
            }

            FileStream file = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Read);
            StreamReader sr = new StreamReader(file);
            Stream stream = sr.BaseStream;

            const int buffersize = 1024 * 16;
            byte[] buffer = new byte[buffersize];
            int count = stream.Read(buffer, 0, buffersize);
            while (count > 0)
            {
                context.Response.OutputStream.Write(buffer, 0, count);
                count = stream.Read(buffer, 0, buffersize);
            }
            file.Dispose();
            stream.Dispose();
            stream.Close();

        }
    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

}