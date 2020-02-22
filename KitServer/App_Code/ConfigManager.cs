using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

/// <summary>
/// Summary description for ConfigManager
/// </summary>
namespace  HITKITServer
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
                return System.Configuration.ConfigurationSettings.AppSettings["connection_string"].ToString().Trim();
            }
        }

        public static string appUrl
        {
            get
            {
                return System.Configuration.ConfigurationSettings.AppSettings["appUrl"].ToString().Trim();
            }
        }
    }
}
