using System.Configuration;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.OleDb;
using Npgsql;

/// <summary>
/// Summary description for ConfigManager
/// </summary>
namespace HIT.OB.STD.Core.DAL
{
    public class ConfigManager
    {

        public static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["obcore_connectionstring"].ConnectionString;
        }

        /// <summary>
        /// Analyze on the provided connection string
        /// </summary>
        /// <returns>Type Name of the Database</returns>
        public static string GetActiveDatabase(string conString)
        {
            return AnalyzeConnectionStringRetriveDatabaseName(conString);
        }
        
        /// <summary>
        /// Analyze on the default Connection String defined in web.config
        /// </summary>
        /// <returns>Type Name of the Database</returns>
        public static string GetActiveDatabase()
        {
            return AnalyzeConnectionStringRetriveDatabaseName(GetConnectionString());
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

    }
}
