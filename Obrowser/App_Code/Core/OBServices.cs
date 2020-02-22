using System;
using System.Collections;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Linq;
using System.Web.Script.Services;
using System.Text.RegularExpressions;
using HIT.OB.STD.Core.BLL;
/// <summary>
/// Summary description for OBWebServices
/// </summary>
namespace HIT.OB.STD.Core.Services
{
    [WebService(Namespace = "http://hawarit.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ScriptService]
    public class OBServices : System.Web.Services.WebService
    {
        public OBServices()
        {
            //Uncomment the following line if using designed components 
            //InitializeComponent();             
        }

        [WebMethod]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json, XmlSerializeString = false)]
        public string HelloWorld(string msg)
        {            
            return "Hello World " + msg;
        }


        [WebMethod(true)]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json, XmlSerializeString = false)]
        public string GetNormalGridData(string REPORT_CODE, string FIELD_CAPS, string SQL_SELECT,
                                        string SQL_FROM, string SQL_WHERE, string SQL_ORDER_BY, string SQL_ORDER_DIR, string START_ROW,
                                        string PAGE_SIZE, string SQL_MANDATORY, string MULTI_SELECT,
                                        string FUNCTION_LIST)
        {

            try
            {
                //*** decode ecoded single quite(') to char
                REPORT_CODE = Microsoft.JScript.GlobalObject.unescape(REPORT_CODE).Replace("@@@", "\"");
                FIELD_CAPS = Microsoft.JScript.GlobalObject.unescape(FIELD_CAPS).Replace("@@@", "\"");
                SQL_SELECT = Microsoft.JScript.GlobalObject.unescape(SQL_SELECT).Replace("@@@", "\"");
                SQL_FROM = Microsoft.JScript.GlobalObject.unescape(SQL_FROM).Replace("@@@", "\"");
                SQL_WHERE = Microsoft.JScript.GlobalObject.unescape(SQL_WHERE).Replace("@@@", "\"").Replace("$$$$","");
                SQL_ORDER_BY = Microsoft.JScript.GlobalObject.unescape(SQL_ORDER_BY).Replace("@@@", "\"");
                SQL_MANDATORY = Microsoft.JScript.GlobalObject.unescape(SQL_MANDATORY).Replace("@@@", "\"");
                FUNCTION_LIST = Microsoft.JScript.GlobalObject.unescape(FUNCTION_LIST).Replace("@@@", "\"");

                string gridData = OBController.GetNormalGridData(REPORT_CODE, FIELD_CAPS, SQL_SELECT, 
                                            SQL_FROM, SQL_WHERE,  SQL_ORDER_BY, SQL_ORDER_DIR, START_ROW,
                                            PAGE_SIZE, SQL_MANDATORY,MULTI_SELECT, FUNCTION_LIST);
                return gridData;
            }
            catch (Exception ex)
            {
                return ex.StackTrace;
            }
        }

        

        [WebMethod(true)]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json, XmlSerializeString = false)]
        public string GetGroupByGridData(string REPORT_CODE, string FIELD_CAPS, string SQL_SELECT,
                                        string SQL_FROM, string SQL_WHERE,  string SQL_ORDER_BY, string QB_GB_SELECT_CLAUSE, string SQL_ORDER_DIR, string START_ROW,
                                        string PAGE_SIZE, string SQL_GROUP_BY, string MULTI_SELECT, string GIS_THEME_LAYER)
        {

            try
            {
                //*** decode ecoded single quite(') to char
                REPORT_CODE = Microsoft.JScript.GlobalObject.unescape(REPORT_CODE).Replace("@@@","\"");
                FIELD_CAPS = Microsoft.JScript.GlobalObject.unescape(FIELD_CAPS).Replace("@@@", "\"");
                SQL_SELECT = Microsoft.JScript.GlobalObject.unescape(SQL_SELECT).Replace("@@@", "\"").Replace(';',',');
                SQL_FROM = Microsoft.JScript.GlobalObject.unescape(SQL_FROM).Replace("@@@", "\"");
                SQL_WHERE = Microsoft.JScript.GlobalObject.unescape(SQL_WHERE).Replace("@@@", "\"");
                
                SQL_ORDER_BY = Microsoft.JScript.GlobalObject.unescape(SQL_ORDER_BY).Replace("@@@", "\"");
                QB_GB_SELECT_CLAUSE = Microsoft.JScript.GlobalObject.unescape(QB_GB_SELECT_CLAUSE).Replace("@@@", "\"");

                return OBController.GetGroupByGridData(REPORT_CODE, FIELD_CAPS, SQL_SELECT, 
                                            SQL_FROM, SQL_WHERE, SQL_ORDER_BY, SQL_ORDER_DIR, START_ROW,
                                            PAGE_SIZE, SQL_GROUP_BY, MULTI_SELECT, QB_GB_SELECT_CLAUSE , GIS_THEME_LAYER);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [WebMethod(true)]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json, XmlSerializeString = false)]
        public string GetFieldValues(string REPORT_CODE, string SQL_FROM, string FIELD_NAME)
        {
            try
            {
                REPORT_CODE = Microsoft.JScript.GlobalObject.unescape(REPORT_CODE).Replace("@@@", "\""); 
                SQL_FROM = Microsoft.JScript.GlobalObject.unescape(SQL_FROM).Replace("@@@", "\""); 
                FIELD_NAME = Microsoft.JScript.GlobalObject.unescape(FIELD_NAME).Replace("@@@", "\""); 

                return OBController.GetFieldValues(REPORT_CODE, SQL_FROM, FIELD_NAME);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [WebMethod(true)]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json, XmlSerializeString = false)]
        public string CheckCustomFieldValidation(string REPORT_CODE, string SQL_FROM, string customFields)
        {
            try
            {
                REPORT_CODE = Microsoft.JScript.GlobalObject.unescape(REPORT_CODE).Replace("@@@", "\""); 
                SQL_FROM = Microsoft.JScript.GlobalObject.unescape(SQL_FROM).Replace("@@@", "\""); ;
                customFields = Microsoft.JScript.GlobalObject.unescape(customFields).Replace(';',',');
                return OBController.CheckCustomFieldValidation(REPORT_CODE, SQL_FROM, customFields);
            }
            catch (Exception ex)
            {
                return ex.StackTrace;
            }
        }

        
        [WebMethod(true)]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json, XmlSerializeString = false)]
        public string ValidateWhereClause(string REPORT_CODE, string SQL_FROM, string whereClause)
        {
            try
            {
                REPORT_CODE = Microsoft.JScript.GlobalObject.unescape(REPORT_CODE).Replace("@@@", "\"");
                SQL_FROM = Microsoft.JScript.GlobalObject.unescape(SQL_FROM).Replace("@@@", "\"");
                whereClause = Microsoft.JScript.GlobalObject.unescape(whereClause).Replace("@@@", "\"");
                return OBController.ValidateWhereClause(REPORT_CODE, SQL_FROM, whereClause);
            }
            catch (Exception ex)
            {
                return ex.StackTrace;
            }
        }

        [WebMethod(true)]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json, XmlSerializeString = false)]
        public string GetFieldNameType(string REPORT_CODE, string SQL_FROM)
        {
            try
            {
                REPORT_CODE = Microsoft.JScript.GlobalObject.unescape(REPORT_CODE).Replace("@@@", "\"");
                SQL_FROM = Microsoft.JScript.GlobalObject.unescape(SQL_FROM).Replace("@@@", "\"");

                return OBController.GetFieldNameType(REPORT_CODE, SQL_FROM);
            }
            catch (Exception ex)
            {
                return "Syntax Error!";
            }
        }

        [WebMethod(true)]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json, XmlSerializeString = false)]
        public string CheckGroupBySelectValidation(string REPORT_CODE, string SQL_FROM, string QB_GB_SELECT_CLAUSE)
        {
            try
            {
                REPORT_CODE = Microsoft.JScript.GlobalObject.unescape(REPORT_CODE);
                SQL_FROM = Microsoft.JScript.GlobalObject.unescape(SQL_FROM).Replace("@@@", "\""); ;
                QB_GB_SELECT_CLAUSE = Microsoft.JScript.GlobalObject.unescape(QB_GB_SELECT_CLAUSE).Replace(';', ',');

                return OBController.CheckGroupBySelectValidation(REPORT_CODE, SQL_FROM, QB_GB_SELECT_CLAUSE);
            }
            catch (Exception ex)
            {
                return ex.StackTrace;
            }
        }
        
        
    }

}
