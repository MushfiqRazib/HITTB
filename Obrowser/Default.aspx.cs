using System;
using System.Data;
using System.Web;
using HIT.OB.STD.Wrapper.BLL;
using HIT.OB.STD.Wrapper.DAL;
using System.Web.SessionState;
using Npgsql;
using System.Configuration;

public partial class Default : System.Web.UI.Page
{
    public string SIDValid = "false";
    public string kitServerPath;
    public string authInformation = string.Empty;
    public string repeater = "false";

    protected void Page_Load(object sender, EventArgs e)
    {
        string sid = Request["sid"];
        if (sid == null)
        {
            try
            {
                sid = Session["SECURITY_KEY"].ToString();
            }
            catch
            {
            }
        }

        if (sid != null)
        {
            string url;
            bool userSessionValid = SecurityManager.GetKitServerURLWithValidityCheck(sid, out url);
            if (url.Length > 0)
            {
                kitServerPath = url.Substring(0, url.LastIndexOf("/"));
            }
            kitServerPath = Microsoft.JScript.GlobalObject.escape(kitServerPath);

            if (userSessionValid)
            {
                SIDValid = "true";
                string[] securityInfos = SecurityManager.GetAuthenticationValues(sid);
                authInformation = Microsoft.JScript.GlobalObject.escape(securityInfos[0]);
                repeater = ConfigurationManager.AppSettings["repeater"];
                Session["SECURITY_KEY"] = sid;
            }
            else
            {
                SIDValid = "false";
            }
        }

        HttpSessionState ss = HttpContext.Current.Session;
        Response.Cookies["sessionid"].Value = ss.SessionID;

    }

}
