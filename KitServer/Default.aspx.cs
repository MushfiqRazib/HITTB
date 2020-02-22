using System;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Text;


namespace HITKITServer
{
    public partial class _Default : System.Web.UI.Page
    {
        string roleName = string.Empty;
        string rolePass = string.Empty;
        protected void Page_Load(object sender, EventArgs e)
        {            
            if (!IsPostBack)
            {
                string userId = Request["userid"];
                string password = Request["password"];
                string appl = Request["appl"];
                DoSecurityCheck(userId, password, appl, ref roleName, ref rolePass);
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string userId = txtUserName.Text;
            string password = txtPassword.Text;
            string appl = txtAppl.Text;            
            DoSecurityCheck(userId, password, appl, ref roleName, ref rolePass);
        }

        private void DoSecurityCheck(string userId, string password, string appl, ref string roleName, ref string rolePass)
        {
            IsValidLANUser(userId, password, appl, ref roleName, ref rolePass);
            if (!roleName.Equals(string.Empty) && !rolePass.Equals(string.Empty))
            {
                string print_server_location = ConfigurationManager.AppSettings["print_server_location"];
                string docSharePath = ConfigurationManager.AppSettings["docSharedPath"];
                HITDB_Services.AppServicesForKit cs = new HITDB_Services.AppServicesForKit();
                string kitServerUrl = Request.Url.ToString().Split('?')[0];
                try
                {
                    string securityId = cs.ValidateUserAndGetSecurityId(userId, roleName, rolePass, print_server_location, kitServerUrl, docSharePath);
                    if (!securityId.Equals("-1"))
                    {
                        Response.Redirect(ConfigurationManager.AppSettings[appl.ToUpper()] + "?sid=" + securityId);
                    }
                }
                catch (Exception ex)
                {
                    //lblLoginMsg.Text = ex.Message;
                }
            }
            if (!string.IsNullOrEmpty(userId))
            {
                lblLoginMsg.Text = "Invalid Credentials";
            }
            txtAppl.Text = appl;
            txtUserName.Text = userId;
            txtPassword.Text = password;
        }

        public void IsValidLANUser(string userId, string password, string appl, ref string roleName, ref string rolePass)
        {            
            string line;
            try
            {
                string filePath = Server.MapPath("./UserInfo.txt");
                // Read the file and display it line by line.
                using (System.IO.StreamReader file = new System.IO.StreamReader(filePath))
                {
                    while ((line = file.ReadLine()) != null)
                    {
                        if (line.StartsWith("[") && line.ToLower().Trim(new char[]{'[',']'}).Equals(appl.ToLower()))
                        {
                            while ((line = file.ReadLine()) != null)
                            {
                                if (line.StartsWith("["))
                                {
                                    break;
                                }
                                if (line.StartsWith(userId, StringComparison.OrdinalIgnoreCase))
                                {
                                    string[] creds = line.Split(',');
                                    if (!creds[1].Equals(password))
                                    {
                                        continue;
                                    }
                                    roleName = creds[2];
                                    rolePass = creds[3];
                                    break;
                                }
                            }
                            break;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                //lblLoginMsg.Text = ex.Message;
            }

        }

    }
}