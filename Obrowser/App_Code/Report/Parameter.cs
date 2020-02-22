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
/// Parameter calss is used for report parameters
/// </summary>
public class Parameter
{
    #region Member variables

    private string _Name;
    private string _Value;
    EAlign _Align;

    #endregion

    #region Contructor

    /// <summary>
    /// Contruct a report parameter with name, value & alignment of a parameter
    /// </summary>
    /// <param name="Name"></param>
    /// <param name="Value"></param>
    /// <param name="Align"></param>
    public Parameter(string Name, string Value, EAlign Align)
    {
        _Name = Name;
        _Value = Value;
        _Align = Align;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Name provides the parameter name
    /// </summary>
    public string Name
    {
        get { return _Name; }
        set { _Name = value; }
    }

    /// <summary>
    /// Name provides the parameter value
    /// </summary>
    public string Value
    {
        get { return _Value; }
        set { _Value = value; }
    }

    /// <summary>
    /// Align provides the parameter alignment
    /// </summary>
    public EAlign Align
    {
        get { return _Align; }
        set { _Align = value; }
    }

    #endregion
}