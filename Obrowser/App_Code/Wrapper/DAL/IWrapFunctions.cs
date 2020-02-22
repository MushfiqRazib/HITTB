using System.Data;
using System.Collections;

/// <summary>
/// Summary description for IOBFunctions
/// </summary>
namespace HIT.OB.STD.Wrapper.DAL
{
    public interface IWrapFunctions
    {
        //*** The database connection has to be opened automatically
        //*** during instantiation of the database manager. So Database
        //*** connection will be active for the lifetime of the manager.
        //*** When the manager no longer used then it has to be closed.
        //*** There is no database open function to restrict frequent database
        //*** connection creation.
        
        DataTable GetReportList();
        DataTable GetReportArguments(string reportCode);
        DataTable GetReportFieldList(string tableName);
        DataTable GetReportFunctionsList(string reportCode);
        DataTable GetDataTable(string query);        
        DataRow GetReportConfigInfo(string reportCode);
        string GetConnectionStringForReport(string REPORT_CODE);
        bool UpdateUserDefinedReportSettings(string REPORT_CODE, string SQL_WHERE, string GROUP_BY, string ORDER_BY, string ORDER_BY_DIR, string report_settings);
        bool InsertGroupColor(string REPORT_CODE, string GROUP_BY, string GROUP_CODE, string COLOR_CODE);
        string DeleteRecord(string keyInfo, string tableName);

        string AddUser(string userCode, string userName, string fullName, string address, string city, string phoneNr,
                            string mobile, string skype, string birthDay, string _function, string joinedHawarit, string status,
                            string internNr, string comment, string department, string email, string nickName);

        DataTable GetProjectCodes();


        ///
               
        string GetRelativePath(string reportName, string fileName);
        string GetInfoForThisFile(string viewName, string fileName);
        string SaveFileInfo(string information, string viewName, string reference);
        string UpdateFileInfo(string information, string viewName, string relfilename);
        string DeleteFileInfo(string viewName, string relfilename);
    }
}