using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Npgsql;
using System.Text;
using System.Collections.Generic;
using HIT.OB.STD.Wrapper.DAL;

/// <summary>
/// Summary description for SecurityManager
/// </summary>
namespace HIT.OB.STD.Wrapper.BLL
{
    public class AuthTypeValue
    {
        public string authType;
        public string[] authValue;
    }
    
    public class SecurityManager
    {
        public SecurityManager()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public static bool IsValidCredential(string roleName, string rolePass)
        {
            try
            {
                return Security_Ext_DBHandler.GetRoleValidationStatus(roleName, rolePass);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public static string InsertSecurityInfo(string userId,string roleName, string print_server_location, string kit_server_url, string docSharePath)
        {
            string securityId = CreateSecurityId(32, true);
            try
            {
                string authString = GetAuthenticationInfo(roleName);
                string authJsonValue = ConvertAuthStringToJson(authString);
                Security_Ext_DBHandler.InsertNewSecurityInfo(userId, securityId, authJsonValue, print_server_location, kit_server_url, docSharePath);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return securityId;

        }

        private static string GetAuthenticationInfo(string roleName)
        {
            DataTable dt = Security_Ext_DBHandler.GetAuthenticationInfo(roleName);
            List<string> lstAuthSections = new List<string>();
            string authSection = string.Empty;
            foreach (DataRow dr in dt.Rows)
            {                
                authSection = dr["authsection"].ToString();
                if (!lstAuthSections.Contains(authSection))
                {
                    lstAuthSections.Add(authSection);
                }            
            }

            AuthTypeValue[] authInfoList = new AuthTypeValue[lstAuthSections.Count];
            AuthTypeValue authTypeValue;
            for (int i = 0; i < lstAuthSections.Count; i++)
            {
                authTypeValue = new AuthTypeValue();
                DataRow[] tmpDrs = dt.Select("authsection='" + lstAuthSections[i] + "'");
                List<string> authValues = new List<string>();

                foreach (DataRow dr in tmpDrs)
                {
                    authValues.Add(dr["functionname"].ToString());
                }
                authTypeValue.authType = lstAuthSections[i];
                authTypeValue.authValue = authValues.ToArray();
                authInfoList[i] = authTypeValue;
            }

            System.Web.Script.Serialization.JavaScriptSerializer jsSlz = new System.Web.Script.Serialization.JavaScriptSerializer();
            return jsSlz.Serialize(authInfoList);
        }

         private static string CreateSecurityId(int size, bool lowerCase)
        {
            StringBuilder builder = new StringBuilder();
            Random seed = new Random();
            Random random = new Random(seed.Next());
            char ch;

            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            if (lowerCase)
                return builder.ToString().ToLower();
            return builder.ToString();
        }

        private static string ConvertAuthStringToJson(string authString)
        {
            string jsonValue = string.Empty;
            System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();
            AuthTypeValue[] authValueList = js.Deserialize<AuthTypeValue[]>(authString);
            
            jsonValue = "{";
            foreach (AuthTypeValue authTValue in authValueList)
            {
                jsonValue += "\"" + authTValue.authType + "\":[";
                foreach (string value in authTValue.authValue)
                {
                    jsonValue += "\"" + value + "\",";
                }
                jsonValue = jsonValue.Remove(jsonValue.LastIndexOf(","));
                jsonValue += "],";
            }
            jsonValue = jsonValue.Remove(jsonValue.LastIndexOf(","));
            jsonValue += "}";
            return jsonValue;
        }

        public static bool GetKitServerURLWithValidityCheck(string securityId, out string url)
        {
            try
            {
                return Security_Ext_DBHandler.GetKitServerURLWithValidityCheck(securityId, out url);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public static string[] GetAuthenticationValues(string securityKey)
        {
            return Security_Ext_DBHandler.GetAuthenticationValues(securityKey);
        }

        
        public static void UpdateLastAccessTime(string securityKey)
        {
            Security_Ext_DBHandler.UpdateLastAccessTime(securityKey);
        }

        internal static void DeleteSecurityKeyInfo(string securityKey)
        {
            Security_Ext_DBHandler.DeleteSecurityKeyInfo(securityKey);
        }

        internal static DataTable GetUserSessionInfo(string securityKey)
        {            
            return Security_Ext_DBHandler.GetUserSessionInfo(securityKey);
        }



    }
}
