using System.Configuration;
using System.Text.RegularExpressions;

/// <summary>
/// Summary description for ConfigManager
/// </summary>
namespace HIT.OB.STD.Wrapper.DAL
{
    public class ConfigManager
    {
        public ConfigManager()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["obwrapper_connectionstring"].ConnectionString;
        }

        public static string GetReportTableName()
        {
            return ConfigurationManager.AppSettings["reporttable"];
        }

        public static string GetActiveDatabase()
        {
            return AnalyzeConnectionStringRetriveDatabaseName(ConfigManager.GetConnectionString());
        }

        public static string GetPartlistQuery()
        {
            return ConfigurationManager.AppSettings["partlistquery"];
        }

        public static string GetLoginTimeout()
        {
            return ConfigurationManager.AppSettings["logintimeout"];
        }

        static string AnalyzeConnectionStringRetriveDatabaseName(string conStr)
        {
            conStr = conStr.ToLower().Replace("  ", " ").Replace("\t", " ");
            // Search Postges

            // Search MS SQL Sever
            if (Regex.Matches(conStr.ToLower(), "initial catalog").Count == 1)
            {
                return "mssql";
            }

            if (Regex.Matches(conStr, "server").Count == 1 && !Regex.IsMatch(conStr, "oracle"))
            {
                return "postgres";
            }

            // Search Postges
            if (Regex.Matches(conStr, "provider").Count == 1 && Regex.Matches(conStr, "oracle").Count == 1)
            {
                return "oracle";
            }

            // Search MySql
            if (Regex.Matches(conStr, "data source").Count == 1)
            {
                return "mysql";
            }

           
           
            return "unknown";
        }

        public static string GetDocBasePath()
        {
            return ConfigurationManager.AppSettings["DocBasePath"];
        }
    }
}
