using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


public static class ErrorList
{
    private static Dictionary<string, string> _ErrorList;

    static ErrorList()
    {
        _ErrorList = new Dictionary<string, string>();
        _ErrorList.Add("DB_NAME_ERROR", "Active database for property editor is undefined/invalid.");
        _ErrorList.Add("CONN_STR_ERROR", "Cannot connect to database.SQL in LOV value is invalid Or Database connection string not defined/invalid.");
        _ErrorList.Add("DB_STORAGE_ERROR", "Metadata storage name undefined(XML/RDB)");
        _ErrorList.Add("DATE_ERR", "Input Date is not valid");

    }

    public static string GetErrorMessage(string errorCode)
    {
        string msg;
        try
        {
            msg = _ErrorList[errorCode];
        }
        catch (Exception exp)
        {
            return errorCode;
        }
        return msg;
    }

}
