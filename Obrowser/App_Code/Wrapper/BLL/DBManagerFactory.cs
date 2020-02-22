using System;
using HIT.OB.STD.Wrapper.DAL;

/// <summary>
/// Summary description for DBManaFactory
/// </summary>
/// 
namespace HIT.OB.STD.Wrapper.BLL
{
    public class DBManagerFactory
    {
       
        public IWrapFunctions GetDBManager()
        {
            string activeDatabase = ConfigManager.GetActiveDatabase();
            switch (activeDatabase)
            {
                case "postgres":
                    return new PostgresDBManager();
                    break;
                case "oracle":
                    return new OracleDBManager();
                    break;
                case "mysql":
                    return new MySqlDBManager();
                    break;
                case "mssql":
                    return new MSSqlDBManager();
                    break;
            }
        
            throw new Exception("No suitable Manager found !!");
        }

    }
}
