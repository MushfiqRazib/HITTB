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
/// FileNameManager provides the file names
/// </summary>
public class FileNameManager
{
    #region Member variables

    private static string _PdfFileName = "OB_Report.pdf";

    #endregion

    #region Properties

    /// <summary>
    /// PdfFileName provides the file name which will contain
    /// all report data.
    /// </summary>
    public static string PdfFileName
    {
        get { return _PdfFileName; }
        set { _PdfFileName = value; }
    }

    #endregion
}
