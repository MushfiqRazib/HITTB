using System;
using HIT.OB.STD.Core.DAL;

/// <summary>
/// Summary description for DBManaFactory
/// </summary>
/// 
namespace HIT.OB.STD.Core.BLL
{
    public class DBManagerFactory
    {
        public IOBFunctions GetDBManager(string reportCode)
        {
            string activeDatabase;
            HIT.OB.STD.Wrapper.BLL.DBManagerFactory dbManagerFactory = new HIT.OB.STD.Wrapper.BLL.DBManagerFactory();
            HIT.OB.STD.Wrapper.DAL.IWrapFunctions iWrapFunctions = dbManagerFactory.GetDBManager();

            string conString = iWrapFunctions.GetConnectionStringForReport(reportCode);
            if (conString.Equals(string.Empty))
            {
                activeDatabase = ConfigManager.GetActiveDatabase();
                conString = ConfigManager.GetConnectionString();
            }
            else
            {
                activeDatabase = ConfigManager.GetActiveDatabase(conString);
            }
            
            switch (activeDatabase)
            {
                case "postgres":
                    return new PostgresDBManager(conString);
                    break;
                case "oracle":
                    return new OracleDBManager(conString);
                    break;
                case "mysql":
                    return new MySqlDBManager(conString);
                    break;
                case "mssql":
                    return new MSSqlDBManager(conString);
                    break;
            }
        
            throw new Exception("No suitable Manager found !!");
        }

    }
}
