using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using HIT.PEditor.Core;


/// <summary>
/// Summary description for DBController
/// </summary>
public class DBManagerFactory
{
    public static IDatabaseFunctionsManager GetDBManager(string activeDatabase,string connectionString)
    {
        try
        {
            //string activeDatabase = ConfigurationManager.AppSettings["activedatabase"].ToString();
            switch (activeDatabase)
            {
                case "postgresql":
                    return new PgsqlDBHandler(connectionString);
                    break;
                case "oracle":
                    return new OracleDBHandler(connectionString);
                    break;
                case "xml":
                    return new XMLDBHandler(connectionString);
                    break;
                default:
                    throw new Exception("DB_NAME_ERROR");
                    break;
            }
        }
        catch
        {
            throw new Exception("DB_NAME_ERROR");
        }
        
    }
}
