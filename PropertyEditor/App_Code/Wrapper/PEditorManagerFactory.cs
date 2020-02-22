using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for PEditorManagerFactory
/// </summary>
namespace HIT.PEditor.Wrapper
{
    public class PEditorManagerFactory
    {
        public static PEditorFunctionsManager GetDBManager()
        {

            string activeDatabase = ConfigurationManager.AppSettings["wrapper_datasource"].ToString();
            switch (activeDatabase)
            {
                case "postgresql":
                    return new PgsqlDBHandler();
                    break;
                case "oracle":
                    return new OracleDBHandler();
                    break;
                default:
                    throw new Exception("No suitable database manager found !!");
                    break;
            }

            throw new Exception("No suitable database manager found !!");
        }
    }
}
