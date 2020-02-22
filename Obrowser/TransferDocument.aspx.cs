using System;
using HIT.OB.STD.Wrapper.BLL;
using System.Collections.Generic;

public partial class UploadDocument : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (IsActionUpload())
            {
                upload.Visible = true;
                download.Visible = false;
                btnUpload.Visible = true;
                btnDownload.Visible = false;
                lblTitle.Text = "Upload Document";
            }
            else
            {
                upload.Visible = false;
                download.Visible = true;
                btnUpload.Visible = false;
                btnDownload.Visible = true;
                lblTitle.Text = "Download Template";
                LoadTemplateTypes();
            }
            string[] projCode = WrappingManager.GetProjectCodes();
            drpProject.DataSource = projCode;
            drpProject.DataBind();
        }
    }

    private void LoadTemplateTypes()
    {
        string netDir = System.Configuration.ConfigurationManager.AppSettings["Template-Network-Path"];

        List<Template> templates = HIT.OB.STD.Wrapper.CommonFunctions.GetTemplates(netDir);
        drpDoctype.DataSource = templates;
        drpDoctype.DataTextField = "Type";
        drpDoctype.DataValueField = "Fullname";
        drpDoctype.DataBind();
    }

    private bool IsActionUpload()
    {
        return Request.Params["action"].ToString().Equals("u");
    }

    protected void btnUpload_Click(object sender, EventArgs e)
    {
        try
        {
            string fileName = docUploader.PostedFile.FileName;
            if (fileName.Equals(string.Empty))
            {
                Response.Write("<script>alert('Please select a document you wanna upload.')</script>");
            }
            else
            {
                string newFileName = WrappingManager.GetNewFileName(fileName, drpProject.SelectedValue);
                docUploader.PostedFile.SaveAs(newFileName);
                Response.Write("<script>window.close(); alert('Document uploaded successfully.')</script>");
            }
        }
        catch(Exception ex)
        {
            Response.Write("<script>alert('" + ex.Message + "')</script>");
        }
    }
    protected void btnDownload_Click(object sender, EventArgs e)
    {
        Response.Redirect("OfficeDownloader.aspx?FILE="+ drpDoctype.SelectedValue);
    }
}
