using System;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Collections;
using AjaxControlToolkit;
using HIT.PEditor.Core;
using System.Xml;

public partial class PropertyEditor : System.Web.UI.Page
{
    int MaxDisplayLength = 1000;
    int MinDisplayLength = 100;
    int inputBoxMaxLength = 0;
    int lebalMaxChar = 0;
    string activeDB = "";
    string conString = "";
    string conStringLOV_SQL = "";
    int CharLength = 7;


    protected void Page_Error(object sernder, EventArgs e)
    {
        //Session["LastError"] = Server.GetLastError();
        //Server.Transfer("ErrorPage.aspx");
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            Page.MaintainScrollPositionOnPostBack = true;
            string fileLocation = HttpContext.Current.Server.MapPath("Output") + "\\";
            lblError.Text = "";
            if (Request.QueryString["file"] != null)
            {
                XMLHandler parser = new XMLHandler(fileLocation, Request.QueryString["file"].ToString());
                Dictionary<string, string> fieldsToShow = new Dictionary<string, string>();
                try
                {
                    fieldsToShow = parser.GetFields();
                }
                catch (Exception exp)
                {
                    ShowErrorMessage(exp.Message);
                    return;
                }
                Session["XElement"] = parser.XmlElement;
                string groupName = parser.XmlElement.Descendants("group").Single<XElement>().Value;
                conString = parser.XmlElement.Descendants("connectionString").Single<XElement>().Value;
                conStringLOV_SQL = parser.XmlElement.Descendants("wrapperConnectionString").Single<XElement>().Value;
                activeDB = parser.XmlElement.Descendants("activeDatabase").Single<XElement>().Value;
                string metaTableName = parser.XmlElement.Descendants("metaTable").Single<XElement>().Value;

                LoadRestrictions(metaTableName, groupName);
                if (!CheckFieldsValidityInXmlFile(ref fieldsToShow))
                {
                    string errMsg = "All fields are invalid  in " + Request.QueryString["file"].ToString();
                    ShowErrorMessage(errMsg);
                    return;
                }


                CreateValidationForEachField(fieldsToShow);
                RenderPageWithFields(fieldsToShow);
            }
            else
            {
                ShowErrorMessage("File name is not correct/File name is not passed with url.");
            }
        }
        catch (Exception ex)
        {
            ErrorOccured(ex.Message);
            //LogWriter.WriteLog(ex.Message);
        }


    }

    void ErrorOccured(string errMsg)
    {
        XmlDocument xml = new XmlDocument();

        XmlElement root = xml.CreateElement("Error");
        xml.AppendChild(root);

        XmlElement errCode = xml.CreateElement("code");
        errCode.InnerText = errMsg;
        root.AppendChild(errCode);


        string fileLocation = HttpContext.Current.Server.MapPath("Output") + "\\" + Session["sessionid"] + "-Error.xml";

        System.IO.StreamWriter sw = new System.IO.StreamWriter(fileLocation, false);

        sw.WriteLine(xml.InnerXml);
        sw.Close();

        ScriptManager.RegisterStartupScript(Page, typeof(Page), "error", "<script>dataSaved='error';</script>", false);
    }

    private void ShowErrorMessage(string msg)
    {
        lblError.ForeColor = System.Drawing.Color.Red;
        lblError.Text = msg;
        btnSave.Visible = false;
    }

    private bool CheckFieldsValidityInXmlFile(ref Dictionary<string, string> fieldsToShow)
    {
        Dictionary<string, string> tmpNewFieldToShow = new Dictionary<string, string>();
        DataTable dt = (DataTable)Session["metadata"];
        int tmpCounter = 0;
        foreach (var field in fieldsToShow)
        {
            if (dt.Select("FieldName ='" + field.Key + "'").Length == 0)
            {
                tmpCounter++;
            }
            else
            {
                tmpNewFieldToShow.Add(field.Key, field.Value);
            }
        }

        if (tmpCounter == fieldsToShow.Count)
        {
            return false;
        }
        else if (tmpCounter > 0)
        {
            fieldsToShow = tmpNewFieldToShow;
            return true;
        }
        else return true;
    }

    private void CreateValidationForEachField(Dictionary<string, string> fieldsToShow)
    {
        Dictionary<string, PEValidator> validatorList = new Dictionary<string, PEValidator>();
        DataTable dt = (DataTable)Session["metadata"];
        foreach (var item in fieldsToShow)
        {
            DataRow row;
            if (dt.Select("FieldName ='" + item.Key + "'").Length != 0)
            {
                row = dt.Select("FieldName ='" + item.Key + "'")[0];
                validatorList.Add(item.Key, GetFieldValidator(row, item));
            }
        }

        Session.Remove("metadata");
        Session["validatorDictionary"] = validatorList;


        // Calculate Actual Dispaly Length
        int defLen_Max = GetMaxDefinedLength(validatorList);
        int undefLen_Max = GetMaxUndefinedLength(validatorList, fieldsToShow);
        int actualLength = inputBoxMaxLength = Math.Max(defLen_Max, undefLen_Max);

        foreach (var item in validatorList)
        {
            if (item.Value.DisplayLength == 0)
            {
                item.Value.ActualLength = actualLength;
            }
            else if (item.Value.DisplayLength > MaxDisplayLength || item.Value.DisplayLength < MinDisplayLength)
            {
                if (item.Value.DisplayLength > MaxDisplayLength)
                    item.Value.ActualLength = MaxDisplayLength;

                if (item.Value.DisplayLength < MinDisplayLength)
                    item.Value.ActualLength = MinDisplayLength;
            }
            else
            {
                item.Value.ActualLength = item.Value.DisplayLength;
            }

        }

        // Calculate Label's Max Character
        lebalMaxChar = GetLabelsMaxCharacter(validatorList, fieldsToShow);
    }

    int GetLabelsMaxCharacter(Dictionary<string, PEValidator> validatorList, Dictionary<string, string> fieldsToShow)
    {
        int charCount = 0;
        foreach (var item in fieldsToShow)
        {
            PEValidator validator = validatorList[item.Key];
            string caption = validator.Caption != string.Empty ? validator.Caption : item.Key;

            if (charCount < caption.Length)
                charCount = caption.Length;

        }

        return charCount;
    }

    int GetMaxUndefinedLength(Dictionary<string, PEValidator> validatorList, Dictionary<string, string> fieldsToShow)
    {
        int returnLength = 0;
        foreach (var item in fieldsToShow)
        {
            PEValidator validator = validatorList[item.Key];
            string defaultValue = validator.DefaultValue != null ? validator.DefaultValue.ToString() : string.Empty;
            string value = item.Value.Trim().Equals(string.Empty) ? defaultValue : item.Value.Trim();
            int valueLength = value.Length * CharLength;
            if (returnLength < valueLength)
                returnLength = valueLength;

        }

        if (returnLength > MaxDisplayLength)
        {
            returnLength = MaxDisplayLength;
        }

        if (returnLength < MinDisplayLength)
        {
            returnLength = MinDisplayLength;
        }
        return returnLength;
    }

    int GetMaxDefinedLength(Dictionary<string, PEValidator> validatorList)
    {
        int returnLength = 0;
        foreach (var item in validatorList)
        {
            PEValidator validator = item.Value;
            if (validator.DisplayLength != 0)
            {
                if (returnLength < validator.DisplayLength)
                    returnLength = validator.DisplayLength;
            }
        }

        if (returnLength > MaxDisplayLength)
        {
            returnLength = MaxDisplayLength;
        }

        if (returnLength < MinDisplayLength)
        {
            returnLength = MinDisplayLength;
        }
        return returnLength;
    }

    private PEValidator GetFieldValidator(DataRow restrictions, KeyValuePair<string, string> item)
    {
        PEValidator validator = new PEValidator();
        //DataType curType = (DataType)Enum.Parse(typeof(DataType), validator.ValueType, true);
        string propertyValue = item.Value;
        string fieldName = item.Key;
        string type = restrictions["fieldtype"].ToString().ToLower();
        string allowEdit = restrictions["allowedit"].ToString().ToLowerInvariant();


        validator.FieldName = fieldName;
        validator.Mandatory = (EnumMandatory)Enum.Parse(typeof(EnumMandatory), restrictions["mandatory"].ToString(), true);

        if (allowEdit.Equals('t') || allowEdit.Equals("true"))
        {
            validator.AllowEdit = true;
        }
        else
        {
            validator.AllowEdit = false;
        }
        switch (type)
        {
            case "int":
                validator.ValueType = DataType.Integer.ToString();
                validator.MinValueInt = restrictions["minvalue"].ToString() != string.Empty ? Convert.ToInt32(restrictions["minvalue"]) : 0;
                validator.MaxValueInt = restrictions["maxvalue"].ToString() != string.Empty ? Convert.ToInt32(restrictions["maxvalue"]) : 0;
                break;
            case "float":
                validator.ValueType = DataType.Float.ToString();
                validator.MinValueFloat = restrictions["minvalue"].ToString() != string.Empty ? Convert.ToDouble(restrictions["minvalue"]) : 0;
                validator.MaxValueFloat = restrictions["maxvalue"].ToString() != string.Empty ? Convert.ToDouble(restrictions["maxvalue"]) : 0;
                validator.MaxDecimals = restrictions["decimals"].ToString() != string.Empty ? Convert.ToInt32(restrictions["decimals"]) : 0;
                break;
            case "str":
                validator.ValueType = DataType.String.ToString();
                validator.MaxStringLength = restrictions["strlen"].ToString() != string.Empty ? Convert.ToInt32(restrictions["strlen"]) : 0;
                break;

            case "date":
                try
                {
                    //propertyValue = Convert.ToDateTime(item.Value).ToString("dd-MM-yyyy");
                    propertyValue = item.Value;
                }
                catch
                {
                }
                validator.ValueType = DataType.Date.ToString();
                break;
            case "lastupdate":
                propertyValue = Convert.ToDateTime(item.Value).ToString("dd-MM-yyyy");
                validator.ValueType = DataType.Lastupdate.ToString();
                validator.AllowEdit = false;
                validator.Mandatory = EnumMandatory.No;
                break;

            case "objlen":

                validator.ValueType = DataType.Objlen.ToString();
                validator.AllowEdit = false;
                break;
            case "objarea":
                validator.ValueType = DataType.Objarea.ToString();
                validator.AllowEdit = false;
                break;
            case "text":
                validator.ValueType = DataType.Text.ToString();
                validator.MaxStringLength = restrictions["strlen"].ToString() != string.Empty ? Convert.ToInt32(restrictions["strlen"]) : 0;
                break;
            default:
                break;
        }

        validator.Caption = restrictions["caption"] != null ? restrictions["caption"].ToString().Trim() : string.Empty;
        validator.DefaultValue = restrictions["default"].ToString() != "" ? restrictions["default"] as object : null;
        validator.ExtractedValue = propertyValue;
        validator.ToolTip = restrictions["tip"].ToString().Trim();

        if (restrictions["displen"] != null && restrictions["displen"].ToString() != "")
        {
            validator.DisplayLength = Convert.ToInt32(restrictions["displen"]);
        }
        else
        {
            validator.DisplayLength = 0;
        }
        //LogWriter.WriteLog("rest: " + restrictions["allowedit"]);
        //allowError = false;
        //string allowErr = restrictions["allowerror"].ToString().ToLowerInvariant();
        //if (allowErr.Equals('t') || allowErr.Equals("true"))
        //{
        //    allowError = true;
        //}
        //validator.AllowError = allowError;

        validator.ErrorLevel = Convert.ToInt16(restrictions["errorlevel"]);
        //if (validator.Mandatory == EnumMandatory.Lov)
        //{
        if (restrictions["lovcp"].ToString().Trim() != string.Empty)
        {
            validator.LOVValue = restrictions["lovcp"].ToString().Trim();
            validator.LovType = EnumLovType.Lovcp;
        }
        else if (restrictions["lovp"].ToString().Trim() != string.Empty)
        {
            validator.LOVValue = restrictions["lovp"].ToString().Trim();
            validator.LovType = EnumLovType.Lovp;
        }
        else if (restrictions["lovc"].ToString().Trim() != string.Empty)
        {
            validator.LOVValue = restrictions["lovc"].ToString().Trim();
            validator.LovType = EnumLovType.Lovc;
        }
        else
        {
            validator.LovType = EnumLovType.None;
        }
        //}

        return validator;
    }

    private void LoadRestrictions(string tableName, string groupName)
    {
        if (conString == "")
        {
            throw new Exception("CONN_STR_ERROR");
        }

        IDatabaseFunctionsManager dbHlr = DBManagerFactory.GetDBManager(activeDB, conString);
        DataTable dt = dbHlr.GetDataTable(dbHlr.GetQuery(tableName, groupName));

        Session["metadata"] = dt;
    }

    private void RenderPageWithFields(Dictionary<string, string> fieldsToShow)
    {
        Dictionary<string, PEValidator> validatorList = Session["validatorDictionary"] as Dictionary<string, PEValidator>;
        Dictionary<string, string> controlIDs = new Dictionary<string, string>();
        Dictionary<string, ControlType> controlDControlType = new Dictionary<string, ControlType>();
        string value;
        foreach (var item in fieldsToShow)
        {
            PEValidator validator = validatorList[item.Key];
            if (validator.ValueType.Equals("Date"))
            {
                try
                {
                    //value = Convert.ToDateTime(item.Value).ToString("dd-MM-yyyy");
                    value = item.Value;
                }
                catch
                {
                    value = item.Value;
                }
            }

            else
            {
                value = item.Value;
            }
            ControlIDWithType elementIDType = CreateElement(item.Key, value, validator);

            controlIDs.Add(elementIDType.ID, item.Key);
            controlDControlType.Add(elementIDType.ID, elementIDType.Type);
        }
        Session["controlIDsWithType"] = controlDControlType;
        Session["controlIDs"] = controlIDs;
    }

    private ControlIDWithType CreateElement(string fieldName, string userInputValue, PEValidator validator)
    {

        ControlIDWithType idType = new ControlIDWithType();
        string value = userInputValue;
        if (value.Trim() == string.Empty && validator.DefaultValue != null)
        {
            value = validator.DefaultValue.ToString();
        }

        Label lbl = new Label();
        lbl.ID = string.Concat("lbl", fieldName);
        lbl.Style.Add("width", "100%");
        if (validator.Caption != string.Empty)
            lbl.Text = validator.Caption.ToFirstCharUpper();
        else
            lbl.Text = fieldName.ToFirstCharUpper();

        HtmlGenericControl divLabels = new HtmlGenericControl("div");
        divLabels.Style.Add("padding-right", "10px");
        divLabels.Style.Add("text-align", "right");
        divLabels.Controls.Add(lbl);

        TextBox txtBx = new TextBox();
        if (validator.ValueType.ToString().ToLower().Equals("text"))
        {
            txtBx.TextMode = TextBoxMode.MultiLine;
            txtBx.Rows = validator.MaxStringLength;
            txtBx.Style.Add("font-family", "Verdana");
            txtBx.Style.Add("font-size", "9pt");
        }
        DropDownList ddl = new DropDownList();

        HtmlGenericControl divControl = new HtmlGenericControl("div");
        DataType curType = (DataType)Enum.Parse(typeof(DataType), validator.ValueType, true);

        if (validator.LovType == EnumLovType.Lovc || validator.LovType == EnumLovType.Lovcp)
        {
            ddl.ID = string.Concat("ddl", fieldName);
            ddl.ToolTip = validator.ToolTip;
            ddl.Attributes.Add("onchange", "SelectDDLValue(this,'txtddl" + fieldName + "')");

            ddl.Style.Add("z-index", "-100");
            hdnDrpIDs.Value += ddl.ID + ",";


            if (validator.LovType != EnumLovType.Lovc || (validator.LovType == EnumLovType.Lovc && validator.LOVValue.Trim().ToLower().StartsWith("select")))
            {
                IDatabaseFunctionsManager db = DBManagerFactory.GetDBManager(activeDB, conStringLOV_SQL);
                DataTable dt = db.GetDataTable(validator.LOVValue);
                if (IsColumnExistInTable(dt, fieldName))
                {
                    if (curType != DataType.Date)
                    {
                        ddl.DataSource = dt;
                        ddl.DataTextField = fieldName;
                        ddl.DataValueField = fieldName;

                    }
                    else
                    {
                        List<string> dates = new List<string>();
                        foreach (DataRow record in dt.Rows)
                        {
                            if (record[fieldName].ToString().Trim() != "")
                            {
                                dates.Add(Convert.ToDateTime(record[fieldName]).ToString("dd-MM-yyyy"));
                            }
                        }

                        ddl.DataSource = dates;
                    }
                }
            }
            else
            {
                char delimiter = validator.LOVValue[0];
                //if (validator.LOVValue.StartsWith("/"))
                //    delimiter = '/';
                //else if (validator.LOVValue.StartsWith(";"))
                //    delimiter = ';';
                //else delimiter = ',';

                string[] valueList = validator.LOVValue.Split(delimiter);
                ddl.DataSource = valueList.Where<string>(val => val != string.Empty).ToList<string>();
            }

            ddl.DataBind();

            for (int i = 0; i < ddl.Items.Count; i++)
            {
                if (ddl.Items[i].Value.ToLower() == userInputValue.ToLower())
                {
                    ddl.Items[i].Selected = true;
                    break;
                }
            }

            divControl.Controls.Add(ddl);
            idType.ID = ddl.ID;
            idType.Type = ControlType.DropDownList;

            if (validator.AllowEdit && validator.LovType == EnumLovType.Lovcp)
            {
                ddl.Style.Add("width", validator.ActualLength - 14 + "px");
            }
            else
            {
                ddl.Style.Add("width", validator.ActualLength + 4 + "px");
            }



            if (validator.AllowEdit)
            {
                if (validator.LovType == EnumLovType.Lovcp)
                {

                    CreatePickButton(ddl.ID, fieldName, ref divControl);
                }

                // Text box for editing dropdown list.

                txtBx.ID = string.Concat("txtddl", fieldName);
                txtBx.Style.Add("position", "absolute");
                txtBx.Style.Add("display", "none");
                txtBx.Style.Add("border-style", "none");
                
                if (!userInputValue.Trim().Equals(string.Empty))
                {
                    if (ddl.SelectedValue != null && ddl.SelectedValue != "")
                    {
                        if (ddl.SelectedValue.Trim().ToLower() == userInputValue.Trim().ToLower())
                        {
                            txtBx.Text = ddl.SelectedValue;
                        }
                        else
                        {
                            txtBx.Text = userInputValue;
                        }
                    }
                }
                else
                {
                    txtBx.Text = ddl.SelectedValue;
                }
                divControl.Controls.Add(txtBx);
                txtBx.ToolTip = validator.ToolTip;


            }
            else
            {

                ddl.Enabled = false;
            }

            //if (!validator.IsInitialValueValid(value))
            //{
            //    ddl.ForeColor = System.Drawing.Color.Red;
            //}


        }
        else
        {
            txtBx.ID = string.Concat("txt", fieldName);
            txtBx.Text = value;
            divControl.Controls.Add(txtBx);
            txtBx.ToolTip = validator.ToolTip;
            txtBx.Style.Add("width", validator.ActualLength + "px");

            if (validator.AllowEdit && (validator.LovType == EnumLovType.Lovp || curType == DataType.Date))
            {
                txtBx.Style.Add("width", validator.ActualLength - 18 + "px");
            }
            else
            {
                txtBx.Style.Add("width", validator.ActualLength + "px");
            }

            if (validator.AllowEdit)
            {
                if (validator.LovType == EnumLovType.Lovp)
                {
                    CreatePickButton(txtBx.ID, fieldName, ref divControl);
                }
            }
            else
            {
                txtBx.ReadOnly = true;
            }


            switch (curType)
            {
                case DataType.Integer:
                    if (validator.MinValueInt >= 0 && validator.MaxValueInt > 0 && validator.ErrorLevel == 2)
                    {
                        RangeValidator rv = new RangeValidator();
                        rv.ID = string.Concat("rv", fieldName);
                        rv.Type = ValidationDataType.Integer;

                        rv.MinimumValue = validator.MinValueInt.ToString();
                        rv.MaximumValue = validator.MaxValueInt.ToString();
                        rv.ControlToValidate = txtBx.ID;
                        rv.ErrorMessage = string.Format("Value should be between {0} and {1}", validator.MinValueInt, validator.MaxValueInt);
                        rv.ValidationGroup = "peValidation";
                        divControl.Controls.Add(rv);
                    }
                    break;
                case DataType.Float:
                    if (validator.ErrorLevel == 2)
                    {
                        if (validator.MinValueInt > 0 && validator.MaxValueInt > 0)
                        {
                            RangeValidator rv = new RangeValidator();
                            rv.ID = string.Concat("rv", fieldName);
                            rv.Type = ValidationDataType.Double;
                            rv.MinimumValue = validator.MinValueFloat.ToString();
                            rv.MaximumValue = validator.MaxValueFloat.ToString();
                            rv.ControlToValidate = txtBx.ID;
                            rv.ErrorMessage = string.Format("Value should be between {0} and {1}", validator.MinValueFloat, validator.MaxValueFloat);
                            rv.ValidationGroup = "peValidation";
                            divControl.Controls.Add(rv);
                        }

                    }
                    break;
                case DataType.String:
                    //if (validator.MaxStringLength > 0)
                    //{
                    //    txtBx.MaxLength = validator.MaxStringLength;
                    //}
                    if (validator.ErrorLevel == 2)
                    {
                        CustomValidator cv = new CustomValidator();
                        cv.ID = string.Concat("cv", fieldName);
                        cv.ValidationGroup = "peValidation";
                        cv.ControlToValidate = txtBx.ID;
                        cv.ValidateEmptyText = true;
                        cv.ServerValidate += new ServerValidateEventHandler(CustomValidator_Handler);
                        divControl.Controls.Add(cv);
                    }
                    break;

                case DataType.Date:
                    if (value.Trim() == string.Empty)
                    {
                        txtBx.Text = value = DateTime.Today.ToString("dd-MM-yyyy");
                    }

                    if (validator.AllowEdit == true && validator.LovType != EnumLovType.Lovp)
                    {
                        ImageButton imgBtn = new ImageButton();
                        imgBtn.ID = string.Concat("imgBtn", fieldName);
                        imgBtn.ImageUrl = "Images/calendericon.png";
                        imgBtn.Attributes.Add("onmouseover", "ShowCalander(this)");

                        CalendarExtender calander = new CalendarExtender();
                        calander.ID = string.Concat("cal", fieldName);
                        calander.TargetControlID = txtBx.ID;
                        calander.PopupButtonID = imgBtn.ID;
                        calander.Format = "dd-MM-yyyy";

                        divControl.Controls.Add(imgBtn);
                        divControl.Controls.Add(calander);
                    }
                    if (validator.ErrorLevel == 2)
                    {
                        CustomValidator cv = new CustomValidator();
                        cv.ID = string.Concat("cv", fieldName);
                        cv.ValidationGroup = "peValidation";
                        cv.ControlToValidate = txtBx.ID;
                        cv.ValidateEmptyText = true;
                        cv.ServerValidate += new ServerValidateEventHandler(CustomValidator_Handler);
                        divControl.Controls.Add(cv);
                    }

                    break;
                default:
                    break;
            }

            if (validator.Mandatory == EnumMandatory.Yes && validator.ErrorLevel != 3)
            {
                RequiredFieldValidator rfv = new RequiredFieldValidator();
                rfv.ID = string.Concat("rfv", fieldName);
                rfv.ControlToValidate = txtBx.ID;
                rfv.ErrorMessage = "Required";
                rfv.ValidationGroup = "peValidation";
                divControl.Controls.Add(rfv);

            }

            idType.ID = txtBx.ID;
            idType.Type = ControlType.TextBox;

        }


        // Check Initail value is correct or has an error.
        // If error than make it Red color.

        bool foundError = false;

        if (validator.Mandatory == EnumMandatory.Lov && userInputValue.Trim() != string.Empty)
        {
            //if (validator.LovType != EnumLovType.Lovc && (validator.LovType == EnumLovType.Lovc && validator.LOVValue.ToLower().StartsWith("select")))
            if (validator.LovType != EnumLovType.Lovp)
            {
                // dropdown control
                if (!CheckInputConsistentWithList(ref ddl, userInputValue.Trim()))
                {
                    foundError = true;
                }
            }
            else if (!CheckInputConsistentWithList(validator, userInputValue.Trim()))
            {
                foundError = true;
            }


        }


        if (foundError || !validator.IsInitialValueValid(value))
        {
            txtBx.ForeColor = System.Drawing.Color.Red;
        }



        HtmlGenericControl table = new HtmlGenericControl("table");
        HtmlGenericControl row = new HtmlGenericControl("tr");
        HtmlGenericControl firstCol = new HtmlGenericControl("td");
        HtmlGenericControl secondCol = new HtmlGenericControl("td");

        // By default labels length = 128px; Considered that we have max 15 chars.
        // For more char than 15 we add CharLength(7px) for each char.
        int labelLength = 228;
        if (lebalMaxChar > 35 && inputBoxMaxLength < MaxDisplayLength)
        {
            int availableSpace = MaxDisplayLength - inputBoxMaxLength;
            int extraLength = (lebalMaxChar - 35) * CharLength;

            if (extraLength <= availableSpace)
            {
                labelLength = labelLength + extraLength;
            }
            else
            {
                labelLength = labelLength + availableSpace;
            }
        }

        table.Attributes.Add("width", "100%");

        firstCol.Style.Add("width", labelLength + "px");

        firstCol.Attributes.Add("valign", "top");
        secondCol.Attributes.Add("valign", "top");

        table.Controls.Add(row);
        row.Controls.Add(firstCol);
        row.Controls.Add(secondCol);
        firstCol.Controls.Add(divLabels);
        secondCol.Controls.Add(divControl);


        pnlControl.Controls.Add(table);

        return idType;

    }

    Dictionary<string, string> GetFieldsWithValues()
    {
        Dictionary<string, string> fieldValues = new Dictionary<string, string>();
        Dictionary<string, PEValidator> validators = Session["validatorDictionary"] as Dictionary<string, PEValidator>;

        Dictionary<string, ControlType> controlDControlType = Session["controlIDsWithType"] as Dictionary<string, ControlType>;
        Dictionary<string, string> controlIDsFieldName = Session["controlIDs"] as Dictionary<string, string>;

        foreach (var item in controlIDsFieldName)
        {
            string fieldName = item.Value;
            string controlID = item.Key;
            ControlType cType = controlDControlType[controlID];
            PEValidator curValidator = validators[fieldName];
            DataType curType = (DataType)Enum.Parse(typeof(DataType), curValidator.ValueType, true);

            if (curType == DataType.Objarea || curType == DataType.Objlen || curValidator.AllowEdit.Equals(false))
            {
                continue;
            }


            switch (cType)
            {
                case ControlType.TextBox:
                    TextBox txtControl = Page.FindControl(controlID) as TextBox;
                    if (curType == DataType.Lastupdate)
                    {
                        fieldValues.Add(fieldName, DateTime.Today.ToString("yyyy-MM-dd"));
                    }
                    else if (curType == DataType.Date)
                    {
                        string date = DateTime.ParseExact(txtControl.Text, "dd-MM-yyyy", null).ToString("yyyy-MM-dd");
                        fieldValues.Add(fieldName, date);
                    }
                    else if (curType == DataType.Integer || curType == DataType.Float)
                    {
                        if (txtControl.Text.Equals(String.Empty))
                        {
                            fieldValues.Add(fieldName, "NULL");
                        }
                        else
                        {
                            fieldValues.Add(fieldName, txtControl.Text);
                        }
                    }
                    else
                    {
                        fieldValues.Add(fieldName, txtControl.Text);
                    }
                    break;
                case ControlType.DropDownList:
                    DropDownList ddl = Page.FindControl(controlID) as DropDownList;
                    string value = (Page.FindControl("txt" + controlID) as TextBox).Text;
                    if (curType == DataType.Date)
                    {
                        try
                        {
                            value = DateTime.ParseExact(value, "dd-MM-yyyy", null).ToString("yyyy-MM-dd");
                        }
                        catch { }

                    }

                    fieldValues.Add(fieldName, value);
                    break;

                default:

                    break;
            }
        }
        return fieldValues;
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        bool warningExist = false;
        List<string> warningFields = new List<string>();
        string error = IsAllFieldValid(ref warningExist, ref warningFields);
        if (Page.IsValid && error == "")
        {

            Dictionary<string, string> editedFieldsWithValues = GetFieldsWithValues();
            string fileLocation = Server.MapPath("Output") + "\\";
            if (Request.QueryString["file"] != null)
            {
                XMLHandler xmlHandler = new XMLHandler(fileLocation, Request.QueryString["file"].ToString());
                XElement xElement = Session["XElement"] as XElement;
                string fileName = xmlHandler.CreateXML(editedFieldsWithValues);
                lblError.Text = "";
                //lblError.ForeColor = System.Drawing.Color.Black;
                string fieldNames = string.Join(",", warningFields.ToArray<string>());

                if (warningExist && hdnConfirmation.Value == "")
                {
                    ScriptManager.RegisterStartupScript(Page, typeof(Page), "warning", "<script>$(document).ready(function(){mystring='Error exists in the following field(s):- \\n" + fieldNames + " !\\n Do you want to save...';GetComfirmation(mystring)});</script>", false);
                    //ScriptManager.RegisterStartupScript(Page, typeof(Page), "warning", "<script>mystring='Data saved with error of the following fields:- \\n" + fieldNames + " !';alert(mystring)</script>", false);
                }
                else
                {
                    hdnConfirmation.Value = "";
                    ScriptManager.RegisterStartupScript(Page, typeof(Page), "saveData", "<script>dataSaved=true;SaveData();</script>", false);
                }

            }
        }
        else
        {
            if (error != "")
                ScriptManager.RegisterStartupScript(Page, typeof(Page), "errorExists", "<script>$(document).ready(function(){MarkDropdownRedError();alert('" + error + "')});</script>", false);
        }
    }



    private string IsAllFieldValid(ref bool warningExist, ref List<string> warningField)
    {
        Dictionary<string, ControlType> controlDControlType = Session["controlIDsWithType"] as Dictionary<string, ControlType>;
        Dictionary<string, string> controlIDsFieldName = Session["controlIDs"] as Dictionary<string, string>;
        Dictionary<string, PEValidator> validators = Session["validatorDictionary"] as Dictionary<string, PEValidator>;

        bool returnValue = true;
        string errorMsg = "";

        foreach (var item in controlIDsFieldName)
        {
            string fieldName = item.Value;
            string controlID = item.Key;
            PEValidator curValidator = validators[fieldName];

            if (curValidator.AllowEdit.Equals(false))
            {
                continue;
            }

            ControlType cType = controlDControlType[controlID];
            string labelCaption = curValidator.Caption != "" ? curValidator.Caption : fieldName;
            labelCaption = labelCaption.ToFirstCharUpper();
            hdnErrorCtrID.Value = "";
            switch (cType)
            {
                case ControlType.TextBox:
                    TextBox txtControl = Page.FindControl(controlID) as TextBox;
                    string value = txtControl.Text;
                    if (curValidator.Mandatory == EnumMandatory.Lov)
                    {
                        if (curValidator.LOVValue != null)
                        {
                            bool isConsistent = CheckInputConsistentWithList(curValidator, value);
                            if (!isConsistent)
                            {
                                if (curValidator.ErrorLevel == 2)
                                {
                                    errorMsg = string.Format("In {0} field {1} is not consistent with the list", labelCaption, value);
                                    returnValue = false;
                                }
                                else
                                {
                                    curValidator.WanrningExist = true;
                                }
                            }

                        }
                        else if ((string)curValidator.ExtractedValue != value)
                        {
                            errorMsg = labelCaption + " value cannot be changed. No list of values definded";
                            returnValue = false;
                        }

                    }
                    if (returnValue && !curValidator.IsValidInput(value))
                    {
                        errorMsg = string.Format("In {0} field value {1} is not valid. {2}", labelCaption, value, curValidator.ErrorMsg);

                        txtControl.ForeColor = System.Drawing.Color.Red;
                        returnValue = false;
                    }


                    if (curValidator.WanrningExist)
                    {
                        warningExist = curValidator.WanrningExist;
                        warningField.Add(fieldName);
                        txtControl.ForeColor = System.Drawing.Color.Red;
                    }
                    else
                    {
                        txtControl.ForeColor = System.Drawing.Color.Black;
                    }
                    break;
                case ControlType.DropDownList:
                    DropDownList ddl = Page.FindControl(controlID) as DropDownList;
                    TextBox txtBxEditDropdown = Page.FindControl("txt" + controlID) as TextBox;
                    value = txtBxEditDropdown.Text;
                    if (curValidator.Mandatory == EnumMandatory.Lov)
                    {
                        bool isConsistent = CheckInputConsistentWithList(ref ddl, value);
                        if (!isConsistent)
                        {
                            if (curValidator.ErrorLevel == 2)
                            {
                                errorMsg = string.Format("In {0} field \"{1}\" is not consistent with the list", labelCaption, value);

                                returnValue = false;
                            }
                            else
                            {
                                curValidator.WanrningExist = true;
                            }
                        }

                    }
                    if (returnValue && !curValidator.IsValidInput(value))
                    {

                        errorMsg = string.Format("Invalid value in {0} field. {1}", labelCaption, curValidator.ErrorMsg);

                        returnValue = false;
                    }
                    if (curValidator.WanrningExist)
                    {
                        warningExist = curValidator.WanrningExist;
                        warningField.Add(fieldName);
                    }

                    break;

                default:
                    break;
            }

            if (!returnValue)
            {
                hdnErrorCtrID.Value = controlID;
                break;
            }
        }

        return returnValue ? "" : errorMsg;
    }

    protected void CustomValidator_Handler(object source, ServerValidateEventArgs args)
    {
        Dictionary<string, string> controlIDFieldNamePair = Session["controlIDs"] as Dictionary<string, string>;
        Dictionary<string, PEValidator> validatorList = Session["validatorDictionary"] as Dictionary<string, PEValidator>;
        CustomValidator cv = (CustomValidator)source;

        string fieldName = controlIDFieldNamePair[cv.ControlToValidate];
        PEValidator curValidator = validatorList[fieldName];
        if (!curValidator.IsValidInput(args.Value))
        {
            args.IsValid = false;
            cv.ErrorMessage = curValidator.ErrorMsg;
        }
    }

    private void CreatePickButton(string targetID, string fieldName, ref HtmlGenericControl div)
    {
        Button pickBtn = new Button();
        pickBtn.ID = string.Concat("pkBtn", fieldName);
        pickBtn.Style.Add("width", "20px");
        pickBtn.Text = "...";
        pickBtn.Attributes.Add("targetFieldId", targetID);
        pickBtn.Attributes.Add("inputFieldName", fieldName);
        pickBtn.Click += new EventHandler(pickBtn_Click);
        div.Controls.Add(pickBtn);
    }

    protected void pickBtn_Click(object sender, EventArgs e)
    {
        try
        {
            Session["filterDataTable"] = null;
            Session["GridDataTable"] = null;            

            Button pickBtn = (Button)sender;
            string targetFieldId = pickBtn.Attributes["targetFieldId"].ToString();
            string fieldName = pickBtn.Attributes["inputFieldName"].ToString();
            hdnCurTargetControlIdForGridvalue.Value = targetFieldId;

            Dictionary<string, PEValidator> validatorList = Session["validatorDictionary"] as Dictionary<string, PEValidator>;
            PEValidator curValidator = validatorList[fieldName];
            IDatabaseFunctionsManager db = DBManagerFactory.GetDBManager(activeDB, conStringLOV_SQL);

            DataTable dt = db.GetDataTable(curValidator.LOVValue);            
            DataTable dtCloned = CloneDataTableWithAllColumnStringDataType(dt);
            int pageSize = 30;
            if (Session["pageSize"] != null)
            {
                pageSize = Int32.Parse(Session["pageSize"].ToString());
            }
            txtPageSize.Text = pageSize.ToString();
            gvPopup.PageSize = pageSize;
            gvPopup.DataSource = dtCloned;
            Session["GridDataTable"] = dtCloned;
            gvPopup.DataBind();
            txtFilter.Text = string.Empty;
            modalGrid.Show();
        }
        catch (Exception ex)
        {
            ScriptManager.RegisterStartupScript(Page, typeof(Page), "errorExists", "<script>$(document).ready(function(){alert('Cannot load the grid.\\nSQL in the LOVP is not correct for this field or Connection string invalid.')});</script>", false);
        }

    }

    protected void btnPageSize_Click(object sender, EventArgs e)
    {
        try
        {
            gvPopup.PageSize = Int32.Parse(txtPageSize.Text);
            Session["pageSize"] = gvPopup.PageSize;

            if (txtFilter.Text.Equals(string.Empty))
            {
                DataTable dt = null;
                if (Session["filterDataTable"] != null)
                {
                    dt = Session["filterDataTable"] as DataTable;
                }
                else
                {
                    dt = Session["GridDataTable"] as DataTable;
                }
                
                gvPopup.DataSource = dt;
                gvPopup.DataBind();
            }
            else
            {
                btnFilter_Click(sender, e);
            }
        }
        catch (Exception ex)
        {
            ScriptManager.RegisterStartupScript(Page, typeof(Page), "error", "<script>alert(" + ex.Message + ");</script>", false);
        }
    }

    protected void btnFilter_Click(object sender, EventArgs e)
    {
        try
        {
            DataTable dt = Session["GridDataTable"] as DataTable;
            DataTable filterTable = new DataTable();
            DataColumn dc = new DataColumn(dt.Columns[0].ColumnName);
            filterTable.Columns.Add(dc);
            DataRow[] rows = dt.Select(dt.Columns[0].ColumnName + " like '%" + txtFilter.Text + "%'");
            foreach (DataRow row in rows)
            {
                DataRow newRow = filterTable.NewRow();
                newRow[dt.Columns[0].ColumnName] = row[0];
                filterTable.Rows.Add(newRow);
            }
            gvPopup.DataSource = filterTable;
            Session["filterDataTable"] = filterTable;
            gvPopup.DataBind();           
        }
        catch (Exception ex)
        {
            ScriptManager.RegisterStartupScript(Page, typeof(Page), "error", "<script>alert(" + ex.Message + ");</script>", false);
        }
    }

    protected void btnClearFilter_Click(object sender, EventArgs e)
    {
        try
        {
            txtFilter.Text = String.Empty;
            DataTable dt = Session["GridDataTable"] as DataTable;
            gvPopup.DataSource = dt;
            gvPopup.DataBind();
            Session["filterDataTable"] = null;
        }
        catch (Exception ex)
        {
            ScriptManager.RegisterStartupScript(Page, typeof(Page), "error", "<script>alert(" + ex.Message + ");</script>", false);
        }
    }

    private DataTable CloneDataTableWithAllColumnStringDataType(DataTable dt)
    {
        DataTable dtTmp = new DataTable();
        foreach (DataColumn dc in dt.Columns)
        {
            dtTmp.Columns.Add(dc.ColumnName.ToFirstCharUpper());
        }

        foreach (DataRow record in dt.Rows)
        {
            DataRow dr = dtTmp.NewRow();
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                if (record.Table.Columns[i].DataType.FullName == "System.DateTime")
                {
                    dr[i] = record[i].ToString() != "" ? Convert.ToDateTime(record[i]).ToString("dd-MM-yyyy") : "";

                }
                else
                {
                    dr[i] = record[i].ToString();
                }
            }

            dtTmp.Rows.Add(dr);
        }

        return dtTmp;

    }

    bool CheckInputConsistentWithList(ref DropDownList ddl, string value)
    {
        for (int i = 0; i < ddl.Items.Count; i++)
        {
            if (ddl.Items[i].Value.ToLower() == value.ToLower())
            {
                return true;
            }
        }

        return false;
    }

    bool CheckInputConsistentWithList(PEValidator validator, string value)
    {
        IDatabaseFunctionsManager db = DBManagerFactory.GetDBManager(activeDB, conStringLOV_SQL);
        DataTable dt = db.GetDataTable(validator.LOVValue);
        string fieldName = validator.FieldName;
        DataType curType = (DataType)Enum.Parse(typeof(DataType), validator.ValueType, true);
        bool fieldExists = false;
        foreach (DataColumn dc in dt.Columns)
        {
            if (dc.ColumnName.ToLower() == fieldName.ToLower())
            {
                fieldExists = true;
                break;
            }
        }

        if (fieldExists)
        {
            foreach (DataRow record in dt.Rows)
            {
                if (curType == DataType.Date)
                {
                    if (record[fieldName].ToString().Trim() != Convert.ToDateTime(record[fieldName]).ToString("dd-MM-yyyy"))
                    {
                        return true;
                    }
                }
                else
                {
                    if (record[fieldName].ToString() == value)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    protected void btnSelectValue_Click(object sender, EventArgs e)
    {
        if (gvPopup.SelectedIndex >= 0)
        {
            int selectedIndex = gvPopup.PageSize * gvPopup.PageIndex + gvPopup.SelectedIndex;
            DataTable dataTable = null;
            if (Session["filterDataTable"] != null)
            {
                dataTable = Session["filterDataTable"] as DataTable;
            }
            else
            {
                dataTable = Session["GridDataTable"] as DataTable;
            }            
            DataRow selectedRow = dataTable.Rows[selectedIndex];

            string controlID = hdnCurTargetControlIdForGridvalue.Value;
            Dictionary<string, string> idFieldNamePair = Session["controlIDs"] as Dictionary<string, string>;
            string fieldName = idFieldNamePair[controlID];
            string newValue = "";

            int cellIndex = dataTable.Columns.IndexOf(fieldName);

            if (IsColumnExistInTable(dataTable, fieldName))
            {                
                newValue = dataTable.Rows[selectedIndex][cellIndex].ToString() ;
            }
            else
            {
                newValue = selectedRow[0].ToString();
                if (controlID.StartsWith("ddl"))
                {
                    controlID = "txt" + controlID;
                }
            }

            SetNewValueToControl(controlID, newValue);
            modalGrid.Hide();
            gvPopup.SelectedIndex = -1;
            ScriptManager.RegisterStartupScript(Page, typeof(Page), "select", "<script> GetDDLSelectedValueToTextBox();</script>", false);

        }
    }

    private bool IsColumnExistInTable(DataTable dt, string colName)
    {
        foreach (DataColumn dc in dt.Columns)
        {
            if (dc.ColumnName.ToLower() == colName.ToLower())
            {
                return true;
            }
        }

        return false;
    }

    private void SetNewValueToControl(string controlID, string value)
    {
        ScriptManager.RegisterStartupScript(Page, typeof(Page), "newValue", "<script>document.getElementById('" + controlID + "').value = '" + value + "';</script>", false);

    }

    protected void gvPopup_Sorting(object sender, GridViewSortEventArgs e)
    {
        if (ViewState["SortExpression"] == null || ViewState["SortExpression"].ToString() != e.SortExpression)
        {
            ViewState["SortExpression"] = e.SortExpression;
            ViewState["SortDirection"] = "ASC";
        }
        else
        {
            if (ViewState["SortDirection"].ToString() == "ASC")
            {
                ViewState["SortDirection"] = "DESC";
            }
            else
            {
                ViewState["SortDirection"] = "ASC";
            }
        }

        DataTable dataTable = Session["GridDataTable"] as DataTable;

        if (dataTable != null)
        {
            DataView dataView = new DataView(dataTable);
            dataView.Sort = ViewState["SortExpression"].ToString() + " " + ViewState["SortDirection"].ToString();

            gvPopup.DataSource = dataView;
            gvPopup.DataBind();
            gvPopup.SelectedIndex = -1;
            Session["GridDataTable"] = dataView.ToTable();
        }
    }

    protected void gvPopup_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvPopup.PageIndex = e.NewPageIndex;
        if (Session["filterDataTable"] != null)
        {
            gvPopup.DataSource = Session["filterDataTable"] as DataTable;
        }
        else
        {
            gvPopup.DataSource = Session["GridDataTable"] as DataTable;
        } 
        gvPopup.DataBind();
        gvPopup.SelectedIndex = -1;
    }

    protected void gvPopup_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            e.Row.Attributes.Add("onmouseover", "this.style.cursor='pointer';this.style.textdecoration='underline';");
            e.Row.Attributes.Add("onmouseout", "this.style.textDecoration='none';");
            e.Row.Attributes.Add("onclick", Page.ClientScript.GetPostBackClientHyperlink(gvPopup, "Select$" + e.Row.RowIndex.ToString()));
        }
    }

    protected void btnClose_Click(object sender, EventArgs e)
    {
        modalGrid.Hide();
    }
}
