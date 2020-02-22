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


public enum ControlType
{
    TextBox,
    DropDownList,
    CheckBox,
    None
}

public struct ControlIDWithType
{
    public string ID;
    public ControlType Type;
}
public class Common
{
    public Common()
    {
        //
    }
}

public static class MyExtension
{
    public static string ToFirstCharUpper(this string myString)
    {
        return char.ToUpper(myString[0]) + myString.Substring(1).ToLower();
    }
}
