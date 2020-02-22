using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

public enum DataType
{
    Integer,
    Float,
    String,
    Date,
    Objlen,
    Objarea,
    Lastupdate,
    Text
}

public enum EnumMandatory
{
    Yes,
    No,
    Lov
}

public enum EnumLovType
{
    Lovc,
    Lovp,
    Lovcp,
    None
}

public enum EnumErrorType
{
    NoError,
    StorageTypeError,
    ConstraintError
}


/// <summary>
/// Summary description for PEValidator
/// </summary>
public class PEValidator
{
    public PEValidator()
    {
        this.ErrorMsg = string.Empty;
        this.WanrningExist = false;
    }

    public string ErrorMsg
    {
        get;
        set;
    }

    public string ValueType
    {
        get;
        set;
    }

    public EnumMandatory Mandatory
    {
        get;
        set;
    }

    public object DefaultValue
    {
        get;
        set;
    }

    /// <summary>
    /// The value that has been read from xml file. Value may be empty.
    /// </summary>
    public object ExtractedValue
    {
        get;
        set;
    }

    public string LOVValue
    {
        get;
        set;
    }

    public EnumLovType LovType
    {
        get;
        set;
    }

    public string FieldName
    {
        get;
        set;
    }

    public string Caption
    {
        set;
        get;
    }

    public int MinValueInt
    {
        get;
        set;
    }

    public int MaxValueInt
    {
        get;
        set;
    }

    public double MinValueFloat
    {
        get;
        set;
    }

    public double MaxValueFloat
    {
        get;
        set;
    }

    public int MaxStringLength
    {
        get;
        set;
    }

    public int MaxDecimals
    {
        get;
        set;
    }

    public string ToolTip
    {
        get;
        set;
    }

    public bool AllowEdit
    {
        get;
        set;
    }

    public int ErrorLevel
    {
        get;
        set;
    }

    public EnumErrorType ErrorType
    {
        get;
        set;
    }

    public bool WanrningExist
    {
        get;
        set;
    }

    public int DisplayLength
    {
        get;
        set;
    }

    public int ActualLength
    {
        get;
        set;
    }

    public int LabelCharCount
    {
        get;
        set;
    }

    public bool IsInitialValueValid(object value)
    {
        bool returnValue = true;
        if (value.ToString().Trim() != string.Empty)
        {
            DataType curType = (DataType)Enum.Parse(typeof(DataType), this.ValueType, true);
            switch (curType)
            {
                case DataType.Integer:
                    int inputValueInt;
                    bool isInteger = int.TryParse(value as string, out inputValueInt);
                    if (isInteger)
                    {
                        returnValue = ProcessIntegerValidity(inputValueInt);
                    }
                    else
                    {
                        this.ErrorMsg = "Invalid Integer";
                        this.ErrorType = EnumErrorType.StorageTypeError;
                        returnValue = false;
                    }
                    break;
                case DataType.Float:
                    float inputValueFlt;

                    bool isFloat = float.TryParse(value as string, out inputValueFlt);
                    if (isFloat)
                    {
                        returnValue = ProcessFloatValidity(inputValueFlt);
                    }
                    else
                    {
                        this.ErrorMsg = "Invalid Float";
                        this.ErrorType = EnumErrorType.StorageTypeError;
                        returnValue = false;
                    }
                    break;
                case DataType.String:                
                    string inputValueStr = value.ToString();
                    returnValue = ProcessStringValidity(inputValueStr);
                    break;
                case DataType.Date:
                    string inputValueDate = value.ToString();
                    returnValue = ProcessDateValidity(inputValueDate);
                    break;
                case DataType.Lastupdate:
                    returnValue = true;
                    break;
                case DataType.Objlen:
                case DataType.Objarea:
                    returnValue = true;
                    break;
                case DataType.Text:
                    returnValue = true;
                    break;
                default:
                    break;
            }
        }
        else if (this.Mandatory == EnumMandatory.Yes)
        {
            this.ErrorMsg = "Cannot be empty";
            returnValue = false;
        }
       

        return returnValue;
    }

