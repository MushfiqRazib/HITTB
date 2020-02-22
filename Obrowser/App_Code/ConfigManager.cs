using System;

using System.Configuration;



/// <summary>
/// Summary description for ConfigManager
/// </summary>
namespace HITKITClient
{
    public class ConfigManager
    {
        public ConfigManager()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public static string ConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["obcore_connectionstring"].ConnectionString; 
            }
        }
    }
}
