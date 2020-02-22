using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Collections;
using System.Xml;
using System.IO;
using System.Text;
using System.Data.OleDb;
using HIT.PEditor.Wrapper;
using Npgsql;
using System.Xml.Linq;


public partial class Wrapper : System.Web.UI.Page
{
    public string fileName = string.Empty;
    public string tableName = string.Empty;
    public string fieldNames = string.Empty;
    public string fieldValues = string.Empty;
    public string groupName = string.Empty;
    public string mode = string.Empty;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (Session["sessionid"] == null)
            {
                Session["sessionid"] = Session.SessionID;
            }

            fileName = Session["sessionid"].ToString() + ".xml";

            tableName = Request.Params["tableName"];
            fieldNames = Request.Params["fieldNames"];
            fieldValues = Request.Params["fieldValues"];
            groupName = Request.Params["groupName"];
            mode = Request.Params["mode"];
            Session["pageSize"] = Request.Params["pageSize"];
            if (!String.IsNullOrEmpty(fieldValues))
            {
                fieldValues = fieldValues.Replace("@$@", "&");
            }

            //tableName = "naw_best";
            //fieldNames = "zoeknaam";
            //fieldValues = "A & X /DORDRECHT";
            //groupName = "naw_best";


        //http://localhost/PropertyEditor/Wrapper.aspx?tableName=naw_best&fieldNames=zoeknaam&fieldValues=ABB%20GLOBAL/LEIDEN&groupName=naw_best
			
            try
            {
                WrapperAppParams.SetParams(fileName, tableName, fieldNames, fieldValues, groupName);
                PEditorFunctionsManager peditorManager = PEditorManagerFactory.GetDBManager();

                //string activeDatabase = ConfigurationManager.AppSettings["activedatabase"].ToString();
                if (Request.Params["erroroccur"] != null)
                {
                    string errorMsg = ErrorList.GetErrorMessage(GetErrorCode());
                    ScriptManager.RegisterStartupScript(Page, typeof(Page), "error", "<script>ErrorLoadingProperyEditor();alert('Fail to load property editor-\\n" + errorMsg + "');</script>", false);

                }
                else
                {
                    if (Request.Params["savedata"] != null)
                    {
                        try
                        {
                            if (Request.Params["mode"] != null && Request.Params["mode"].Equals("add"))
                            {
                                peditorManager.addData();
                                Response.Write("Data added successfully.");
                            }
                            else
                            {
                                peditorManager.SaveData();
                                Response.Write("Data updated successfully.");
                            }
                        }
                        catch (Exception exp)
                        {
                            Response.Write("Cannot update data;Following error occured:\n" + exp.Message);
                        }
                    }
                    else
                    {
                        ScriptManager.RegisterStartupScript(Page, typeof(Page), "errorCore", "<script> timerObj= setInterval(ShowError, 500);</script>", false);
                        peditorManager.WritePropertyXML();

                    }
                }

            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(Page, typeof(Page), "errorWpr", "<script>ErrorLoadingProperyEditor();alert('" + ex.Message + "');</script>", false);
                //Response.Write(ex.Message);
            }



        }
    }

    string GetErrorCode()
    {
        string errorFile = Session["sessionid"].ToString() + "-Error.xml";
        XElement xmlElement = XElement.Load(Server.MapPath("Output") + "\\" + errorFile);
        string code = xmlElement.Descendants("code").Single<XElement>().Value;
        return code;

    }

}
