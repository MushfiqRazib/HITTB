using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using PdfSharp.Drawing;

/// <summary>
/// Summary description for Group
/// </summary>
public class Group
{
    private string _GroupName = string.Empty;
    private double _GroupValue = 0;
    private XColor _GroupColour;
    
    public Group()
	{
		//
		// TODO: Add constructor logic here
		//
	}

    public string GroupName
    {
        get { return _GroupName; }
        set { _GroupName = value; }
    }

    public double GroupValue
    {
        get { return _GroupValue; }
        set { _GroupValue = value; }
       
    }

    public XColor GroupColour
    {
        get { return _GroupColour; }
        set { _GroupColour = value; }
    }
}
