using System;
using System.Collections;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Linq;
using HITKITClient;
using System.Web.Script.Services;
using HIT.OB.STD.Wrapper.BLL;
using System.Data;
using System.IO;

/// <summary>
/// Summary description for ClientSecurityService
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
 [System.Web.Script.Services.ScriptService]
public class AppServicesForKit : System.Web.Services.WebService
{

    [WebMethod]
    public string ValidateUserAndGetSecurityId(string userId, string roleName, string rolePass, string print_server_location, string kit_server_url, string docSharePath)
    {
        if (SecurityManager.IsValidCredential(roleName, rolePass))
        {
            return SecurityManager.InsertSecurityInfo(userId, roleName, print_server_location, kit_server_url, docSharePath);
        }
        return "-1";
    }

    [WebMethod]
    [ScriptMethod]
    public string[] GetAuthenticationValues(string securityKey)
    {
        return SecurityManager.GetAuthenticationValues(securityKey);
    }
      
    /// <summary>
    /// 1. Return True means last access time update and session exists
    /// 2. Return False means session timeout
    /// </summary>
    /// <param name="securityKey"></param>
    /// <returns></returns>
    [WebMethod]
    [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json, XmlSerializeString = false)]        
    public bool UpdateLastAccessTime(string securityKey)
    {
        DataTable dt = SecurityManager.GetUserSessionInfo(securityKey);
        if (dt.Rows.Count > 0)
        {
            DateTime dTime = Convert.ToDateTime(dt.Rows[0]["last_access_time"]);
            Double timeout = Convert.ToDouble(dt.Rows[0]["timeout"]);
            TimeSpan idleTimeSpan = DateTime.Now.Subtract(dTime);
            Double idleTimeInSecond = idleTimeSpan.TotalSeconds;
            if (timeout >= idleTimeInSecond)
            {
                SecurityManager.UpdateLastAccessTime(securityKey);
                return true;

            }
            else
            {
                SecurityManager.DeleteSecurityKeyInfo(securityKey);
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    [WebMethod]
    public DataSet GetReportCodes()
    {

        DataSet ds = new DataSet();
        DataTable dt = new DataTable("reportTable");

        try
        {
            dt = WrappingManager.GetReportCodeList();
            ds.Tables.Add(dt);
            return ds;
        }
        catch (Exception ex)
        {
            ds.Tables.Add(dt);
            return ds;
        }
    }


    [WebMethod]
    public string SaveDocument(Byte[] fileChunk, int chunkNo, bool isNewFile, string relfileName)
    {

        string filePath = HIT.OB.STD.Wrapper.CommonFunctions.GetDocBasePath() + relfileName;

        LogWriter.WriteLog(filePath.Substring(0, filePath.LastIndexOf(Path.DirectorySeparatorChar) + 1));

        string message = string.Empty;
        long customPosition = chunkNo * 64000;
        FileStream objFileStream;
        try
        {
            if (isNewFile)
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                else
                {
                    Directory.CreateDirectory(filePath.Substring(0, filePath.LastIndexOf(Path.DirectorySeparatorChar) + 1));
                }
                objFileStream = new FileStream(filePath, FileMode.Create);
            }
            else
            {
                objFileStream = new FileStream(filePath, FileMode.Open);
            }

            objFileStream.Seek(customPosition, SeekOrigin.Begin);
            objFileStream.Write(fileChunk, 0, (int)fileChunk.Length);
            objFileStream.Close();
        }
        catch (Exception ex)
        {
            message = "error";
        }

        return message;
    }


    [WebMethod]
    public string GetFullXML(string filePath)
    {
        XElement xml = XElement.Load(filePath);
        return xml.ToString();
    }

    [WebMethod]
    public string GetRelativePath(string reportName, string fileName)
    {
        string message = string.Empty;

        try
        {
            message = WrappingManager.GetRelativePath(reportName, fileName);
        }
        catch (Exception ex)
        {
            message = "error";
        }

        return message;
    }
    [WebMethod]
    public Byte[] FetchFileChunk(string filePath, bool findLatest, DateTime? lastModifiedDTime, string locationCode, int counter)
    {
        string message = string.Empty;

        if (!File.Exists(filePath))
        {
            return null;
        }
        else
        {
            if (findLatest && lastModifiedDTime != null && File.GetLastWriteTime(filePath) <= lastModifiedDTime)
            {
                return null;
            }
        }


        try
        {
            System.IO.FileStream fileStream = new FileStream(filePath, FileMode.Open);
            byte[] fileBuffer = null;
            fileBuffer = GiveFileChunk(counter, ref fileStream);
            fileStream.Close();
            return fileBuffer;
        }
        catch (Exception exp)
        {
            return null;
        }
    }


    private Byte[] GiveFileChunk(int chunkNo, ref FileStream objFileStream)
    {
        int customCount = 64000;
        int readDataSuccessfull = 0;
        long customPosition = chunkNo * customCount;

        Byte[] fileInfo = new Byte[customCount];
        /*
         * As count or file chunk count starts from 0.
         * So position will be set from 0 first time.
         *Setting the stream position
         */
        objFileStream.Seek(customPosition, SeekOrigin.Begin);

        if (customPosition < objFileStream.Length)
        {
            long remainingLength = objFileStream.Length - customPosition;
            if (remainingLength < customCount)
            {
                fileInfo = new Byte[remainingLength];
                customCount = (int)remainingLength;
            }
        }

        if (customPosition <= objFileStream.Length)
        {
            try
            {
                readDataSuccessfull = objFileStream.Read(fileInfo, 0, customCount);
            }
            catch (FieldAccessException objFileAccessEx)
            {
                throw objFileAccessEx;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        if (readDataSuccessfull == 0)
        {
            return null;
        }
        else
        {
            return fileInfo;
        }
    }

    [WebMethod]
    public string GetFileProperties(string relativeFilePath)
    {
        string filePath = HIT.OB.STD.Wrapper.CommonFunctions.GetDocBasePath() + relativeFilePath;

        if (File.Exists(filePath))
        {
            FileInfo fileInfo = new FileInfo(filePath);
            return fileInfo.LastWriteTime.ToString("dd-MM-yyyy") + "@@@@" + fileInfo.CreationTime.ToString("dd-MM-yyyy") + "@@@@" + (fileInfo.Length / 1024).ToString() + " kb";
        }

        return "";
    }

    [WebMethod]
    public string UpdateFileInfo(string information, string viewName, string relfilename)
    {
        string message = string.Empty;

        message = WrappingManager.UpdateFileInfo(information, viewName, relfilename);
        return message;
    }

    [WebMethod]
    public string SaveFileInfo(string information, string viewName, string reference)
    {
        string message = string.Empty;

        message = WrappingManager.SaveFileInfo(information, viewName, reference);
        return message;
    }

    [WebMethod]
    public string GetInfoForThisFile(string viewName, string fileName)
    {
        string message = string.Empty;

        try
        {
            message = WrappingManager.GetInfoForThisFile(viewName, fileName);
        }
        catch (Exception ex)
        {
            message = "error";
        }

        return message;
    }

    [WebMethod]
    public string DeleteFileInfo(string viewName, string relfilename)
    {
        string message = string.Empty;

        message = WrappingManager.DeleteFileInfo(viewName, relfilename);

        return message;
    }

    [WebMethod]
    public string DeleteFile(string relfilename)
    {
        string message = "success";
        string filePath = HIT.OB.STD.Wrapper.CommonFunctions.GetDocBasePath() + relfilename;

        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        catch (Exception ex)
        {
            message = "error";
        }
        return message;

    }

    
}