    public bool IsValidInput(object value)
    {
        bool returnValue = false;
        if (value.ToString().Trim() != string.Empty)
        {
            DataType curType = (DataType)Enum.Parse(typeof(DataType), this.ValueType, true);
            switch (curType)
            {
                case DataType.Integer:
                    int inputValueInt;
                    bool isInteger = int.TryParse(value as string, out inputValueInt);
                    if (isInteger)
                    {
                        returnValue = ProcessIntegerValidity(inputValueInt);
                    }
                    else
                    {
                        this.ErrorMsg = "Invalid Integer";
                        this.ErrorType = EnumErrorType.StorageTypeError;
                        returnValue = false;
                    }
                    break;
                case DataType.Float:
                    float inputValueFlt;
                    bool isFloat = float.TryParse(value as string, out inputValueFlt);
                    if (isFloat)
                    {
                        returnValue = ProcessFloatValidity(inputValueFlt);
                    }
                    else
                    {
                        this.ErrorMsg = "Invalid Float";
                        this.ErrorType = EnumErrorType.StorageTypeError;
                        returnValue = false;
                    }
                    break;
                case DataType.String:
                    string inputValueStr = value.ToString();
                    returnValue = ProcessStringValidity(inputValueStr);
                    break;
                case DataType.Date:
                    string inputValueDate = value.ToString();
                    returnValue = ProcessDateValidity(inputValueDate);
                    break;
                case DataType.Lastupdate:
                    returnValue = true;
                    break;
                case DataType.Objlen:
                case DataType.Objarea:
                    returnValue = true;
                    break;

                default:
                    break;
            }
        }
        else if (this.Mandatory == EnumMandatory.Yes)
        {
            this.ErrorMsg = "Cannot be empty";
            returnValue = false;
        }
        else if (this.Mandatory == EnumMandatory.Lov)
        {
            this.ErrorMsg = "Empty value not allowed.";
            returnValue = false;
        }
        else if (this.Mandatory == EnumMandatory.No)
        {
            returnValue = true;
        }

        if (this.WanrningExist != true)
        {
            this.WanrningExist = false;
        }


        if (this.ErrorLevel == 0)
        {
            if (returnValue == false && this.ErrorType == EnumErrorType.ConstraintError)
            {
                this.WanrningExist = !returnValue;
            }
            return true;
        }
        else if (this.ErrorLevel == 1)
        {
            if (returnValue == false)
            {
                this.WanrningExist = true;
                if (this.ErrorType == EnumErrorType.ConstraintError || this.ErrorType == EnumErrorType.NoError)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else return true;

        }
        else
        {
            if (returnValue == false)
            {
                this.WanrningExist = true;
                return false;

            }
            else return true;
        }

    }

    private bool ProcessStringValidity(string value)
    {
        if (this.MaxStringLength > 0)
        {
            if (value.Length <= MaxStringLength)
            {
                return true;
            }
            else
            {
                this.ErrorMsg = string.Format("String length cannot be more than({0})", this.MaxStringLength);
                this.ErrorType = EnumErrorType.ConstraintError;
                return false;
            }
        }
        else return true;


    }

    private bool ProcessFloatValidity(float value)
    {

        if (this.MinValueFloat >= 0 && this.MinValueFloat > 0)
        {
            if (value >= this.MinValueFloat)
            {
                if (value <= this.MaxValueFloat)
                {
                    int decimalLength = value.ToString().Contains('.') ? value.ToString().Split('.')[1].Length : 0;
                    if (decimalLength <= this.MaxDecimals)
                        return true;
                    else
                    {
                        this.ErrorMsg = string.Format("Float decimal digit cannot be more than ({0})", this.MaxDecimals);
                        this.ErrorType = EnumErrorType.ConstraintError;
                        return false;
                    }
                }
                else
                {
                    this.ErrorMsg = string.Format("Float cannot be more than maximum value ({0})", this.MaxValueFloat);
                    this.ErrorType = EnumErrorType.ConstraintError;
                    return false;
                }
            }
            else
            {
                this.ErrorMsg = string.Format("Float cannot be less than minimum value ({0})", this.MinValueFloat);
                this.ErrorType = EnumErrorType.ConstraintError;
                return false;
            }
        }


        if (this.MaxDecimals > 0)
        {
            int decimalLength = value.ToString().Split('.')[1].Length;
            if (decimalLength <= MaxDecimals)
                return true;
            else
            {
                this.ErrorMsg = string.Format("Float decimal digit cannot be more than ({0})", this.MaxValueFloat);
                this.ErrorType = EnumErrorType.ConstraintError;
                return false;
            }
        }
        else
        {
            return true;
        }



    }

    private bool ProcessIntegerValidity(int value)
    {

        if (this.MinValueInt >= 0 && this.MaxValueInt > 0)
        {
            if (value >= this.MinValueInt)
            {
                if (value <= this.MaxValueInt)
                {
                    return true;
                }
                else
                {
                    this.ErrorMsg = string.Format("Integer cannot be more than maximum value ({0})", this.MaxValueInt);
                    this.ErrorType = EnumErrorType.ConstraintError;
                    return false;
                }
            }
            else
            {
                this.ErrorMsg = string.Format("Integer cannot be less than minimum value ({0})", this.MinValueInt);
                this.ErrorType = EnumErrorType.ConstraintError;
                return false;
            }
        }
        else return true;



    }

    private bool ProcessDateValidity(string inputDate)
    {

        DateTime date = new DateTime();
        try
        {
            date = DateTime.ParseExact(inputDate, "dd-MM-yyyy", null);
            return true;
        }
        catch (Exception exp)
        {
            this.ErrorMsg = exp.Message;
            this.ErrorType = EnumErrorType.StorageTypeError;
            return false;
        }





    }
}
