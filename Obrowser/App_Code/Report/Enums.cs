using System;
using System.Data;
using System.Configuration;

using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;


/// <summary>
/// EOperation determines the operation
/// on column data
/// </summary>

public enum EOperation
{
    Add,
    Sum,
    Average,
    Min,
    Max,
    None
}

/// <summary>
/// EAlign determines the alignment
/// </summary>
public enum EAlign
{ 
    Left,
    Right,
    Center
}

/// <summary>
/// EPageSize determines the report page size.
/// </summary>
public enum EPageSize
{ 
    A4,
    A3
}

/// <summary>
/// EPaperFormat determines the paper orientation
/// </summary>
public enum EPaperFormat
{
    Portrait,
    LandScape
}
